using FFMpegCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BiliLite.VideoExporter
{

    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoExporterWindow : Window
    {
        ConvertFileInfo convertFileInfo;
        string currentDir = "";
        string ffmpegFile = "";
        private bool debug = false;
        public VideoExporterWindow()
        {
            InitializeComponent();
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = 400, Height = 400 });
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
                StartTask();
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
                    var builder = new AppNotificationBuilder()
                        .AddText($"FFmpeg解压失败:{ex.Message}");

                    var notification = builder.BuildNotification();

                    AppNotificationManager.Default.Show(notification);
                }
                return false;
            });
        }

        private async void StartTask()
        {
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
}
