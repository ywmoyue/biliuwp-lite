using FFMpegCore;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace BiliLite.Win32Tools
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum TaskKind
        {
            VideoConvert,
            AudioNormalize
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            // 调用方（UWP 侧）用小驼峰字段名序列化
            PropertyNameCaseInsensitive = true,
        };

        private ConvertFileInfo m_convertFileInfo;
        private AudioNormalizeInfo m_audioNormalizeInfo;
        private TaskKind m_taskKind = TaskKind.VideoConvert;
        private string m_currentDir = "";
        private string m_ffmpegFile = "";
        private bool m_debug = false;
        private string m_debugLogFile = "";
        private readonly object m_logLock = new object();
        private CancellationTokenSource m_cancelSource;
        private TimeSpan m_totalDuration = TimeSpan.Zero;
        private int m_lastLoggedPercent = -1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadInfo()
        {
            var args = Environment.GetCommandLineArgs();

            // 音量均衡：命令行调试入口（Base64/URL 编码的 JSON）
            if (TryLoadAudioNormalizeDebugArgs(args))
            {
                return;
            }

            // 音量均衡：由 UWP 侧写入 LocalSettings 后拉起本进程
            var normalizeStr = Windows.Storage.ApplicationData.Current.LocalSettings.Values["AudioNormalizeRequest"] as string;
            if (!string.IsNullOrWhiteSpace(normalizeStr))
            {
                m_audioNormalizeInfo = JsonSerializer.Deserialize<AudioNormalizeInfo>(normalizeStr, _jsonOptions);
                m_taskKind = TaskKind.AudioNormalize;
                txtName.Text = string.IsNullOrWhiteSpace(m_audioNormalizeInfo?.inputFile)
                    ? "音量均衡"
                    : Path.GetFileName(m_audioNormalizeInfo.inputFile);
                return;
            }

            // 视频导出：脱离界面的验证入口，用文件传 JSON 避免命令行转义问题
            var debugFileParam = args.FirstOrDefault(arg => arg.StartsWith("--debug-file="));
            if (debugFileParam != null)
            {
                var infoFile = debugFileParam.Substring("--debug-file=".Length);
                m_convertFileInfo = DeserializeInfo(File.ReadAllText(infoFile));
                txtName.Text = m_convertFileInfo.Title;
                m_debug = true;
                return;
            }

            var debugParam = args.FirstOrDefault(arg => arg.StartsWith("--debug="));
            var param = debugParam != null ? debugParam.Substring("--debug=".Length) : "";
            if (!string.IsNullOrEmpty(param))
            {
                m_convertFileInfo = DeserializeInfo(param);
                txtName.Text = m_convertFileInfo.Title;
                m_debug = true;
                return;
            }
            var str = Windows.Storage.ApplicationData.Current.LocalSettings.Values["VideoConverterInfo"] as string;
            m_convertFileInfo = DeserializeInfo(str);
            txtName.Text = m_convertFileInfo.Title;
        }

        private static ConvertFileInfo DeserializeInfo(string json)
        {
            return JsonSerializer.Deserialize<ConvertFileInfo>(json, _jsonOptions);
        }

        private bool TryLoadAudioNormalizeDebugArgs(string[] args)
        {
            var debugParam = args.FirstOrDefault(arg => arg.StartsWith("--normalize-debug="));
            if (string.IsNullOrWhiteSpace(debugParam))
            {
                return false;
            }

            var rawParam = debugParam.Substring("--normalize-debug=".Length);
            if (string.IsNullOrWhiteSpace(rawParam))
            {
                return false;
            }

            var json = TryDecodeNormalizeDebugParam(rawParam);
            m_audioNormalizeInfo = JsonSerializer.Deserialize<AudioNormalizeInfo>(json, _jsonOptions);
            if (m_audioNormalizeInfo == null || string.IsNullOrWhiteSpace(m_audioNormalizeInfo.inputFile))
            {
                throw new InvalidOperationException("--normalize-debug 参数无效：缺少 inputFile");
            }

            if (string.IsNullOrWhiteSpace(m_audioNormalizeInfo.operationId))
            {
                m_audioNormalizeInfo.operationId = $"debug_{Guid.NewGuid():N}";
            }

            m_taskKind = TaskKind.AudioNormalize;
            m_debug = true;
            txtName.Text = Path.GetFileName(m_audioNormalizeInfo.inputFile);
            return true;
        }

        private static string TryDecodeNormalizeDebugParam(string rawParam)
        {
            // 优先按 Base64 解析，失败后回退到 URL Decode（可直接传 encodeURIComponent(JSON)）
            try
            {
                var bytes = Convert.FromBase64String(rawParam);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return Uri.UnescapeDataString(rawParam);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadInfo();
                RunAudioNormalizeInBackgroundIfNeeded();
                txtStatus.Text = "正在解压FFmpeg,请稍等";

                var result = await DecompressFFmpeg();
                if (!result)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    txtStatus.Text = "解压FFmpeg失败，请关闭程序后再试";
                    if (m_taskKind == TaskKind.AudioNormalize)
                    {
                        SetAudioNormalizeResult(false, null, "ffmpeg解压失败");
                        Close();
                    }
                    return;
                }
                txtStatus.Text = m_taskKind == TaskKind.AudioNormalize ? "正在均衡音量" : "正在导出视频";
                await StartTask();
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"执行任务失败：\r\n{ex.Message}";
                if (m_taskKind == TaskKind.AudioNormalize)
                {
                    SetAudioNormalizeResult(false, null, ex.Message);
                    Close();
                }
            }

        }

        private void RunAudioNormalizeInBackgroundIfNeeded()
        {
            if (m_taskKind != TaskKind.AudioNormalize)
            {
                return;
            }

            ShowInTaskbar = false;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                WindowState = WindowState.Minimized;
                Hide();
            }), DispatcherPriority.ApplicationIdle);
        }

        private async Task<bool> DecompressFFmpeg()
        {

            var zipDir = Assembly.GetExecutingAssembly().Location;
            zipDir = System.IO.Path.GetDirectoryName(zipDir);
            if (m_debug)
            {
                m_currentDir = Environment.CurrentDirectory;
            }
            else
            {
                m_currentDir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            if (m_debug)
            {
                m_debugLogFile = System.IO.Path.Combine(m_currentDir, "export_debug.log");
                Log("=== export debug ===");
            }
            return await Task.Run<bool>(() =>
            {
                try
                {

                    var ffmpeg7ZipPath = System.IO.Path.Combine(zipDir, "ffmpeg.7z");
                    m_ffmpegFile = System.IO.Path.Combine(m_currentDir, "ffmpeg.exe");
                    //检查文件是否存在
                    if (File.Exists(m_ffmpegFile))
                    {
                        return true;
                    }
                    //解压文件
                    using (var archive = SevenZipArchive.Open(ffmpeg7ZipPath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(m_currentDir, new ExtractionOptions()
                            {
                                ExtractFullPath = true,
                                Overwrite = true
                            });
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "FFmpeg解压失败");
                }
                return false;
            });
        }

        private async Task StartTask()
        {
            if (m_taskKind == TaskKind.AudioNormalize)
            {
                await NormalizeAudio();
                return;
            }

            VideoExportPlan plan;
            try
            {
                plan = VideoExportArgs.Build(m_convertFileInfo);
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"无法开始导出：\r\n{ex.Message}";
                Log("BUILD FAILED: " + ex.Message);
                return;
            }

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = m_currentDir, TemporaryFilesFolder = m_currentDir });

            Log("format=" + plan.Format + " transcode=" + plan.Transcode);
            Log(plan.ToCommandLine(m_ffmpegFile));

            m_totalDuration = TimeSpan.Zero;
            m_lastLoggedPercent = -1;
            m_cancelSource = new CancellationTokenSource();
            progressBar.Value = 0;
            progressBar.IsIndeterminate = true;
            btnCancel.IsEnabled = true;
            btnCancel.Visibility = Visibility.Visible;

            try
            {
                var ffmpegArgs = FFMpegArguments.FromFileInput(plan.Inputs[0]);
                foreach (var input in plan.Inputs.Skip(1))
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(input);
                }

                var outputArguments = string.Join(" ", plan.OutputArgs);
                var processor = ffmpegArgs
                    .OutputToFile(plan.OutputFile, true, options =>
                        options.WithArgument(new FFMpegCore.Arguments.CustomArgument(outputArguments)))
                    .NotifyOnOutput(new Action<string>(HandleFFmpegLine))
                    .NotifyOnError(new Action<string>(HandleFFmpegLine))
                    .NotifyOnProgress(new Action<TimeSpan>(OnProgress))
                    .CancellableThrough(m_cancelSource.Token);

                await processor.ProcessAsynchronously();

                progressBar.IsIndeterminate = false;
                progressBar.Value = 100;
                btnCancel.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
                Log("SUCCESS");
            }
            catch (Exception ex)
            {
                btnCancel.Visibility = Visibility.Collapsed;
                if (m_cancelSource != null && m_cancelSource.IsCancellationRequested)
                {
                    txtStatus.Text = "已取消导出";
                    Log("CANCELLED");
                    TryDeleteOutput(plan.OutputFile);
                }
                else
                {
                    txtStatus.Text = $"视频导出失败：\r\n{ex.Message}" + BuildFailureHint(plan);
                    Log("FAILED: " + ex);
                }
            }
        }

        private async Task NormalizeAudio()
        {
            if (m_audioNormalizeInfo == null || string.IsNullOrWhiteSpace(m_audioNormalizeInfo.inputFile))
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "音量均衡失败：输入文件为空";
                SetAudioNormalizeResult(false, null, "输入文件为空");
                Close();
                return;
            }

            if (!File.Exists(m_audioNormalizeInfo.inputFile))
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "音量均衡失败：输入文件不存在";
                SetAudioNormalizeResult(false, null, "输入文件不存在");
                Close();
                return;
            }

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = m_currentDir, TemporaryFilesFolder = m_currentDir });

            try
            {
                var lufs = Math.Max(-20d, Math.Min(-5d, m_audioNormalizeInfo.targetLufs));
                var measuredLufs = await MeasureInputLufsAsync(m_audioNormalizeInfo.inputFile, lufs);
                if (measuredLufs.HasValue && Math.Abs(measuredLufs.Value - lufs) <= 1d)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    txtStatus.Text = $"原始响度 {measuredLufs.Value:F1} LUFS，接近目标，已跳过处理";
                    SetAudioNormalizeResult(true, m_audioNormalizeInfo.inputFile,
                        $"skip-normalize: input={measuredLufs.Value:F1} target={lufs:F1}");
                    Close();
                    return;
                }

                var outputFile = Path.Combine(
                    Path.GetDirectoryName(m_audioNormalizeInfo.inputFile) ?? m_currentDir,
                    $"{Path.GetFileNameWithoutExtension(m_audioNormalizeInfo.inputFile)}.loudnorm.m4a");
                var customArgs = $"-af loudnorm=I={lufs:F1}:TP=-2:LRA=7";

                await FFMpegArguments
                    .FromFileInput(m_audioNormalizeInfo.inputFile)
                    .OutputToFile(outputFile, true, options =>
                        options.WithArgument(new FFMpegCore.Arguments.CustomArgument(customArgs)))
                    .ProcessAsynchronously();

                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "音量均衡成功";
                SetAudioNormalizeResult(true, outputFile, null);
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"音量均衡失败：\r\n{ex.Message}";
                SetAudioNormalizeResult(false, null, ex.Message);
            }

            Close();
        }

        private async Task<double?> MeasureInputLufsAsync(string inputFile, double targetLufs)
        {
            if (string.IsNullOrWhiteSpace(m_ffmpegFile) || !File.Exists(m_ffmpegFile))
            {
                return null;
            }

            var args = $"-hide_banner -i \"{inputFile}\" -af loudnorm=I={targetLufs:F1}:TP=-2:LRA=7:print_format=json -f null NUL";
            var psi = new ProcessStartInfo
            {
                FileName = m_ffmpegFile,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            var stderrTask = process.StandardError.ReadToEndAsync();
            var stdoutTask = process.StandardOutput.ReadToEndAsync();
            await Task.Run(() => process.WaitForExit());

            var text = (await stderrTask) + Environment.NewLine + (await stdoutTask);
            var match = Regex.Match(text, "\\\"input_i\\\"\\s*:\\s*\\\"?(?<value>-?\\d+(\\.\\d+)?)\\\"?",
                RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            if (double.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var lufs))
            {
                return lufs;
            }

            return null;
        }

        private void SetAudioNormalizeResult(bool success, string outputFile, string error)
        {
            if (m_audioNormalizeInfo == null || string.IsNullOrWhiteSpace(m_audioNormalizeInfo.operationId))
            {
                return;
            }

            var result = new AudioNormalizeResult
            {
                success = success,
                outputFile = outputFile,
                error = error
            };
            var resultStr = JsonSerializer.Serialize(result);

            if (m_debug)
            {
                Debug.WriteLine($"[AudioNormalizeResult_{m_audioNormalizeInfo.operationId}] {resultStr}");
                return;
            }

            Windows.Storage.ApplicationData.Current.LocalSettings.Values[$"AudioNormalizeResult_{m_audioNormalizeInfo.operationId}"] = resultStr;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove("AudioNormalizeRequest");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.IsEnabled = false;
            txtStatus.Text = "正在取消...";
            Log("CANCEL REQUESTED");
            m_cancelSource?.Cancel();
        }

        /// <summary>
        /// 直接关窗口时也要终止 ffmpeg，否则会留下继续跑的后台进程
        /// </summary>
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                m_cancelSource?.Cancel();
            }
            catch
            {
                // 忽略
            }
        }

        /// <summary>
        /// 记录 ffmpeg 输出，并抓总时长供进度条使用。
        /// 回调在后台线程上，界面更新要切回 UI 线程。
        /// </summary>
        private void HandleFFmpegLine(string line)
        {
            Log(line);
            if (m_totalDuration > TimeSpan.Zero || string.IsNullOrEmpty(line)) return;

            var match = Regex.Match(line, @"Duration:\s*(\d+):(\d+):(\d+(?:\.\d+)?)");
            if (!match.Success) return;

            var hours = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var minutes = int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
            var seconds = double.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture);
            m_totalDuration = new TimeSpan(0, hours, minutes, 0, (int)(seconds * 1000));

            RunOnUiThread(() =>
            {
                progressBar.IsIndeterminate = false;
                progressBar.Value = 0;
            });
        }

        private void OnProgress(TimeSpan elapsed)
        {
            if (m_totalDuration <= TimeSpan.Zero) return;
            var percent = elapsed.TotalMilliseconds / m_totalDuration.TotalMilliseconds * 100;
            if (percent > 100) percent = 100;
            if (percent < 0) percent = 0;
            RunOnUiThread(() => progressBar.Value = percent);

            // 每跨过 25% 记一条日志，便于脱离界面确认进度回调在工作
            var bucket = (int)(percent / 25);
            if (bucket != m_lastLoggedPercent)
            {
                m_lastLoggedPercent = bucket;
                Log("progress=" + (int)percent + "%");
            }
        }

        private void RunOnUiThread(Action action)
        {
            try
            {
                if (Dispatcher.CheckAccess()) action();
                else Dispatcher.Invoke(action);
            }
            catch
            {
                // 窗口已关闭，忽略
            }
        }

        private void TryDeleteOutput(string outputFile)
        {
            try
            {
                if (!string.IsNullOrEmpty(outputFile) && File.Exists(outputFile)) File.Delete(outputFile);
            }
            catch
            {
                // 文件被占用时留个半成品，不影响其它流程
            }
        }

        /// <summary>
        /// copy 模式下音轨可能无法封装进 MP4，给出可操作的提示而不是静默失败
        /// </summary>
        private static string BuildFailureHint(VideoExportPlan plan)
        {
            if (plan.Format != VideoExportArgs.FORMAT_MP4 || plan.Transcode) return string.Empty;
            return "\r\n\r\n如果音轨是「无损」或「杜比」，请重新导出并选择「转码为 H.264 + AAC」。";
        }

        private void Log(string message)
        {
            if (!m_debug || string.IsNullOrEmpty(m_debugLogFile)) return;
            try
            {
                lock (m_logLock)
                {
                    File.AppendAllText(m_debugLogFile, message + Environment.NewLine);
                }
            }
            catch
            {
                // 日志失败不影响导出
            }
        }
    }

    public class AudioNormalizeInfo
    {
        public string operationId { get; set; }
        public string inputFile { get; set; }
        public string outputFile { get; set; }
        public double targetLufs { get; set; }
    }

    public class AudioNormalizeResult
    {
        public bool success { get; set; }
        public string outputFile { get; set; }
        public string error { get; set; }
    }
}
