using BiliLite.Controls.Dialogs;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Download;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace BiliLite.Services
{
    /// <summary>
    /// 已下载视频导出：单集导出与整组批量导出
    /// </summary>
    public static class VideoExportHelper
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private const string INVALID_FILE_NAME_CHARS = "[<>/\\\\|:\":?*\\x00-\\x1F]";

        /// <summary>
        /// 导出指定的已下载剧集。多集时只选一次输出目录、只启动一次导出器，由导出器顺序处理。
        /// </summary>
        public static async Task ExportAsync(DownloadedItem data, IEnumerable<DownloadedSubItem> episodes)
        {
            if (data == null || episodes == null) return;

            var records = new List<ExportRecord>();
            foreach (var episode in episodes)
            {
                if (episode == null) continue;
                var videoOptions = episode.GetVideoExportTrackOptions();
                if (videoOptions.Count == 0) continue;
                records.Add(new ExportRecord
                {
                    Item = episode,
                    VideoOptions = videoOptions,
                    AudioOptions = episode.GetAudioExportTrackOptions(),
                });
            }
            if (records.Count == 0)
            {
                NotificationShowExtensions.ShowMessageToast("没有可以导出的视频");
                return;
            }

            var isBatch = records.Count > 1;

            var dialog = App.ServiceProvider.GetRequiredService<VideoExportOptionsDialog>();
            // WinUI3 的 ContentDialog 必须挂到 XamlRoot 上才能弹出
            dialog.XamlRoot = App.MainWindow.Content.XamlRoot;
            var first = records[0];
            dialog.LoadOptions(LoadLastOptions(), first.VideoOptions, first.AudioOptions,
                first.Item.SubtitlePath ?? new List<DownloadSubtitleInfo>(), isBatch);
            if (await dialog.ShowAsync() != ContentDialogResult.Primary) return;

            var options = dialog.BuildOptions();
            var selectedSubtitleIndex = dialog.ViewModel.SelectedSubtitleIndex;
            SaveExportOptions(options);

            var folder = await PickOutputFolder();
            if (folder == null) return;

            var extension = options.IsMp4 ? ".mp4" : ".mkv";
            var usedNames = await GetExistingFileNames(folder);

            var jobs = new List<VideoExportJob>();
            for (var i = 0; i < records.Count; i++)
            {
                var record = records[i];
                var episodeOptions = BuildEpisodeOptions(options, record);
                if (episodeOptions == null) continue;

                var subtitleInfos = record.Item.SubtitlePath ?? new List<DownloadSubtitleInfo>();
                episodeOptions.SubtitleTracks =
                    await ConvertSubtitles(record.Item, subtitleInfos, options, selectedSubtitleIndex);

                var baseName = BuildOutputFileName(data, record.Item, i, records.Count);
                var fileName = MakeUniqueFileName(baseName, extension, usedNames);
                usedNames.Add(fileName);

                jobs.Add(new VideoExportJob
                {
                    Title = string.IsNullOrWhiteSpace(record.Item.Title) ? baseName : record.Item.Title,
                    OutFile = Path.Combine(folder.Path, fileName),
                    Options = episodeOptions,
                });
            }

            if (jobs.Count == 0)
            {
                NotificationShowExtensions.ShowMessageToast("没有可以导出的视频");
                return;
            }

            var closeOnSuccess = SettingService.GetValue(SettingConstants.Download.EXPORT_AUTO_CLOSE_WINDOW,
                SettingConstants.Download.DEFAULT_EXPORT_AUTO_CLOSE_WINDOW);

            NotificationShowExtensions.ShowMessageToast(isBatch ? $"开始导出 {jobs.Count} 集" : "开始导出视频");
            await AppHelper.LaunchConverter(jobs, closeOnSuccess);
        }

        /// <summary>
        /// 导出文件名：分组标题 + 剧集标题，多集时加上排序用的序号
        /// </summary>
        public static string BuildOutputFileName(DownloadedItem data, DownloadedSubItem item, int index, int total)
        {
            var parts = new List<string>();
            if (total > 1) parts.Add((index + 1).ToString("00"));
            if (!string.IsNullOrWhiteSpace(data?.Title)) parts.Add(data.Title.Trim());
            parts.Add(string.IsNullOrWhiteSpace(item?.Title) ? $"第{index + 1}集" : item.Title.Trim());
            return SanitizeFileName(string.Join("-", parts));
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "视频";

            var result = Regex.Replace(name, INVALID_FILE_NAME_CHARS, string.Empty).Trim().TrimEnd('.');
            if (result.Length > 100)
            {
                result = result.Substring(0, 100).Trim().TrimEnd('.');
            }
            return string.IsNullOrWhiteSpace(result) ? "视频" : result;
        }

        private static string MakeUniqueFileName(string baseName, string extension, ISet<string> usedNames)
        {
            var candidate = baseName + extension;
            var index = 1;
            while (usedNames.Contains(candidate))
            {
                candidate = $"{baseName}-{index}{extension}";
                index++;
            }
            return candidate;
        }

        private static async Task<HashSet<string>> GetExistingFileNames(StorageFolder folder)
        {
            var files = await folder.GetFilesAsync();
            return new HashSet<string>(files.Select(x => x.Name), StringComparer.OrdinalIgnoreCase);
        }

        private static async Task<StorageFolder> PickOutputFolder()
        {
            // WinUI3 桌面的 FolderPicker 必须先绑定窗口句柄，否则 PickSingleFolderAsync 会抛 COMException
            var picker = FileExtensions.GetFolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add("*");
            return await picker.PickSingleFolderAsync();
        }

        private static VideoExportOptions LoadLastOptions()
        {
            return new VideoExportOptions
            {
                Format = SettingService.GetValue(SettingConstants.Download.EXPORT_FORMAT,
                    SettingConstants.Download.DEFAULT_EXPORT_FORMAT),
                VideoMode = SettingService.GetValue(SettingConstants.Download.EXPORT_MP4_MODE,
                    SettingConstants.Download.DEFAULT_EXPORT_MP4_MODE),
                HdrToSdr = SettingService.GetValue(SettingConstants.Download.EXPORT_HDR_TO_SDR,
                    SettingConstants.Download.DEFAULT_EXPORT_HDR_TO_SDR),
                Downscale = SettingService.GetValue(SettingConstants.Download.EXPORT_DOWNSCALE,
                    SettingConstants.Download.DEFAULT_EXPORT_DOWNSCALE),
            };
        }

        private static void SaveExportOptions(VideoExportOptions options)
        {
            SettingService.SetValue(SettingConstants.Download.EXPORT_FORMAT, options.Format);
            SettingService.SetValue(SettingConstants.Download.EXPORT_MP4_MODE, options.VideoMode);
            SettingService.SetValue(SettingConstants.Download.EXPORT_HDR_TO_SDR, options.HdrToSdr);
            SettingService.SetValue(SettingConstants.Download.EXPORT_DOWNSCALE, options.Downscale);
        }

        /// <summary>
        /// 按界面上的选项组装单个视频的轨道：MKV 保留全部轨道，MP4 每集取默认视频轨/音频轨
        /// </summary>
        private static VideoExportOptions BuildEpisodeOptions(VideoExportOptions template, ExportRecord record)
        {
            var options = new VideoExportOptions
            {
                Format = template.Format,
                VideoMode = template.VideoMode,
                HdrToSdr = template.HdrToSdr,
                Downscale = template.Downscale,
            };

            if (template.IsMp4)
            {
                var video = record.VideoOptions.FirstOrDefault(x => x.IsDefault)
                            ?? record.VideoOptions.FirstOrDefault();
                if (video == null) return null;
                options.VideoTracks.Add(video);

                var audio = record.AudioOptions.FirstOrDefault(x => x.IsDefault)
                            ?? record.AudioOptions.FirstOrDefault();
                if (audio != null) options.AudioTracks.Add(audio);
            }
            else
            {
                options.VideoTracks.AddRange(record.VideoOptions);
                options.AudioTracks.AddRange(record.AudioOptions);
            }

            return options;
        }

        /// <summary>
        /// 按需把 CC 字幕转成 SRT：MKV 全部，MP4 只有选中的那一条。
        /// </summary>
        private static async Task<List<VideoExportTrackOption>> ConvertSubtitles(DownloadedSubItem item,
            List<DownloadSubtitleInfo> subtitleInfos, VideoExportOptions options, int selectedSubtitleIndex)
        {
            var results = new List<VideoExportTrackOption>();
            if (subtitleInfos.Count == 0) return results;

            List<DownloadSubtitleInfo> needConvert;
            if (options.Format == VideoExportOptions.FORMAT_MKV)
            {
                needConvert = subtitleInfos;
            }
            else if (selectedSubtitleIndex >= 0 && selectedSubtitleIndex < subtitleInfos.Count)
            {
                needConvert = new List<DownloadSubtitleInfo> { subtitleInfos[selectedSubtitleIndex] };
            }
            else
            {
                return results;
            }

            try
            {
                var toSimplified = SettingService.GetValue<bool>(SettingConstants.Player.TO_SIMPLIFIED, true);
                var folder = await StorageFolder.GetFolderFromPathAsync(item.FilePath);
                foreach (var subtitle in needConvert)
                {
                    var outSrtFile = await folder.CreateFileAsync(subtitle.Name + ".srt",
                        CreationCollisionOption.ReplaceExisting);
                    var subtitleFile = await StorageFile.GetFileFromPathAsync(Path.Combine(item.FilePath, subtitle.Url));
                    var content = await FileIO.ReadTextAsync(subtitleFile);
                    var result = content.CcConvertToSrt(toSimplified && subtitle.Name.Contains("繁体"));
                    await FileIO.WriteTextAsync(outSrtFile, result);
                    results.Add(new VideoExportTrackOption
                    {
                        Path = outSrtFile.Path,
                        QualityId = -1,
                        CodecId = -1,
                        Label = subtitle.Name,
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("转换SRT字幕失败");
                logger.Log("转换字幕失败", LogType.Error, ex);
            }

            return results;
        }

        private class ExportRecord
        {
            public DownloadedSubItem Item { get; set; }

            public List<VideoExportTrackOption> VideoOptions { get; set; }

            public List<VideoExportTrackOption> AudioOptions { get; set; }
        }
    }
}
