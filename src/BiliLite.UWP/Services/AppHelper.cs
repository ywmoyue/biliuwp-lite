using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Requests.Api;
using BiliLite.VideoExporter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.Services
{
    public static class AppHelper
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 导出任务文件名写在 LocalSettings 里，导出器进程读取后自行删除
        /// </summary>
        public const string EXPORT_JOB_FILE_SETTING_KEY = "VideoExportJobFile";

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

        public static async Task LaunchConverter(IReadOnlyList<VideoExportJob> jobs, bool closeOnSuccess)
        {
            if (jobs == null || jobs.Count == 0) return;

            try
            {
                var payload = new
                {
                    closeOnSuccess,
                    items = jobs.Select(job => new
                    {
                        title = job.Title,
                        outFile = job.OutFile,
                        format = job.Options.Format,
                        videoMode = job.Options.VideoMode,
                        videoTracks = job.Options.VideoTracks.Select(x => new
                        {
                            path = x.Path,
                            qualityId = x.QualityId,
                            codecId = x.CodecId,
                            label = x.Label,
                        }),
                        audioTracks = job.Options.AudioTracks.Select(x => new
                        {
                            path = x.Path,
                            qualityId = x.QualityId,
                            codecId = x.CodecId,
                            label = x.Label,
                        }),
                        subtitleTracks = job.Options.SubtitleTracks.Select(x => new
                        {
                            path = x.Path,
                            label = x.Label,
                        }),
                        transcode = new
                        {
                            hdrToSdr = job.Options.HdrToSdr,
                            downscale = job.Options.Downscale,
                        },
                    }).ToList(),
                };

                var json = JsonConvert.SerializeObject(payload);
                logger.Debug("videoExportJobs: " + json);

                // 批量导出的任务可能很大，走文件传递而不是 LocalSettings（单个值有大小限制）
                // winui3 的导出器与主程序同进程：写完文件、设好键后直接开窗
                var jobFileName = $"video-export-jobs-{Guid.NewGuid():N}.json";
                var jobFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(jobFileName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(jobFile, json);
                ApplicationData.Current.LocalSettings.Values[EXPORT_JOB_FILE_SETTING_KEY] = jobFileName;

                var exporterWindow = new VideoExporterWindow();
                exporterWindow.Activate();
                exporterWindow.Start();
            }
            catch (Exception ex)
            {
                logger.Log("启动视频导出进程失败", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("启动导出进程失败，已记录错误");
            }
        }
    }
}
