using BiliLite.Models.Common.Download;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Download;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls.Dialogs
{
    /// <summary>
    /// 导出前的选项对话框：格式 / 编码方式 / 轨道选择
    /// </summary>
    public sealed partial class VideoExportOptionsDialog : ContentDialog
    {
        public VideoExportOptionsDialogViewModel ViewModel { get; }

        public VideoExportOptionsDialog(VideoExportOptionsDialogViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        /// <summary>
        /// 用上次的选项和当前视频的轨道填充界面。
        /// 轨道选择不持久化（每个视频的轨道不同），默认取各自的推荐项。
        /// </summary>
        public void LoadOptions(VideoExportOptions current, List<VideoExportTrackOption> videoOptions,
            List<VideoExportTrackOption> audioOptions, List<DownloadSubtitleInfo> subtitleInfos)
        {
            ViewModel.VideoTracks = videoOptions ?? new List<VideoExportTrackOption>();
            ViewModel.AudioTracks = audioOptions ?? new List<VideoExportTrackOption>();
            ViewModel.SubtitleOptions = BuildSubtitleOptions(subtitleInfos);

            ViewModel.Format = current?.Format ?? VideoExportOptions.FORMAT_MKV;
            ViewModel.VideoMode = current?.VideoMode ?? VideoExportOptions.MODE_COPY;
            ViewModel.HdrToSdr = current?.HdrToSdr ?? false;

            var downscale = current?.Downscale ?? VideoExportOptions.DOWNSCALE_SOURCE;
            ViewModel.SelectedDownscale = ViewModel.DownscaleOptions.FirstOrDefault(x => x.Value == downscale)
                                          ?? ViewModel.DownscaleOptions[0];

            ViewModel.SelectedVideoTrack = ViewModel.VideoTracks.FirstOrDefault(x => x.IsDefault)
                                           ?? ViewModel.VideoTracks.FirstOrDefault();
            ViewModel.SelectedAudioTrack = ViewModel.AudioTracks.FirstOrDefault(x => x.IsDefault)
                                           ?? ViewModel.AudioTracks.FirstOrDefault();
            ViewModel.SelectedSubtitle = ViewModel.SubtitleOptions.FirstOrDefault(x => x.Index >= 0)
                                         ?? ViewModel.SubtitleOptions.FirstOrDefault();
        }

        /// <summary>
        /// 把界面上的选择组装成导出选项（含要导出的轨道）。
        /// MKV 保留全部轨道；MP4 只保留 1 条视频轨 + 1 条音频轨。
        /// </summary>
        public VideoExportOptions BuildOptions()
        {
            var options = new VideoExportOptions
            {
                Format = ViewModel.Format,
                VideoMode = ViewModel.VideoMode,
                HdrToSdr = ViewModel.HdrToSdr,
                Downscale = ViewModel.SelectedDownscale?.Value ?? VideoExportOptions.DOWNSCALE_SOURCE,
            };

            if (ViewModel.Format == VideoExportOptions.FORMAT_MKV)
            {
                options.VideoTracks.AddRange(ViewModel.VideoTracks);
                options.AudioTracks.AddRange(ViewModel.AudioTracks);
            }
            else
            {
                var video = ViewModel.SelectedVideoTrack
                            ?? ViewModel.VideoTracks.FirstOrDefault(x => x.IsDefault)
                            ?? ViewModel.VideoTracks.FirstOrDefault();
                if (video != null) options.VideoTracks.Add(video);

                var audio = ViewModel.SelectedAudioTrack
                            ?? ViewModel.AudioTracks.FirstOrDefault(x => x.IsDefault);
                if (audio != null) options.AudioTracks.Add(audio);
            }

            return options;
        }

        private void Mp4_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.Format = VideoExportOptions.FORMAT_MP4;
        }

        private void Mkv_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.Format = VideoExportOptions.FORMAT_MKV;
        }

        private void CopyMode_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.VideoMode = VideoExportOptions.MODE_COPY;
        }

        private void TranscodeMode_Checked(object sender, RoutedEventArgs e)
        {
            ViewModel.VideoMode = VideoExportOptions.MODE_TRANSCODE;
        }

        private static List<VideoExportSubtitleOption> BuildSubtitleOptions(List<DownloadSubtitleInfo> subtitleInfos)
        {
            var options = new List<VideoExportSubtitleOption>
            {
                new VideoExportSubtitleOption { Name = "不封装字幕", Index = -1 },
            };
            if (subtitleInfos == null) return options;

            for (var i = 0; i < subtitleInfos.Count; i++)
            {
                options.Add(new VideoExportSubtitleOption
                {
                    Name = subtitleInfos[i].Name,
                    Index = i,
                });
            }
            return options;
        }
    }
}
