using System.Collections.Generic;

namespace BiliLite.Models.Common.Download
{
    /// <summary>
    /// 导出时的单条轨道选项。
    /// </summary>
    public class VideoExportTrackOption
    {
        public string Path { get; set; } = string.Empty;

        public int QualityId { get; set; }

        public int CodecId { get; set; }

        /// <summary>界面显示用的轨道名</summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>多轨时默认选中哪一条</summary>
        public bool IsDefault { get; set; }

        public override string ToString() => Label;
    }

    /// <summary>
    /// 导出选项。字段名要和导出器进程的 ConvertFileInfo 保持一致
    /// （见 BiliLite.Win32Tools/VideoExportArgs.cs、BiliLite.VideoExporter/VideoExportWindow.xaml.cs）。
    /// </summary>
    public class VideoExportOptions
    {
        public const string FORMAT_MP4 = "mp4";
        public const string FORMAT_MKV = "mkv";

        public const string MODE_COPY = "copy";
        public const string MODE_TRANSCODE = "transcode";

        public const string DOWNSCALE_SOURCE = "source";
        public const string DOWNSCALE_1080 = "1080";
        public const string DOWNSCALE_720 = "720";

        /// <summary>"mp4" | "mkv"。默认与 SettingConstants.Download.DEFAULT_EXPORT_FORMAT 一致</summary>
        public string Format { get; set; } = FORMAT_MKV;

        /// <summary>"copy" | "transcode"，仅 mp4 有意义</summary>
        public string VideoMode { get; set; } = MODE_COPY;

        /// <summary>是否把 HDR 转成 SDR，由用户勾选</summary>
        public bool HdrToSdr { get; set; }

        /// <summary>"source" | "1080" | "720"</summary>
        public string Downscale { get; set; } = DOWNSCALE_SOURCE;

        public List<VideoExportTrackOption> VideoTracks { get; set; } = new();

        public List<VideoExportTrackOption> AudioTracks { get; set; } = new();

        public List<VideoExportTrackOption> SubtitleTracks { get; set; } = new();

        public bool IsMp4 => Format == FORMAT_MP4;

        public bool IsTranscode => IsMp4 && VideoMode == MODE_TRANSCODE;
    }
}
