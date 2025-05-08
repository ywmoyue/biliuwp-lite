
using BiliLite.Models;
using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Player.ShakaPlayer.Extensions
{
    public static class ShakaExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static async Task<string> CreateMpdFiles(this GenerateMPDModel model)
        {
            var mpdString = GenerateMpd(model);

            var tempFileService = App.ServiceProvider.GetRequiredService<TempFileService>();

            var tempFile = await tempFileService.CreateTempFile();

            await FileIO.WriteTextAsync(tempFile, mpdString);

            var folder = await tempFile.GetParentAsync();

            return tempFile.Path.Replace(folder.Path, "https://temp.bililte.service").Replace("\\", "/");
        }

        public static string GenerateMpd(GenerateMPDModel model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            // 对 URL 进行 XML 转义（替换 & 为 &amp;）
            string escapedVideoUrl = EscapeXmlUrl(model.VideoUrl);
            string escapedAudioUrl = EscapeXmlUrl(model.AudioUrl);

            // MPD模板字符串
            var mpdTemplate = new StringBuilder(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<MPD xmlns=""urn:mpeg:dash:schema:mpd:2011"" 
     xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
     xsi:schemaLocation=""urn:mpeg:dash:schema:mpd:2011 DASH-MPD.xsd""
     type=""dynamic""
     mediaPresentationDuration=""PT{0}S""
timeShiftBufferDepth=""PT1S"" minimumUpdatePeriod=""PT1H"" maxSegmentDuration=""PT3S"" minBufferTime=""PT1S"" 
profiles=""urn:mpeg:dash:profile:isoff-live:2011,urn:com:dashif:dash264"">
    <Period start=""PT0S"" duration=""PT{1}S"">
        {2}
    </Period>
</MPD>");

            var adaptationSets = new StringBuilder();
            bool hasVideo = !string.IsNullOrEmpty(escapedVideoUrl);
            bool hasAudio = !string.IsNullOrEmpty(escapedAudioUrl);

            // 视频AdaptationSet
            if (hasVideo)
            {
                var videoRepresentation = $@"
            <Representation id=""{model.VideoID}""
                           bandwidth=""{model.VideoBandwidth}""
                           codecs=""{model.VideoCodec}""
                           width=""{model.VideoWidth}""
                           height=""{model.VideoHeight}""
                           frameRate=""{model.VideoFrameRate}""
                           mimeType=""video/mp4"">
                <SegmentTemplate
                    timescale=""16000""
                    duration=""16000""
                    media=""{escapedVideoUrl}""
                    startNumber=""1""/>
            </Representation>";

                adaptationSets.Append($@"
        <AdaptationSet id=""1""
                      contentType=""video""
                      startWithSAP=""1""
                      segmentAlignment=""true""
                      subsegmentAlignment=""true""
                      subsegmentStartsWithSAP=""1"">
            {videoRepresentation}
        </AdaptationSet>");
            }

            // 音频AdaptationSet
            if (hasAudio)
            {
                var audioRepresentation = $@"
            <Representation id=""{model.AudioID}""
                           bandwidth=""{model.AudioBandwidth}""
                           codecs=""{model.AudioCodec}""
                           mimeType=""audio/mp4"">
                <SegmentTemplate
                    duration=""16000""
                    media=""{escapedAudioUrl}""
                    startNumber=""1""/>
            </Representation>";

                adaptationSets.Append($@"
        <AdaptationSet id=""2""
                      contentType=""audio""
                      startWithSAP=""1""
                      segmentAlignment=""true""
                      subsegmentAlignment=""true""
                      subsegmentStartsWithSAP=""1""
                      lang=""en"">
            {audioRepresentation}
        </AdaptationSet>");
            }

            // 计算持续时间（秒）
            double durationSeconds = model.DurationMS / 1000.0;

            // 替换模板中的占位符
            string mpdContent = string.Format(mpdTemplate.ToString(),
                durationSeconds,
                durationSeconds,
                adaptationSets.ToString());

            return mpdContent;
        }

        /// <summary>
        /// 转义 XML 中的特殊字符（如 & 转为 &amp;）
        /// </summary>
        private static string EscapeXmlUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;

            // 先替换 & 为 &amp;
            url = url.Replace("&", "&amp;");

            // 确保 &amp;amp; 不会重复转义
            url = url.Replace("&amp;amp;", "&amp;");

            return url;
        }
    }
}
