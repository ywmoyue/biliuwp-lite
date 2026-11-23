using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BiliLite.Win32Tools
{
    /// <summary>
    /// 视频轨/音频轨/字幕轨
    /// </summary>
    public class ConvertTrackInfo
    {
        /// <summary>绝对路径</summary>
        public string Path { get; set; }

        public int QualityId { get; set; }

        public int CodecId { get; set; }

        /// <summary>界面显示用的轨道名</summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// 转码选项，仅 format=mp4 且 videoMode=transcode 时生效
    /// </summary>
    public class ConvertTranscodeOptions
    {
        /// <summary>是否把 HDR 转成 SDR</summary>
        public bool HdrToSdr { get; set; }

        /// <summary>降分辨率目标："source" | "1080" | "720"</summary>
        public string Downscale { get; set; }
    }

    /// <summary>
    /// 导出任务描述。InputFiles/Subtitle/IsDash 仅作 v1 回退兼容保留。
    /// </summary>
    public class ConvertFileInfo
    {
        public string Title { get; set; }

        public List<string> InputFiles { get; set; }

        public List<string> Subtitle { get; set; }

        public string OutFile { get; set; }

        public bool IsDash { get; set; }

        /// <summary>"mp4" | "mkv"</summary>
        public string Format { get; set; }

        /// <summary>"copy" | "transcode"，仅 mp4 有意义</summary>
        public string VideoMode { get; set; }

        public List<ConvertTrackInfo> VideoTracks { get; set; }

        public List<ConvertTrackInfo> AudioTracks { get; set; }

        public List<ConvertTrackInfo> SubtitleTracks { get; set; }

        public ConvertTranscodeOptions Transcode { get; set; }
    }

    /// <summary>
    /// 拼装好的导出参数：输入文件顺序 + 输出参数顺序。
    /// </summary>
    public class VideoExportPlan
    {
        public string Format { get; set; }

        /// <summary>是否走转码（仅 mp4 可能为 true）</summary>
        public bool Transcode { get; set; }

        public string OutputFile { get; set; }

        public List<string> Inputs { get; private set; }

        public List<string> OutputArgs { get; private set; }

        public VideoExportPlan()
        {
            Inputs = new List<string>();
            OutputArgs = new List<string>();
        }

        /// <summary>
        /// 还原成可读的 ffmpeg 命令行，用于日志和脱离界面的验证。
        /// </summary>
        public string ToCommandLine(string ffmpegExe)
        {
            var parts = new List<string> { Quote(ffmpegExe) };
            foreach (var input in Inputs)
            {
                parts.Add("-i");
                parts.Add(Quote(input));
            }
            parts.AddRange(OutputArgs);
            parts.Add(Quote(OutputFile));
            return string.Join(" ", parts);
        }

        private static string Quote(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";
            return value.IndexOf(' ') >= 0 ? "\"" + value + "\"" : value;
        }
    }

    /// <summary>
    /// 导出参数拼装：不做源编码判定，MKV 保留全部轨道，MP4 只保留 1 条视频轨、1 条音频轨、1 条字幕轨
    /// </summary>
    public static class VideoExportArgs
    {
        public const string FORMAT_MP4 = "mp4";
        public const string FORMAT_MKV = "mkv";

        public const string MODE_COPY = "copy";
        public const string MODE_TRANSCODE = "transcode";

        /// <summary>HDR -> SDR 滤镜链</summary>
        private const string TONEMAP_FILTER =
            "zscale=t=linear:npl=100,format=gbrpf32le,zscale=p=bt709,tonemap=tonemap=hable:desat=0,zscale=t=bt709:m=bt709:r=tv,format=yuv420p";

        public static VideoExportPlan Build(ConvertFileInfo info)
        {
            if (info == null) throw new ArgumentNullException("info");
            if (string.IsNullOrWhiteSpace(info.OutFile)) throw new InvalidOperationException("输出文件为空");

            var videos = ResolvePaths(info, info.VideoTracks, "video");
            var audios = ResolvePaths(info, info.AudioTracks, "audio");
            var subtitles = ResolvePaths(info, info.SubtitleTracks, "subtitle");

            if (videos.Count == 0) throw new InvalidOperationException("视频为空，无法导出");

            var plan = new VideoExportPlan
            {
                Format = ResolveFormat(info),
                OutputFile = info.OutFile
            };

            if (plan.Format == FORMAT_MKV)
            {
                BuildMkv(plan, videos, audios, subtitles);
            }
            else
            {
                BuildMp4(plan, info, videos, audios, subtitles);
            }

            return plan;
        }

        /// <summary>
        /// 优先用 v2 显式轨道；没有则按文件名前缀从 v1 的 InputFiles 里回退挑
        /// </summary>
        private static List<string> ResolvePaths(ConvertFileInfo info, List<ConvertTrackInfo> tracks, string kind)
        {
            var result = new List<string>();
            if (tracks != null)
            {
                foreach (var track in tracks)
                {
                    if (track != null && !string.IsNullOrWhiteSpace(track.Path)) result.Add(track.Path);
                }
            }
            if (result.Count > 0) return result;

            if (kind == "subtitle")
            {
                return info.Subtitle == null ? result : new List<string>(info.Subtitle);
            }

            if (info.InputFiles == null) return result;
            foreach (var file in info.InputFiles)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (name != null && name.StartsWith(kind, StringComparison.OrdinalIgnoreCase)) result.Add(file);
            }

            // 非 DASH：单个文件里同时含音视频
            if (result.Count == 0 && kind == "video" && info.InputFiles.Count > 0)
            {
                result.Add(info.InputFiles[0]);
            }

            return result;
        }

        private static string ResolveFormat(ConvertFileInfo info)
        {
            if (!string.IsNullOrWhiteSpace(info.Format))
            {
                var format = info.Format.Trim().ToLowerInvariant();
                if (format == FORMAT_MP4 || format == FORMAT_MKV) return format;
            }
            var extension = Path.GetExtension(info.OutFile ?? string.Empty).ToLowerInvariant();
            return extension == ".mp4" ? FORMAT_MP4 : FORMAT_MKV;
        }

        /// <summary>MKV：全部轨道，行为与改造前一致。</summary>
        private static void BuildMkv(VideoExportPlan plan, List<string> videos, List<string> audios, List<string> subtitles)
        {
            plan.Inputs.AddRange(videos);
            plan.Inputs.AddRange(audios);
            plan.Inputs.AddRange(subtitles);

            for (var i = 0; i < videos.Count; i++) Add(plan, "-map", i + ":v:0");
            if (audios.Count > 0)
            {
                for (var i = 0; i < audios.Count; i++) Add(plan, "-map", (videos.Count + i) + ":a:0");
            }
            else
            {
                // 音频和视频在同一个文件里（非 DASH）
                Add(plan, "-map", "0:a:0?");
            }
            for (var i = 0; i < subtitles.Count; i++) Add(plan, "-map", (videos.Count + audios.Count + i) + ":s:0");

            Add(plan, "-c:v", "copy");
            Add(plan, "-c:a", "copy");
            if (subtitles.Count > 0) Add(plan, "-c:s", "copy");
            Add(plan, "-strict", "-2");
            Add(plan, "-f", "matroska");
        }

        /// <summary>MP4：只保留 1 条视频轨、1 条音频轨、1 条字幕轨。</summary>
        private static void BuildMp4(VideoExportPlan plan, ConvertFileInfo info, List<string> videos,
            List<string> audios, List<string> subtitles)
        {
            var audio = audios.Count > 0 ? audios[0] : null;
            var subtitle = subtitles.Count > 0 ? subtitles[0] : null;

            plan.Inputs.Add(videos[0]);
            if (audio != null) plan.Inputs.Add(audio);
            if (subtitle != null) plan.Inputs.Add(subtitle);

            var videoIndex = 0;
            // 音频和视频在同一个文件里时，音频也用第 0 个输入
            var audioIndex = audio != null ? 1 : videoIndex;
            var subtitleIndex = subtitle != null ? plan.Inputs.Count - 1 : -1;

            Add(plan, "-map", videoIndex + ":v:0");
            Add(plan, "-map", audio != null ? audioIndex + ":a:0" : videoIndex + ":a:0?");
            if (subtitle != null) Add(plan, "-map", subtitleIndex + ":s:0");

            plan.Transcode = IsTranscode(info);

            if (plan.Transcode)
            {
                var filters = BuildFilters(info);
                if (filters != null) Add(plan, "-vf", filters);

                Add(plan, "-c:v", "libx264");
                Add(plan, "-preset", "veryfast");
                var bitrate = ResolveVideoBitrate(info);
                Add(plan, "-b:v", bitrate + "k");
                Add(plan, "-maxrate", (bitrate * 3 / 2) + "k");
                Add(plan, "-bufsize", (bitrate * 3) + "k");
                Add(plan, "-pix_fmt", "yuv420p");
                Add(plan, "-c:a", "aac");
                Add(plan, "-b:a", "192k");
            }
            else
            {
                Add(plan, "-c:v", "copy");
                Add(plan, "-c:a", "copy");
                // 兜住 FLAC 之类的非标准 MP4 组合
                Add(plan, "-strict", "-2");
            }

            if (subtitle != null)
            {
                Add(plan, "-c:s", "mov_text");
            }

            Add(plan, "-movflags", "+faststart");
            Add(plan, "-f", "mp4");
        }

        private static bool IsTranscode(ConvertFileInfo info)
        {
            return (info.VideoMode ?? string.Empty).Trim().ToLowerInvariant() == MODE_TRANSCODE;
        }

        /// <summary>只在需要时返回滤镜链，否则让 -pix_fmt 处理位深</summary>
        private static string BuildFilters(ConvertFileInfo info)
        {
            var options = info.Transcode;
            var filters = new List<string>();

            if (options != null && options.HdrToSdr) filters.Add(TONEMAP_FILTER);
            var scale = ResolveScale(options);
            if (scale != null) filters.Add(scale);

            return filters.Count == 0 ? null : string.Join(",", filters);
        }

        private static string ResolveScale(ConvertTranscodeOptions options)
        {
            if (options == null || string.IsNullOrWhiteSpace(options.Downscale)) return null;
            switch (options.Downscale.Trim().ToLowerInvariant())
            {
                case "1080": return "scale=1920:-2";
                case "720": return "scale=1280:-2";
                default: return null; // "source" 或未知值 -> 保持原始分辨率
            }
        }

        /// <summary>
        /// 转码码率（kbps）。固定码率而不是 -crf，避免引入源编码判定
        /// </summary>
        private static int ResolveVideoBitrate(ConvertFileInfo info)
        {
            var options = info.Transcode;
            if (options != null)
            {
                var scale = (options.Downscale ?? string.Empty).Trim().ToLowerInvariant();
                if (scale == "1080") return 8000;
                if (scale == "720") return 4000;
            }

            var qualityId = 0;
            if (info.VideoTracks != null && info.VideoTracks.Count > 0 && info.VideoTracks[0] != null)
            {
                qualityId = info.VideoTracks[0].QualityId;
            }

            if (qualityId >= 127) return 40000;  // 8K 超高清
            if (qualityId >= 120) return 25000;  // 4K 超清 / HDR 真彩色 / 杜比视界
            if (qualityId >= 112) return 12000;  // 1080P60 高帧率 / 1080P+ 高码率
            if (qualityId >= 80) return 8000;    // 1080P 高清
            if (qualityId >= 64) return 4000;    // 720P 高清
            if (qualityId > 0) return 2000;      // 更低清晰度
            return 8000;                         // 未知（老下载）取保守值
        }

        private static void Add(VideoExportPlan plan, string name, string value)
        {
            plan.OutputArgs.Add(name);
            plan.OutputArgs.Add(value);
        }
    }
}
