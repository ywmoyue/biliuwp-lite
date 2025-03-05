using System.Collections.Generic;
using System.IO;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Common.Video;

namespace BiliLite.Extensions;

public static class DownloadExtensions
{
    public static List<DownloadTrackInfo> GetVideoDownloadTrackInfoList(this DownloadedSubItem downloadedSubItem)
    {
        return GetDownloadTrackInfoList(downloadedSubItem, "video");
    }

    public static List<DownloadTrackInfo> GetAudioDownloadTrackInfoList(this DownloadedSubItem downloadedSubItem)
    {
        return GetDownloadTrackInfoList(downloadedSubItem, "audio");
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
