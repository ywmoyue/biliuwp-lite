using BiliLite.Models.Requests.Api;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using System.IO;
using System.Threading;

namespace BiliLite.Services
{
    public static class AppHelper
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public static List<RegionItem> Regions { get; set; }
        private static RegionAPI regionAPI = new RegionAPI();

        public static async Task<List<RegionItem>> GetDefaultRegions()
        {
            try
            {
                var str = await FileIO.ReadTextAsync(
                    await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Text/regions.json")));
                return JsonConvert.DeserializeObject<List<RegionItem>>(str);
            }
            catch (Exception ex)
            {
                logger.Log("读取默认分区失败！" + ex.Message, LogType.Error, ex);
                return new List<RegionItem>();
            }
        }

        public static async Task SetRegions()
        {
            try
            {
                var results = await regionAPI.Regions().Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<List<RegionItem>>(data["data"].ToString()
                            .Replace("goto", "_goto"));
                        foreach (var item in ls.Where(x =>
                                         string.IsNullOrEmpty(x.Uri) || x.Name == "会员购" || x.Name == "漫画" ||
                                         x.Name == "游戏中心" || x.Name == "话题中心" || x.Name == "音频" || x.Name == "原创排行榜")
                                     .ToList())
                        {
                            ls.Remove(item);
                        }

                        Regions = ls;
                    }
                    else
                    {
                        //var str =await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Text/regions.json")));
                        Regions = await AppHelper.GetDefaultRegions();
                    }
                }
                else
                {
                    Regions = await AppHelper.GetDefaultRegions();

                }
            }
            catch (Exception ex)
            {
                Regions = await AppHelper.GetDefaultRegions();
                logger.Log("读取分区失败" + ex.Message, LogType.Error, ex);
            }
        }

        public static async Task LaunchConverter(string title, List<string> inputFiles, string outFile,
            List<string> subtitle, bool isDash)
        {
            var videoConverterInfo = JsonConvert.SerializeObject(new
            {
                title = title,
                inputFiles = inputFiles,
                outFile = outFile,
                subtitle = subtitle,
                isDash = isDash
            });
            logger.Debug("videoConverterInfo: " + videoConverterInfo);
            ApplicationData.Current.LocalSettings.Values["VideoConverterInfo"] = videoConverterInfo;
            await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
        }

        public static async Task<string> NormalizeAudioWithFfmpegAsync(string inputFile, double targetLufs,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
            {
                return null;
            }

            var operationId = Guid.NewGuid().ToString("N");
            var resultKey = $"AudioNormalizeResult_{operationId}";

            var payload = JsonConvert.SerializeObject(new
            {
                operationId,
                inputFile,
                targetLufs
            });

            ApplicationData.Current.LocalSettings.Values["AudioNormalizeRequest"] = payload;
            ApplicationData.Current.LocalSettings.Values.Remove(resultKey);

            try
            {
                await Windows.ApplicationModel.FullTrustProcessLauncher.LaunchFullTrustProcessForCurrentAppAsync();
            }
            catch (Exception ex)
            {
                logger.Warn("Launch fulltrust process for audio normalization failed", ex);
                return null;
            }

            var timeoutAt = DateTime.UtcNow.AddSeconds(90);
            while (DateTime.UtcNow < timeoutAt)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var resultStr = ApplicationData.Current.LocalSettings.Values[resultKey] as string;
                if (!string.IsNullOrWhiteSpace(resultStr))
                {
                    ApplicationData.Current.LocalSettings.Values.Remove(resultKey);
                    try
                    {
                        var result = JsonConvert.DeserializeAnonymousType(resultStr, new
                        {
                            success = false,
                            outputFile = string.Empty,
                            error = string.Empty
                        });

                        if (result?.success == true && !string.IsNullOrWhiteSpace(result.outputFile) && File.Exists(result.outputFile))
                        {
                            return result.outputFile;
                        }

                        logger.Warn($"Audio normalization failed in fulltrust process: {result?.error}");
                        return null;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn("Parse audio normalization result failed", ex);
                        return null;
                    }
                }

                await Task.Delay(250, cancellationToken);
            }

            logger.Warn("Audio normalization timeout");
            return null;
        }
    }
}
