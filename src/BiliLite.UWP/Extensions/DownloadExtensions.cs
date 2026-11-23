using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Common.Video;
using BiliLite.Services;

namespace BiliLite.Extensions;

public static class DownloadExtensions
{
    /// <summary>
    /// 默认音质优先级，与播放时保持一致（见 SoundQualityConstants.GetDefaultAudio）
    /// </summary>
    private static readonly int[] _defaultAudioQualityOrder = { 30280, 30232, 30216, 30251, 30250 };

    private static readonly int[] _maxAudioQualityOrder = { 30251, 30250, 30280, 30232, 30216 };
    public static List<DownloadTrackInfo> GetVideoDownloadTrackInfoList(this DownloadedSubItem downloadedSubItem)
    {
        return GetDownloadTrackInfoList(downloadedSubItem, "video");
    }

    public static List<DownloadTrackInfo> GetAudioDownloadTrackInfoList(this DownloadedSubItem downloadedSubItem)
    {
        return GetDownloadTrackInfoList(downloadedSubItem, "audio");
    }

    /// <summary>
    /// 导出用的视频轨选项，顺序与下载时的 Paths 一致（保持 MKV 「全部轨道」的输出顺序不变）。
    /// 非 DASH（老下载）没有 video-/audio- 命名，整份文件算一条视频轨。
    /// </summary>
    public static List<VideoExportTrackOption> GetVideoExportTrackOptions(this DownloadedSubItem downloadedSubItem)
    {
        var results = new List<VideoExportTrackOption>();
        if (downloadedSubItem?.Paths == null || downloadedSubItem.Paths.Count == 0) return results;

        if (downloadedSubItem.IsDash)
        {
            foreach (var path in downloadedSubItem.Paths)
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (!IsTrackFile(fileName, "video")) continue;
                var trackInfo = ParseTrackInfo(fileName, "video");
                results.Add(new VideoExportTrackOption
                {
                    Path = path,
                    QualityId = trackInfo.QualityId,
                    CodecId = trackInfo.CodecId,
                    Label = GetTrackName(trackInfo, false),
                    IsDefault = results.Count == 0,
                });
            }
        }

        if (results.Count == 0)
        {
            // 非 DASH：单个文件里同时含音视频（MultiFLV 的分段沿用原有行为，只取第一个）
            results.Add(new VideoExportTrackOption
            {
                Path = downloadedSubItem.Paths.First(),
                QualityId = -1,
                CodecId = -1,
                Label = string.IsNullOrEmpty(downloadedSubItem.QualityName) ? "视频" : downloadedSubItem.QualityName,
                IsDefault = true,
            });
        }

        return results;
    }

    /// <summary>
    /// 导出用的音频轨选项。非 DASH 时返回空列表，导出器会把音频从视频文件里取。
    /// </summary>
    public static List<VideoExportTrackOption> GetAudioExportTrackOptions(this DownloadedSubItem downloadedSubItem)
    {
        var results = new List<VideoExportTrackOption>();
        if (downloadedSubItem?.Paths == null || !downloadedSubItem.IsDash) return results;

        foreach (var path in downloadedSubItem.Paths)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (!IsTrackFile(fileName, "audio")) continue;
            var trackInfo = ParseTrackInfo(fileName, "audio");
            results.Add(new VideoExportTrackOption
            {
                Path = path,
                QualityId = trackInfo.QualityId,
                CodecId = trackInfo.CodecId,
                Label = GetTrackName(trackInfo, true),
            });
        }

        MarkDefaultAudio(results);
        return results;
    }

    /// <summary>
    /// 按播放时的音质优先级挑默认音频轨。
    /// </summary>
    private static void MarkDefaultAudio(List<VideoExportTrackOption> options)
    {
        if (options.Count == 0) return;

        var enableMaxQuality = SettingService.GetValue(
            SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY,
            SettingConstants.Player.DEFAULT_ENABLE_DEFAULT_MAX_SOUND_QUALITY);

        var preferredQualityIds = enableMaxQuality ? _maxAudioQualityOrder : _defaultAudioQualityOrder;

        VideoExportTrackOption selected = null;
        foreach (var qualityId in preferredQualityIds)
        {
            selected = options.FirstOrDefault(x => x.QualityId == qualityId);
            if (selected != null) break;
        }

        selected ??= options.First();
        selected.IsDefault = true;
    }

    private static bool IsTrackFile(string fileName, string prefix)
    {
        return !string.IsNullOrEmpty(fileName) &&
               fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    public static (int TrackId, string TrackName) GetDownloadedTrackIdName(this string downloadedTrackPath, bool isAudio = false)
    {
        var prefix = isAudio ? "audio" : "video";
        var fileName = Path.GetFileNameWithoutExtension(downloadedTrackPath);
        var trackInfo = ParseTrackInfo(fileName, prefix);

        if (trackInfo.QualityId == -1)
        {
            return (-1, "未知");
        }

        var trackId = trackInfo.QualityId * trackInfo.CodecId;
        var trackName = GetTrackName(trackInfo, isAudio);

        return (trackId, trackName);
    }

    public static int CodecModeToCodecId(this PlayUrlCodecMode mode)
    {
        return mode switch
        {
            PlayUrlCodecMode.DASH_AV1 => 13,
            PlayUrlCodecMode.DASH_H264 => 7,
            PlayUrlCodecMode.DASH_H265 => 12,
            _ => -1
        };
    }

    private static string CodecIdToCodecName(int id)
    {
        return id switch
        {
            13 => "AV1",
            7 => "H264",
            12 => "H265",
            _ => "未知"
        };
    }

    private static List<DownloadTrackInfo> GetDownloadTrackInfoList(DownloadedSubItem downloadedSubItem, string prefix)
    {
        var results = new List<DownloadTrackInfo>();
        foreach (var path in downloadedSubItem.Paths)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var trackInfo = ParseTrackInfo(fileName, prefix);
            results.Add(trackInfo);
        }

        return results;
    }

    private static DownloadTrackInfo ParseTrackInfo(string fileName, string prefix)
    {
        var trackInfo = new DownloadTrackInfo();
        var parts = fileName.Split('-');

        if (parts.Length == 3 && parts[0] == prefix) // 格式为 {prefix}-{QualityId}-{CodecId}
        {
            if (int.TryParse(parts[1], out var qualityId) && int.TryParse(parts[2], out var codecId))
            {
                trackInfo.QualityId = qualityId;
                trackInfo.CodecId = codecId;
            }
        }
        else if (fileName == prefix) // 格式为 {prefix}.m4s
        {
            trackInfo.QualityId = -1;
            trackInfo.CodecId = -1;
        }

        return trackInfo;
    }

    private static string GetTrackName(DownloadTrackInfo trackInfo, bool isAudio)
    {
        var trackName = string.Empty;
        var qualityName = isAudio
            ? SoundQualityConstants.Dictionary.GetValueOrDefault(trackInfo.QualityId, "未知")
            : QualityConstants.Dictionary.GetValueOrDefault(trackInfo.QualityId, "未知");

        trackName = qualityName;

        if (!isAudio)
        {
            trackName += CodecIdToCodecName(trackInfo.CodecId);
        }

        return trackName;
    }
}
