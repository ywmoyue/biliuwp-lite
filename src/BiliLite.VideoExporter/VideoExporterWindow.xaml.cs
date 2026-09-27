using FFMpegCore;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BiliLite.VideoExporter
{
    /// <summary>
    /// VideoExporterWindow.xaml 的交互逻辑
    /// </summary>
    public sealed partial class VideoExporterWindow : Window
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            // 调用方（UWP 侧）用小驼峰字段名序列化
            PropertyNameCaseInsensitive = true,
        };

        private ConvertFileInfo m_convertFileInfo;
        private string m_currentDir = "";
        private string m_ffmpegFile = "";
        private bool m_debug = false;
        private string m_debugLogFile = "";
        private readonly object m_logLock = new object();
        private CancellationTokenSource m_cancelSource;
        private TimeSpan m_totalDuration = TimeSpan.Zero;
        private int m_lastLoggedPercent = -1;

        public VideoExporterWindow()
        {
            InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 400 });
            // 直接关窗口时也要终止 ffmpeg，否则会留下继续跑的后台进程
            this.Closed += (sender, args) =>
            {
                try
                {
                    m_cancelSource?.Cancel();
                }
                catch
                {
                    // 忽略
                }
            };
        }

        public async void Start()
        {
            try
            {
                LoadInfo();
                txtStatus.Text = "正在解压FFmpeg,请稍等";

                var result = await DecompressFFmpeg();
                if (!result)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    txtStatus.Text = "解压FFmpeg失败，请关闭程序后再试";
                    return;
                }
                txtStatus.Text = "正在导出视频";
                await StartTask();
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"执行任务失败：\r\n{ex.Message}";
            }
        }

        private void LoadInfo()
        {
            var args = Environment.GetCommandLineArgs();
            // 脱离界面的验证入口，用文件传 JSON 避免命令行转义问题
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
                    var builder = new AppNotificationBuilder()
                        .AddText($"FFmpeg解压失败:{ex.Message}");

                    var notification = builder.BuildNotification();

                    AppNotificationManager.Default.Show(notification);
                }
                return false;
            });
        }

        private async Task StartTask()
        {
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

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            btnCancel.IsEnabled = false;
            txtStatus.Text = "正在取消...";
            Log("CANCEL REQUESTED");
            m_cancelSource?.Cancel();
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
                if (DispatcherQueue.HasThreadAccess) action();
                else DispatcherQueue.TryEnqueue(() => action());
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
}
