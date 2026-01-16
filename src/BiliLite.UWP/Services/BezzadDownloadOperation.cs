using System;
using System.Threading;
using System.Threading.Tasks;
using Downloader;
using BiliLite.Models.Download;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    public class BezzadDownloadOperation : IDownloadOperation
    {
        private readonly Downloader.DownloadService m_downloadService;
        private readonly DownloadPackage m_downloadPackage;
        private readonly string m_filePath;
        private readonly DownloadInfo m_downloadInfo;
        private readonly BezzadDownloadProgress m_progress;
        private readonly Guid m_guid;
        private bool m_isPaused;
        private bool m_isStarted;

        public BezzadDownloadOperation(Downloader.DownloadService downloadService, Downloader.DownloadPackage downloadPackage, string filePath, DownloadInfo downloadInfo)
        {
            m_downloadService = downloadService;
            m_downloadPackage = downloadPackage;
            m_filePath = filePath;
            m_downloadInfo = downloadInfo;
            m_guid = Guid.NewGuid();
            m_progress = new BezzadDownloadProgress(this);
            
            SetupDownloadEvents();
        }

        public Guid Guid => m_guid;

        public string Group => m_downloadInfo?.CID ?? string.Empty;

        public IDownloadProgress Progress => m_progress;

        public async Task AttachAsync(CancellationToken cancellationToken = default, IProgress<IDownloadOperation>? progress = null)
        {
            if (!m_isStarted)
            {
                await StartAsync();
            }

            // 监听下载进度事件
            m_downloadService.DownloadProgressChanged += (sender, e) =>
            {
                progress?.Report(this);
            };

            // 等待下载完成或取消
            var tcs = new TaskCompletionSource<bool>();
            cancellationToken.Register(() => tcs.TrySetCanceled());
            
            m_downloadService.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    tcs.TrySetCanceled();
                }
                else if (e.Error != null)
                {
                    tcs.TrySetException(e.Error);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            };

            await tcs.Task;
        }

        public void Pause()
        {
            if (m_isStarted && !m_isPaused)
            {
                m_downloadService.CancelAsync();
                m_isPaused = true;
                m_progress.UpdateStatus(DownloadStatus.PausedByApplication);
            }
        }

        public void Resume()
        {
            if (m_isStarted && m_isPaused)
            {
                _ = StartAsync();
                m_isPaused = false;
                m_progress.UpdateStatus(DownloadStatus.Running);
            }
        }

        public async Task StartAsync()
        {
            if (m_isStarted && !m_isPaused)
            {
                return; // 已经在运行中
            }

            try
            {
                m_isStarted = true;
                m_isPaused = false;
                m_progress.UpdateStatus(DownloadStatus.Running);

                // 保存下载信息到设置
                var guidInfo = new DownloadGUIDInfo()
                {
                    GUID = m_guid.ToString(),
                    CID = m_downloadInfo.CID,
                    ID = m_downloadInfo.Type == DownloadType.Video ? m_downloadInfo.AVID : m_downloadInfo.SeasonID.ToString(),
                    FileName = m_downloadPackage.FileName,
                    Title = m_downloadInfo.Title,
                    EpisodeTitle = m_downloadInfo.EpisodeTitle,
                    Path = System.IO.Path.GetDirectoryName(m_filePath),
                    Type = m_downloadInfo.Type
                };

                SettingService.SetValue(m_guid.ToString(), guidInfo);

                // 开始下载
                await m_downloadService.DownloadFileTaskAsync(m_downloadPackage.Urls[0], m_filePath);
                
                if (m_downloadPackage.Status == Downloader.DownloadStatus.Completed)
                {
                    m_progress.UpdateStatus(DownloadStatus.Completed);
                }
                else if (m_downloadPackage.Status == Downloader.DownloadStatus.Failed)
                {
                    m_progress.UpdateStatus(DownloadStatus.Error);
                }
            }
            catch (OperationCanceledException)
            {
                m_progress.UpdateStatus(DownloadStatus.PausedByApplication);
            }
            catch (Exception)
            {
                m_progress.UpdateStatus(DownloadStatus.Error);
                throw;
            }
        }

        private void SetupDownloadEvents()
        {
            m_downloadService.DownloadProgressChanged += (sender, e) =>
            {
                m_progress.UpdateBytes((ulong)e.ReceivedBytesSize, (ulong)e.TotalBytesToReceive);
            };

            m_downloadService.DownloadFileCompleted += (sender, e) =>
            {
                if (e.Cancelled)
                {
                    m_progress.UpdateStatus(DownloadStatus.PausedByApplication);
                }
                else if (e.Error != null)
                {
                    m_progress.UpdateStatus(DownloadStatus.Error);
                }
                else
                {
                    m_progress.UpdateStatus(DownloadStatus.Completed);
                }
            };
        }
    }

    internal class BezzadDownloadProgress : IDownloadProgress
    {
        private readonly BezzadDownloadOperation _operation;
        private ulong _bytesReceived;
        private ulong _totalBytesToReceive;
        private DownloadStatus _status;

        public BezzadDownloadProgress(BezzadDownloadOperation operation)
        {
            _operation = operation;
            _status = DownloadStatus.Idle;
        }

        public ulong BytesReceived => _bytesReceived;

        public ulong TotalBytesToReceive => _totalBytesToReceive;

        public DownloadStatus Status => _status;

        public void UpdateBytes(ulong bytesReceived, ulong totalBytesToReceive)
        {
            _bytesReceived = bytesReceived;
            _totalBytesToReceive = totalBytesToReceive;
        }

        public void UpdateStatus(DownloadStatus status)
        {
            _status = status;
        }
    }
}
