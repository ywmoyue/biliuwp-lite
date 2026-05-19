using FFMpegCore;
using FFMpegCore.Enums;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;
using System.Text.RegularExpressions;
using System.Globalization;

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

        ConvertFileInfo convertFileInfo;
        AudioNormalizeInfo audioNormalizeInfo;
        string currentDir = "";
        string ffmpegFile = "";
        private bool debug = false;
        private TaskKind taskKind = TaskKind.VideoConvert;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadInfo()
        {
            var args = Environment.GetCommandLineArgs();
            if (TryLoadAudioNormalizeDebugArgs(args))
            {
                return;
            }

            var normalizeStr = Windows.Storage.ApplicationData.Current.LocalSettings.Values["AudioNormalizeRequest"] as string;
            if (!string.IsNullOrWhiteSpace(normalizeStr))
            {
                audioNormalizeInfo = System.Text.Json.JsonSerializer.Deserialize<AudioNormalizeInfo>(normalizeStr);
                taskKind = TaskKind.AudioNormalize;
                txtName.Text = string.IsNullOrWhiteSpace(audioNormalizeInfo?.inputFile)
                    ? "音量均衡"
                    : Path.GetFileName(audioNormalizeInfo.inputFile);
                return;
            }

            var debugParam = args.FirstOrDefault(arg => arg.StartsWith("--debug="));
            var param = debugParam != null ? debugParam.Substring("--debug=".Length) : "";
            if (!string.IsNullOrEmpty(param))
            {
                convertFileInfo = System.Text.Json.JsonSerializer.Deserialize<ConvertFileInfo>(param);
                txtName.Text = convertFileInfo.title;
                debug = true;
                return;
            }
            var str = Windows.Storage.ApplicationData.Current.LocalSettings.Values["VideoConverterInfo"] as string;
            convertFileInfo = System.Text.Json.JsonSerializer.Deserialize<ConvertFileInfo>(str);
            txtName.Text = convertFileInfo.title;
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
            audioNormalizeInfo = System.Text.Json.JsonSerializer.Deserialize<AudioNormalizeInfo>(json);
            if (audioNormalizeInfo == null || string.IsNullOrWhiteSpace(audioNormalizeInfo.inputFile))
            {
                throw new InvalidOperationException("--normalize-debug 参数无效：缺少 inputFile");
            }

            if (string.IsNullOrWhiteSpace(audioNormalizeInfo.operationId))
            {
                audioNormalizeInfo.operationId = $"debug_{Guid.NewGuid():N}";
            }

            taskKind = TaskKind.AudioNormalize;
            debug = true;
            txtName.Text = Path.GetFileName(audioNormalizeInfo.inputFile);
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
                    if (taskKind == TaskKind.AudioNormalize)
                    {
                        SetAudioNormalizeResult(false, null, "ffmpeg解压失败");
                        Close();
                    }
                    return;
                }
                txtStatus.Text = taskKind == TaskKind.AudioNormalize ? "正在均衡音量" : "正在导出视频";
                StartTask();
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"执行任务失败：\r\n{ex.Message}";
                if (taskKind == TaskKind.AudioNormalize)
                {
                    SetAudioNormalizeResult(false, null, ex.Message);
                    Close();
                }
            }

        }

        private void RunAudioNormalizeInBackgroundIfNeeded()
        {
            if (taskKind != TaskKind.AudioNormalize)
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
            if (debug)
            {
                currentDir = Environment.CurrentDirectory;
            }
            else
            {
                currentDir = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
            }
            return await Task.Run<bool>(() =>
            {
                try
                {

                    var ffmpeg7ZipPath = System.IO.Path.Combine(zipDir, "ffmpeg.7z");
                    ffmpegFile = System.IO.Path.Combine(currentDir, "ffmpeg.exe");
                    //检查文件是否存在
                    if (File.Exists(ffmpegFile))
                    {
                        return true;
                    }
                    //解压文件
                    using (var archive = SevenZipArchive.Open(ffmpeg7ZipPath))
                    {
                        foreach (var entry in archive.Entries.Where(entry => !entry.IsDirectory))
                        {
                            entry.WriteToDirectory(currentDir, new ExtractionOptions()
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

        private async void StartTask()
        {
            if (taskKind == TaskKind.AudioNormalize)
            {
                await NormalizeAudio();
                return;
            }

            if (convertFileInfo.inputFiles.Count == 0)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频为空";
                return;
            }
            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = currentDir, TemporaryFilesFolder = currentDir });

            if (convertFileInfo.isDash)
            {
                if (convertFileInfo.subtitle.Count <= 0)
                {
                    await ConvertDash();
                }
                else
                {
                    await ConvertDashWithSubtitle();
                }

            }
            else
            {
                await ConvertToMp4();
            }

        }

        private async Task NormalizeAudio()
        {
            if (audioNormalizeInfo == null || string.IsNullOrWhiteSpace(audioNormalizeInfo.inputFile))
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "音量均衡失败：输入文件为空";
                SetAudioNormalizeResult(false, null, "输入文件为空");
                Close();
                return;
            }

            if (!File.Exists(audioNormalizeInfo.inputFile))
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "音量均衡失败：输入文件不存在";
                SetAudioNormalizeResult(false, null, "输入文件不存在");
                Close();
                return;
            }

            GlobalFFOptions.Configure(new FFOptions { BinaryFolder = currentDir, TemporaryFilesFolder = currentDir });

            try
            {
                var lufs = Math.Max(-20d, Math.Min(-5d, audioNormalizeInfo.targetLufs));
                var measuredLufs = await MeasureInputLufsAsync(audioNormalizeInfo.inputFile, lufs);
                if (measuredLufs.HasValue && Math.Abs(measuredLufs.Value - lufs) <= 1d)
                {
                    progressBar.Visibility = Visibility.Collapsed;
                    txtStatus.Text = $"原始响度 {measuredLufs.Value:F1} LUFS，接近目标，已跳过处理";
                    SetAudioNormalizeResult(true, audioNormalizeInfo.inputFile,
                        $"skip-normalize: input={measuredLufs.Value:F1} target={lufs:F1}");
                    Close();
                    return;
                }

                var outputFile = Path.Combine(
                    Path.GetDirectoryName(audioNormalizeInfo.inputFile) ?? currentDir,
                    $"{Path.GetFileNameWithoutExtension(audioNormalizeInfo.inputFile)}.loudnorm.m4a");
                var customArgs = $"-af loudnorm=I={lufs:F1}:TP=-2:LRA=7";

                await FFMpegArguments
                    .FromFileInput(audioNormalizeInfo.inputFile)
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
            if (string.IsNullOrWhiteSpace(ffmpegFile) || !File.Exists(ffmpegFile))
            {
                return null;
            }

            var args = $"-hide_banner -i \"{inputFile}\" -af loudnorm=I={targetLufs:F1}:TP=-2:LRA=7:print_format=json -f null NUL";
            var psi = new ProcessStartInfo
            {
                FileName = ffmpegFile,
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
            if (audioNormalizeInfo == null || string.IsNullOrWhiteSpace(audioNormalizeInfo.operationId))
            {
                return;
            }

            var result = new AudioNormalizeResult
            {
                success = success,
                outputFile = outputFile,
                error = error
            };
            var resultStr = System.Text.Json.JsonSerializer.Serialize(result);

            if (debug)
            {
                Debug.WriteLine($"[AudioNormalizeResult_{audioNormalizeInfo.operationId}] {resultStr}");
                return;
            }

            Windows.Storage.ApplicationData.Current.LocalSettings.Values[$"AudioNormalizeResult_{audioNormalizeInfo.operationId}"] = resultStr;
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove("AudioNormalizeRequest");
        }
        private async Task ConvertDash()
        {
            try
            {
                var audioPaths = convertFileInfo.inputFiles
                    .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("audio")).ToList();
                var videoPaths = convertFileInfo.inputFiles
                    .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("video")).ToList();

                var ffmpegArgs = FFMpegArguments.FromFileInput(videoPaths.FirstOrDefault());

                foreach (var videoPath in videoPaths.Skip(1))
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(videoPath);
                }
                foreach (var audioPath in audioPaths)
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(audioPath);
                }

                // 构建-map参数
                var mapArguments = new StringBuilder();
                int videoIndex = 0, audioIndex = videoPaths.Count, subtitleIndex = videoPaths.Count + audioPaths.Count;

                // 映射视频流
                for (int i = 0; i < videoPaths.Count; i++)
                {
                    mapArguments.Append($"-map {videoIndex}:v ");
                    videoIndex++;
                }
                // 映射音频流
                for (int i = 0; i < audioPaths.Count; i++)
                {
                    mapArguments.Append($"-map {audioIndex}:a ");
                    audioIndex++;
                }

                // 输出文件并添加-map参数
                var info = ffmpegArgs
                    .OutputToFile(convertFileInfo.outFile, true, options =>
                            options
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument(mapArguments.ToString())) // 添加-map参数
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument("-c copy")) // 直接复制流
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument("-strict -2")) // 允许实验性编码器
                                .WithFastStart() // 启用快速启动
                    ).ProcessAsynchronously();

                await info;

                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }
        }
        private async Task ConvertToMp4()
        {
            try
            {
                var info = FFMpegArguments.FromFileInput(convertFileInfo.inputFiles.FirstOrDefault());
                if (convertFileInfo.subtitle.Count > 0)
                {
                    info = info.AddFileInput(convertFileInfo.subtitle.FirstOrDefault());
                }
                var processor = info.OutputToFile(convertFileInfo.outFile, true, options =>
                        options.WithArgument(new FFMpegCore.Arguments.CustomArgument(convertFileInfo.subtitle.Count > 0 ? "-c copy -c:s mov_text" : "-c copy"))
                        .WithFastStart()
                );
                await processor.ProcessAsynchronously();
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }

        }
        private async Task ConvertDashWithSubtitle()
        {
            try
            {
                var audioPaths = convertFileInfo.inputFiles
                    .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("audio")).ToList();
                var videoPaths = convertFileInfo.inputFiles
                    .Where(x => Path.GetFileNameWithoutExtension(x).StartsWith("video")).ToList();

                var ffmpegArgs = FFMpegArguments.FromFileInput(videoPaths.FirstOrDefault());

                foreach (var videoPath in videoPaths.Skip(1))
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(videoPath);
                }
                foreach (var audioPath in audioPaths)
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(audioPath);
                }
                foreach (var subtitle in convertFileInfo.subtitle)
                {
                    ffmpegArgs = ffmpegArgs.AddFileInput(subtitle);
                }

                // 构建-map参数
                var mapArguments = new StringBuilder();
                int videoIndex = 0, audioIndex = videoPaths.Count, subtitleIndex = videoPaths.Count + audioPaths.Count;

                // 映射视频流
                for (int i = 0; i < videoPaths.Count; i++)
                {
                    mapArguments.Append($"-map {videoIndex}:v ");
                    videoIndex++;
                }
                // 映射音频流
                for (int i = 0; i < audioPaths.Count; i++)
                {
                    mapArguments.Append($"-map {audioIndex}:a ");
                    audioIndex++;
                }
                // 映射字幕流
                for (int i = 0; i < convertFileInfo.subtitle.Count; i++)
                {
                    mapArguments.Append($"-map {subtitleIndex}:s ");
                    subtitleIndex++;
                }

                // 输出文件并添加-map参数
                var info = ffmpegArgs
                    .OutputToFile(convertFileInfo.outFile, true, options =>
                            options
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument(mapArguments.ToString())) // 添加-map参数
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument("-c copy")) // 直接复制流
                                                                                                  //.WithArgument(new FFMpegCore.Arguments.CustomArgument("-c:s mov_text")) // 字幕编码器
                                .WithArgument(new FFMpegCore.Arguments.CustomArgument("-strict -2")) // 允许实验性编码器
                                .WithFastStart() // 启用快速启动
                    ).ProcessAsynchronously();

                progressBar.Visibility = Visibility.Collapsed;

                await info;

                txtStatus.Text = "视频导出成功!";
            }
            catch (Exception ex)
            {
                progressBar.Visibility = Visibility.Collapsed;
                txtStatus.Text = $"视频导出失败：\r\n{ex.Message}";
            }

        }


    }


    public class ConvertFileInfo
    {
        public string title { get; set; }
        public List<string> inputFiles { get; set; }
        public List<string> subtitle { get; set; }
        public string outFile { get; set; }
        public bool isDash { get; set; }
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
