using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace BiliLite.Player.RangeDownload
{
    /// <summary>
    /// 多线程分段下载
    /// 把一个音视频文件切成小块，从多个节点、多条连接同时下载，并在播放位置前面提前缓存一段，
    /// 改善海外观看时单条连接太慢导致的转圈、起播慢、拖动后等很久。
    /// 思路参考 Bilibili 线程撕裂者（MIT）：https://github.com/MrTangLuyao/Bilibili-thread-ripper
    /// </summary>
    public sealed class MultiThreadRangeDownloader : IDisposable
    {
        /// <summary>
        /// 可选的线程数
        /// </summary>
        public static readonly IReadOnlyList<int> ThreadOptions = new[] { 4, 8, 16, 32 };

        /// <summary>
        /// 每块大小
        /// </summary>
        private const int BLOCK_SIZE = 256 * 1024;

        /// <summary>
        /// 播放位置之前保留的块数，小幅回退时不用重下
        /// </summary>
        private const int KEEP_BEHIND_BLOCKS = 8;

        /// <summary>
        /// 每块最多尝试次数
        /// </summary>
        private const int MAX_ATTEMPTS = 4;

        /// <summary>
        /// 提前缓存的秒数
        /// </summary>
        private const int AHEAD_SECONDS = 30;

        /// <summary>
        /// 节点连续失败几次后暂停使用
        /// </summary>
        private const int STRIKE_LIMIT = 2;

        /// <summary>
        /// 节点多久没有响应算失败
        /// </summary>
        private static readonly TimeSpan FirstByteTimeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// 传输中途多久没有新数据算失败
        /// </summary>
        private static readonly TimeSpan StallTimeout = TimeSpan.FromSeconds(4);

        /// <summary>
        /// 单次请求最长时间
        /// </summary>
        private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(20);

        /// <summary>
        /// 播放器急需的块超过这个时间还没下完，就换个节点再发一份，谁先到用谁
        /// </summary>
        private static readonly TimeSpan HedgeDelay = TimeSpan.FromSeconds(1.5);

        /// <summary>
        /// 还没有节点测过速时（起播），再发一份的等待时间
        /// </summary>
        private static readonly TimeSpan StartupHedgeDelay = TimeSpan.FromSeconds(0.5);

        /// <summary>
        /// 播放器的一次请求最多等这么久，超时交还给播放器自己下载
        /// </summary>
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// 节点暂停使用的时长，过后再试
        /// </summary>
        private static readonly TimeSpan BanDuration = TimeSpan.FromSeconds(60);

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private readonly string m_name;
        private readonly List<RangeSource> m_sources;
        private readonly HttpClient m_httpClient;
        private readonly int m_concurrency;
        private readonly long m_aheadBlocks;
        private readonly CancellationTokenSource m_disposeCts = new CancellationTokenSource();
        private readonly object m_lock = new object();
        private readonly Dictionary<long, RangeBlock> m_blocks = new Dictionary<long, RangeBlock>();
        private long m_totalLength = -1;
        private long m_playheadBlock;
        private int m_running;
        private bool m_disposed;

        private MultiThreadRangeDownloader(string name, List<RangeSource> sources, int concurrency, long aheadBytes,
            string userAgent, string referer)
        {
            m_name = name;
            m_sources = sources;
            m_concurrency = concurrency;
            m_aheadBlocks = Math.Max(1, aheadBytes / BLOCK_SIZE);

            var filter = new HttpBaseProtocolFilter();
            // HTTP/2 会把发往同一节点的请求挤进一条连接，多线程就没有意义了
            filter.MaxVersion = HttpVersion.Http11;
            filter.MaxConnectionsPerServer = (uint)Math.Max(6, concurrency);
            filter.AutomaticDecompression = false;
            // 视频数据只放在内存里，不写进系统的网络缓存，避免磁盘占用
            filter.CacheControl.ReadBehavior = HttpCacheReadBehavior.NoCache;
            filter.CacheControl.WriteBehavior = HttpCacheWriteBehavior.NoCache;
            m_httpClient = new HttpClient(filter);
            if (!string.IsNullOrEmpty(userAgent))
            {
                m_httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }
            if (!string.IsNullOrEmpty(referer))
            {
                m_httpClient.DefaultRequestHeaders.Add("Referer", referer);
            }
        }

        /// <summary>
        /// 按设置为一路音频或视频创建下载器，未开启或地址不适用时返回 null
        /// </summary>
        public static MultiThreadRangeDownloader TryCreate(BiliDashItem item, string userAgent, string referer)
        {
            if (item == null) return null;
            if (!SettingService.GetValue(SettingConstants.Player.MULTI_THREAD_DOWNLOAD,
                    SettingConstants.Player.DEFAULT_MULTI_THREAD_DOWNLOAD))
            {
                return null;
            }
            if (!BiliVideoCdn.IsBiliMediaUrl(item.Url)) return null;

            var mode = (MultiThreadCdnMode)SettingService.GetValue(SettingConstants.Player.MULTI_THREAD_DOWNLOAD_CDN_MODE,
                (int)SettingConstants.Player.DEFAULT_MULTI_THREAD_DOWNLOAD_CDN_MODE);
            var threads = SettingService.GetValue(SettingConstants.Player.MULTI_THREAD_DOWNLOAD_THREADS,
                SettingConstants.Player.DEFAULT_MULTI_THREAD_DOWNLOAD_THREADS);
            if (!ThreadOptions.Contains(threads))
            {
                threads = SettingConstants.Player.DEFAULT_MULTI_THREAD_DOWNLOAD_THREADS;
            }

            var sources = BiliVideoCdn.BuildSources(item.Url, item.BackupUrls, mode);
            if (sources.Count == 0) return null;

            // 音频码率低，分到的线程少一些
            var concurrency = item.IsVideo ? threads : Math.Max(2, threads / 4);
            var minAhead = item.IsVideo ? 4L * 1024 * 1024 : 1L * 1024 * 1024;
            var maxAhead = item.IsVideo ? 96L * 1024 * 1024 : 16L * 1024 * 1024;
            var aheadBytes = Math.Min(maxAhead, Math.Max(minAhead, (long)item.BandWidth / 8 * AHEAD_SECONDS));
            var name = item.IsVideo ? "视频" : "音频";

            _logger.Info($"{name}多线程下载：{sources.Count}个节点，{concurrency}线程，模式{mode}");
            return new MultiThreadRangeDownloader(name, sources, concurrency, aheadBytes, userAgent, referer);
        }

        /// <summary>
        /// 接管 AdaptiveMediaSource 的一次分段下载。
        /// 下载失败时不设置结果，播放器会照常自己下载
        /// </summary>
        public async void HandleDownloadRequested(AdaptiveMediaSourceDownloadRequestedEventArgs args)
        {
            // async void 里漏出去的异常会成为程序级的未处理异常，这里全部兜住
            AdaptiveMediaSourceDownloadRequestedDeferral deferral = null;
            try
            {
                var offset = args.ResourceByteRangeOffset;
                var length = args.ResourceByteRangeLength;
                if (offset == null || length == null || length.Value == 0) return;
                lock (m_lock)
                {
                    if (m_disposed) return;
                }

                deferral = args.GetDeferral();
                var read = ReadAsync((long)offset.Value, (long)length.Value);
                var timeout = Task.Delay(RequestTimeout, m_disposeCts.Token);
                if (await Task.WhenAny(read, timeout) == read)
                {
                    args.Result.Buffer = await read;
                }
                else
                {
                    Observe(read);
                    if (!m_disposeCts.IsCancellationRequested)
                    {
                        _logger.Warn($"{m_name}多线程下载超时，交给播放器下载");
                    }
                }
            }
            catch (Exception ex)
            {
                if (!m_disposeCts.IsCancellationRequested)
                {
                    _logger.Warn($"{m_name}多线程下载失败，交给播放器下载", ex);
                }
            }
            finally
            {
                try
                {
                    deferral?.Complete();
                }
                catch (Exception)
                {
                    // 切换视频时播放器已经关闭，这一段的结果不需要再交了
                }
            }
        }

        /// <summary>
        /// 读取文件中的一段，同时安排后面的块提前下载
        /// </summary>
        public async Task<IBuffer> ReadAsync(long start, long length)
        {
            var end = start + length - 1;
            var needed = new List<RangeBlock>();
            long firstBlock, lastBlock;
            lock (m_lock)
            {
                if (m_disposed) throw new ObjectDisposedException(nameof(MultiThreadRangeDownloader));
                // 超出文件末尾的部分不下载
                if (m_totalLength > 0 && end >= m_totalLength)
                {
                    end = m_totalLength - 1;
                }
                if (end < start) throw new ArgumentOutOfRangeException(nameof(start), "超出文件长度");
                firstBlock = start / BLOCK_SIZE;
                lastBlock = end / BLOCK_SIZE;
                m_playheadBlock = firstBlock;
                for (var index = firstBlock; index <= lastBlock; index++)
                {
                    needed.Add(GetOrAddBlockLocked(index, true));
                }
                for (var index = lastBlock + 1; index <= lastBlock + m_aheadBlocks && !IsBeyondEndLocked(index); index++)
                {
                    GetOrAddBlockLocked(index, false);
                }
                TrimLocked(firstBlock - KEEP_BEHIND_BLOCKS, lastBlock + m_aheadBlocks);
            }
            Pump();

            var parts = await Task.WhenAll(needed.Select(x => x.Completion.Task)).ConfigureAwait(false);

            lock (m_lock)
            {
                if (m_totalLength > 0 && end >= m_totalLength)
                {
                    end = m_totalLength - 1;
                }
            }
            var result = new byte[end - start + 1];
            long written = 0;
            for (var i = 0; i < parts.Length; i++)
            {
                var blockStart = (firstBlock + i) * BLOCK_SIZE;
                var data = parts[i];
                var from = Math.Max(start, blockStart);
                var to = Math.Min(end, blockStart + data.Length - 1);
                if (to < from) break;
                Array.Copy(data, from - blockStart, result, written, to - from + 1);
                written += to - from + 1;
            }
            if (written != result.Length)
            {
                throw new InvalidOperationException($"分段长度不符：{written}/{result.Length}");
            }
            return result.AsBuffer();
        }

        public void Dispose()
        {
            List<RangeBlock> blocks;
            lock (m_lock)
            {
                if (m_disposed) return;
                m_disposed = true;
                blocks = m_blocks.Values.ToList();
                m_blocks.Clear();
            }
            m_disposeCts.Cancel();
            foreach (var block in blocks)
            {
                block.Completion.TrySetCanceled();
            }
            m_httpClient.Dispose();
        }

        private RangeBlock GetOrAddBlockLocked(long index, bool urgent)
        {
            if (!m_blocks.TryGetValue(index, out var block))
            {
                block = new RangeBlock(index);
                m_blocks[index] = block;
            }
            if (urgent)
            {
                block.Urgent = true;
            }
            return block;
        }

        private bool IsBeyondEndLocked(long index)
        {
            return m_totalLength > 0 && index * BLOCK_SIZE >= m_totalLength;
        }

        /// <summary>
        /// 丢掉窗口外的块：已下好的释放内存，还没开始的不再下载。
        /// 播放器正在等的块和正在下的块留着
        /// </summary>
        private void TrimLocked(long keepFrom, long keepTo)
        {
            foreach (var block in m_blocks.Values.ToList())
            {
                if (block.Index >= keepFrom && block.Index <= keepTo) continue;
                if (block.State == RangeBlockState.Running) continue;
                if (block.State == RangeBlockState.Pending && block.Urgent) continue;
                m_blocks.Remove(block.Index);
                block.Completion.TrySetCanceled();
            }
        }

        private void Pump()
        {
            var started = new List<RangeBlock>();
            lock (m_lock)
            {
                while (!m_disposed && m_running < m_concurrency)
                {
                    var next = PickNextBlockLocked();
                    if (next == null) break;
                    next.State = RangeBlockState.Running;
                    m_running++;
                    started.Add(next);
                }
            }
            foreach (var block in started)
            {
                _ = Task.Run(() => RunBlockAsync(block));
            }
        }

        /// <summary>
        /// 播放器在等的块先下，其余按离播放位置的远近
        /// </summary>
        private RangeBlock PickNextBlockLocked()
        {
            RangeBlock best = null;
            long bestKey = long.MaxValue;
            foreach (var block in m_blocks.Values)
            {
                if (block.State != RangeBlockState.Pending) continue;
                var distance = block.Index >= m_playheadBlock
                    ? block.Index - m_playheadBlock
                    : (1L << 40) + m_playheadBlock - block.Index;
                var key = block.Urgent ? distance : (1L << 50) + distance;
                if (key < bestKey)
                {
                    best = block;
                    bestKey = key;
                }
            }
            return best;
        }

        private async Task RunBlockAsync(RangeBlock block)
        {
            try
            {
                var data = await DownloadBlockAsync(block).ConfigureAwait(false);
                lock (m_lock)
                {
                    block.State = RangeBlockState.Done;
                }
                block.Completion.TrySetResult(data);
            }
            catch (Exception ex)
            {
                lock (m_lock)
                {
                    block.State = RangeBlockState.Failed;
                    if (m_blocks.TryGetValue(block.Index, out var current) && current == block)
                    {
                        m_blocks.Remove(block.Index);
                    }
                }
                if (ex is OperationCanceledException)
                {
                    block.Completion.TrySetCanceled();
                }
                else
                {
                    block.Completion.TrySetException(ex);
                }
            }
            finally
            {
                lock (m_lock)
                {
                    m_running--;
                }
                Pump();
            }
        }

        private async Task<byte[]> DownloadBlockAsync(RangeBlock block)
        {
            var start = block.Index * BLOCK_SIZE;
            var end = start + BLOCK_SIZE - 1;
            lock (m_lock)
            {
                if (m_totalLength > 0)
                {
                    if (start >= m_totalLength) throw new InvalidOperationException("超出文件长度");
                    end = Math.Min(end, m_totalLength - 1);
                }
            }

            var tried = new List<RangeSource>();
            Exception lastError = null;
            for (var attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                m_disposeCts.Token.ThrowIfCancellationRequested();
                var source = PickSource(tried, block.Urgent);
                if (source == null) break;
                tried.Add(source);
                try
                {
                    return await FetchWithHedgeAsync(block, source, tried, start, end).ConfigureAwait(false);
                }
                catch (RangeSourceException ex) when (ex.StatusCode == 416)
                {
                    throw;
                }
                catch (Exception ex) when (!m_disposeCts.IsCancellationRequested)
                {
                    lastError = ex;
                }
            }
            throw lastError ?? new InvalidOperationException("没有可用的视频节点");
        }

        /// <summary>
        /// 下载一块；播放器在等这一块而它迟迟没到时，换个节点再发一份
        /// </summary>
        private async Task<byte[]> FetchWithHedgeAsync(RangeBlock block, RangeSource source, List<RangeSource> tried,
            long start, long end)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(m_disposeCts.Token))
            {
                var primary = FetchAsync(source, start, end, cts.Token);
                var stopwatch = Stopwatch.StartNew();
                while (!primary.IsCompleted)
                {
                    // 刚开始还不知道哪个节点快，起播的块早一点找第二个节点
                    var hedgeDelay = HasTestedSource() ? HedgeDelay : StartupHedgeDelay;
                    if (block.Urgent && stopwatch.Elapsed >= hedgeDelay) break;
                    await Task.WhenAny(primary, Task.Delay(250, cts.Token)).ConfigureAwait(false);
                }
                if (primary.IsCompleted)
                {
                    return await primary.ConfigureAwait(false);
                }

                var hedgeSource = PickSource(tried, true);
                if (hedgeSource == null)
                {
                    return await primary.ConfigureAwait(false);
                }
                tried.Add(hedgeSource);
                var hedge = FetchAsync(hedgeSource, start, end, cts.Token);
                var tasks = new List<Task<byte[]>> { primary, hedge };
                while (true)
                {
                    var done = await Task.WhenAny(tasks).ConfigureAwait(false);
                    tasks.Remove(done);
                    if (done.Status == TaskStatus.RanToCompletion || tasks.Count == 0)
                    {
                        cts.Cancel();
                        foreach (var loser in tasks)
                        {
                            Observe(loser);
                        }
                        return await done.ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task<byte[]> FetchAsync(RangeSource source, long start, long end, CancellationToken token)
        {
            lock (m_lock)
            {
                source.Active++;
            }
            var stopwatch = Stopwatch.StartNew();
            try
            {
                using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(token))
                using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(source.Url)))
                {
                    timeout.CancelAfter(AttemptTimeout);
                    request.Headers.TryAppendWithoutValidation("Range", $"bytes={start}-{end}");
                    HttpResponseMessage firstResponse;
                    using (var firstByte = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token))
                    {
                        firstByte.CancelAfter(FirstByteTimeout);
                        try
                        {
                            firstResponse = await m_httpClient
                                .SendRequestAsync(request, HttpCompletionOption.ResponseHeadersRead)
                                .AsTask(firstByte.Token).ConfigureAwait(false);
                        }
                        catch (Exception) when (firstByte.IsCancellationRequested && !token.IsCancellationRequested)
                        {
                            throw new RangeSourceException(0, "节点没有响应");
                        }
                    }
                    using (var response = firstResponse)
                    {
                        // 文件长度还不知道时，提前下载的块可能超出文件末尾，这不算节点的错
                        if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable)
                        {
                            var totalLength = response.Content.Headers.ContentRange?.Length;
                            if (totalLength != null)
                            {
                                lock (m_lock)
                                {
                                    if (m_totalLength < 0) m_totalLength = (long)totalLength.Value;
                                }
                            }
                            throw new RangeSourceException(416, "超出文件长度");
                        }

                        if (response.StatusCode != HttpStatusCode.PartialContent)
                        {
                            throw new RangeSourceException((int)response.StatusCode,
                                $"节点返回 {(int)response.StatusCode}");
                        }

                        var contentRange = response.Content.Headers.ContentRange;
                        if (contentRange?.FirstBytePosition == null || contentRange.FirstBytePosition.Value != (ulong)start)
                        {
                            throw new RangeSourceException(0, "节点返回的位置不对");
                        }

                        var expectedEnd = end;
                        if (contentRange.Length != null)
                        {
                            var total = (long)contentRange.Length.Value;
                            lock (m_lock)
                            {
                                if (m_totalLength < 0) m_totalLength = total;
                            }
                            expectedEnd = Math.Min(end, total - 1);
                        }

                        var data = new byte[expectedEnd - start + 1];
                        await ReadBodyAsync(response, data, timeout.Token, token).ConfigureAwait(false);

                        ReportSuccess(source, data.Length, stopwatch.Elapsed);
                        return data;
                    }
                }
            }
            catch (Exception ex) when (!token.IsCancellationRequested)
            {
                ReportFailure(source, ex);
                throw;
            }
            finally
            {
                lock (m_lock)
                {
                    source.Active--;
                }
            }
        }

        /// <summary>
        /// 读取响应内容；中途长时间没有新数据就放弃，换节点重下
        /// </summary>
        private static async Task ReadBodyAsync(HttpResponseMessage response, byte[] data, CancellationToken timeoutToken,
            CancellationToken token)
        {
            using (var input = await response.Content.ReadAsInputStreamAsync().AsTask(timeoutToken).ConfigureAwait(false))
            {
                var chunk = new global::Windows.Storage.Streams.Buffer(64 * 1024);
                var offset = 0;
                while (offset < data.Length)
                {
                    IBuffer received;
                    using (var stall = CancellationTokenSource.CreateLinkedTokenSource(timeoutToken))
                    {
                        stall.CancelAfter(StallTimeout);
                        try
                        {
                            received = await input.ReadAsync(chunk, chunk.Capacity, InputStreamOptions.Partial)
                                .AsTask(stall.Token).ConfigureAwait(false);
                        }
                        catch (Exception) when (stall.IsCancellationRequested && !token.IsCancellationRequested)
                        {
                            throw new RangeSourceException(0, "节点传输停住了");
                        }
                    }
                    if (received.Length == 0) break;
                    if (offset + received.Length > data.Length)
                    {
                        throw new RangeSourceException(0, "节点返回的数据过多");
                    }
                    received.CopyTo(0, data, offset, (int)received.Length);
                    offset += (int)received.Length;
                }
                if (offset != data.Length)
                {
                    throw new RangeSourceException(0, $"节点返回的长度不对：{offset}/{data.Length}");
                }
            }
        }

        /// <summary>
        /// 选一个节点：坏节点暂停使用；提前下载的块顺便试试还没用过的节点，
        /// 播放器在等的块交给测过速的节点，按速度和正在用的连接数分配
        /// </summary>
        private RangeSource PickSource(List<RangeSource> tried, bool urgent)
        {
            lock (m_lock)
            {
                var now = DateTime.UtcNow;
                var usable = m_sources.Where(x => x.BannedUntil <= now).ToList();
                var candidates = usable.Where(x => !tried.Contains(x)).ToList();
                if (candidates.Count == 0) candidates = usable;
                if (candidates.Count == 0) return null;

                if (!urgent)
                {
                    var untested = candidates.FirstOrDefault(x => !x.Tested && x.Active == 0);
                    if (untested != null) return untested;
                }

                var tested = candidates.Where(x => x.Tested).ToList();
                if (tested.Count == 0)
                {
                    // 还没有测速数据：接口给的地址排在最前，选正在用的连接最少的
                    return candidates.OrderBy(x => x.Active).First();
                }
                if (urgent)
                {
                    candidates = tested;
                }

                var guess = tested.Average(x => x.Speed);
                return candidates
                    .OrderByDescending(x => (x.Tested ? x.Speed : guess) / (x.Active + 1))
                    .First();
            }
        }

        private bool HasTestedSource()
        {
            lock (m_lock)
            {
                return m_sources.Any(x => x.Tested);
            }
        }

        private void ReportSuccess(RangeSource source, int bytes, TimeSpan elapsed)
        {
            var speed = bytes / Math.Max(0.05, elapsed.TotalSeconds);
            lock (m_lock)
            {
                source.Speed = source.Tested ? source.Speed * 0.7 + speed * 0.3 : speed;
                source.Tested = true;
                source.Strikes = 0;
            }
        }

        private void ReportFailure(RangeSource source, Exception ex)
        {
            if (ex is RangeSourceException { StatusCode: 416 }) return;
            lock (m_lock)
            {
                // 4xx：节点拒绝了这个地址，这个视频不再用它
                if (ex is RangeSourceException { StatusCode: >= 400 and < 500 })
                {
                    source.BannedUntil = DateTime.MaxValue;
                }
                else if (++source.Strikes >= STRIKE_LIMIT)
                {
                    source.Strikes = 0;
                    source.BannedUntil = DateTime.UtcNow + BanDuration;
                }
                else
                {
                    return;
                }
            }
            _logger.Debug($"{m_name}多线程下载暂停使用节点 {new Uri(source.Url).Host}：{ex.Message}");
        }

        private static void Observe(Task task)
        {
            task.ContinueWith(t => _ = t.Exception, TaskContinuationOptions.OnlyOnFaulted);
        }

        private enum RangeBlockState
        {
            Pending,
            Running,
            Done,
            Failed,
        }

        private class RangeBlock
        {
            public RangeBlock(long index)
            {
                Index = index;
                Observe(Completion.Task);
            }

            public long Index { get; }

            /// <summary>
            /// 播放器正在等这一块
            /// </summary>
            public bool Urgent { get; set; }

            public RangeBlockState State { get; set; } = RangeBlockState.Pending;

            public TaskCompletionSource<byte[]> Completion { get; } =
                new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private class RangeSourceException : Exception
        {
            public RangeSourceException(int statusCode, string message) : base(message)
            {
                StatusCode = statusCode;
            }

            public int StatusCode { get; }
        }
    }
}
