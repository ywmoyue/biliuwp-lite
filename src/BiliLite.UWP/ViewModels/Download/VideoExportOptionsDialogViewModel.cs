using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Common.Settings;
using BiliLite.ViewModels.Common;
using Microsoft.UI.Xaml;
using PropertyChanged;
using System.Collections.Generic;

namespace BiliLite.ViewModels.Download
{
    [RegisterTransientViewModel]
    public class VideoExportOptionsDialogViewModel : BaseViewModel
    {
        public VideoExportOptionsDialogViewModel()
        {
            DownscaleOptions = new List<KeyValueOption<string>>
            {
                new KeyValueOption<string>("保持原始分辨率", VideoExportOptions.DOWNSCALE_SOURCE),
                new KeyValueOption<string>("1080P", VideoExportOptions.DOWNSCALE_1080),
                new KeyValueOption<string>("720P", VideoExportOptions.DOWNSCALE_720),
            };
            Format = VideoExportOptions.FORMAT_MKV;
            VideoMode = VideoExportOptions.MODE_COPY;
            SelectedDownscale = DownscaleOptions[0];
        }

        public List<VideoExportTrackOption> VideoTracks { get; set; }

        public List<VideoExportTrackOption> AudioTracks { get; set; }

        public List<VideoExportSubtitleOption> SubtitleOptions { get; set; }

        public List<KeyValueOption<string>> DownscaleOptions { get; set; }

        /// <summary>"mp4" | "mkv"</summary>
        public string Format { get; set; }

        /// <summary>"copy" | "transcode"</summary>
        public string VideoMode { get; set; }

        public bool HdrToSdr { get; set; }

        public KeyValueOption<string> SelectedDownscale { get; set; }

        public VideoExportTrackOption SelectedVideoTrack { get; set; }

        public VideoExportTrackOption SelectedAudioTrack { get; set; }

        public VideoExportSubtitleOption SelectedSubtitle { get; set; }

        [DependsOn(nameof(Format))]
        public bool IsMp4 => Format == VideoExportOptions.FORMAT_MP4;

        [DependsOn(nameof(Format))]
        public bool IsMkv => Format == VideoExportOptions.FORMAT_MKV;

        [DependsOn(nameof(VideoMode))]
        public bool IsCopyMode => VideoMode == VideoExportOptions.MODE_COPY;

        [DependsOn(nameof(VideoMode))]
        public bool IsTranscodeMode => VideoMode == VideoExportOptions.MODE_TRANSCODE;

        /// <summary>选中的字幕下标，-1 表示不封装字幕</summary>
        [DependsOn(nameof(SelectedSubtitle))]
        public int SelectedSubtitleIndex => SelectedSubtitle?.Index ?? -1;

        [DependsOn(nameof(Format))]
        public Visibility Mp4PanelVisibility =>
            IsMp4 ? Visibility.Visible : Visibility.Collapsed;

        [DependsOn(nameof(Format), nameof(VideoMode))]
        public Visibility TranscodePanelVisibility =>
            IsMp4 && IsTranscodeMode ? Visibility.Visible : Visibility.Collapsed;

        [DependsOn(nameof(Format), nameof(VideoTracks))]
        public Visibility VideoTrackPanelVisibility =>
            IsMp4 && VideoTracks != null && VideoTracks.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

        [DependsOn(nameof(Format), nameof(AudioTracks))]
        public Visibility AudioTrackPanelVisibility =>
            IsMp4 && AudioTracks != null && AudioTracks.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

        [DependsOn(nameof(Format), nameof(SubtitleOptions))]
        public Visibility SubtitlePanelVisibility =>
            IsMp4 && SubtitleOptions != null && SubtitleOptions.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
    }
}
