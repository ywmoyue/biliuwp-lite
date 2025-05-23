﻿using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Responses;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliLite.Extensions
{
    public static class BiliExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 根据Epid取番剧ID
        /// </summary>
        /// <returns></returns>
        public static async Task<string> BangumiEpidToSid(string epid)
        {
            try
            {
                var results = await new SeasonApi().Detail(epid, SeasonIdType.EpId).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }

                //访问番剧详情
                var data = await results.GetJson<ApiDataModel<SeasonDetailModel>>();

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                return data.data.SeasonId.ToString();
            }
            catch (Exception ex)
            {
                _logger.Error("转换epId到seasonId错误", ex);
                return "";
            }
        }

        /// <summary>
        /// 短链接还原
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetShortLinkLocation(string shortlink)
        {
            try
            {
                HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false,
                };
                using (HttpClient client = new HttpClient(httpMessageHandler))
                {
                    var response = await client.GetAsync(shortlink);
                    return response.Headers.Location.ToString();
                }
            }
            catch (Exception)
            {
                return shortlink;
            }
        }

        /// <summary>
        /// 默认一些请求头
        /// </summary>
        /// <returns></returns>
        public static IDictionary<string, string> GetDefaultHeaders()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("user-agent", "Mozilla/5.0 BiliDroid/5.34.1 (bbcallen@gmail.com)");
            headers.Add("Referer", "https://www.bilibili.com/");
            return headers;
        }

        public static async Task<bool> ActionCheckLogin()
        {
            if (SettingService.Account.Logined || await NotificationShowExtensions.ShowLoginDialog()) return true;
            NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
            return false;
        }

        public static async Task CheckVersion(bool isSilentUpdateCheck = false)
        {
            try
            {
                var num = $"{SystemInformation.ApplicationVersion.Major}{SystemInformation.ApplicationVersion.Minor.ToString("00")}{SystemInformation.ApplicationVersion.Build.ToString("00")}";
                _logger.Log($"BiliLite.UWP version: {num}", LogType.Necessary);
                var result = await new GitApi().CheckUpdate().Request();
                if (result == null || string.IsNullOrEmpty(result.results)) throw new Exception("请求更新信息失败");
                var ver = JsonConvert.DeserializeObject<NewVersionResponse>(result.results);
                var ignoreVersion = SettingService.GetValue(SettingConstants.Other.IGNORE_VERSION, "");
                if (ignoreVersion.Equals(ver.Version)) return;
                var v = int.Parse(num);
                if (ver.VersionNum > v)
                {
                    var dialog = new ContentDialog();

                    dialog.Title = $"发现新版本 Ver {ver.Version}";
                    MarkdownTextBlock markdownText = new MarkdownTextBlock()
                    {
                        Text = ver.VersionDesc,
                        TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap,
                        IsTextSelectionEnabled = true,
                        Background = new SolidColorBrush(Colors.Transparent)
                    };
                    markdownText.LinkClicked += new EventHandler<LinkClickedEventArgs>(async (sender, args) =>
                    {
                        await Launcher.LaunchUriAsync(new Uri(args.Link));
                    });
                    dialog.Content = markdownText;
                    dialog.PrimaryButtonText = "查看详情";
                    dialog.CloseButtonText = "取消";
                    dialog.SecondaryButtonText = "忽略该版本";

                    dialog.PrimaryButtonClick += new Windows.Foundation.TypedEventHandler<ContentDialog, ContentDialogButtonClickEventArgs>(async (sender, e) =>
                    {
                        await Launcher.LaunchUriAsync(new Uri(ver.Url));
                    });
                    dialog.SecondaryButtonClick += (sender, e) =>
                    {
                        SettingService.SetValue(SettingConstants.Other.IGNORE_VERSION, ver.Version);
                    };
                    await dialog.ShowAsync();
                }
                else
                {
                    if (!isSilentUpdateCheck) NotificationShowExtensions.ShowMessageToast($"LatestRelease版本：Ver {ver.Version}；当前已是最新或更新版本");
                }
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("检查更新失败；请检查日志");
                _logger.Error($"检查更新失败：{ex.Message}", ex);
            }
        }
    }
}
