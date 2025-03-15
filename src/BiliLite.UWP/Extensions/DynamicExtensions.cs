using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.UserDynamic;
using System;
using BiliLite.Services.Biz;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Dialogs;
using BiliLite.Pages.User;

namespace BiliLite.Extensions
{
    public static class DynamicExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static void OpenUserEx(this IUserDynamicCommands dynamic, object parameter)
        {
            long userId = 0;
            if (parameter is string userLink)
            {
                var regex = new Regex("bilibili://space/(\\d+)");
                var match = regex.Match(userLink.ToString());
                userId = !match.Success ? userLink.ToInt64() : match.Groups[1].Value.ToInt64();
            }

            else if (parameter is DynamicV2ItemViewModel dynamicItem)
            {
                if (dynamicItem.CardType == Constants.DynamicTypes.PGC)
                {
                    var url = dynamicItem.Dynamic.DynPgc != null
                        ? dynamicItem.Dynamic.DynPgc.Uri
                        : dynamicItem.Dynamic.DynArchive.Uri;
                    dynamic.LaunchUrlEx(url);
                    return;
                }

                if (dynamicItem.Author != null)
                {
                    userId = dynamicItem.Author.Author.Mid;
                }
                else if (dynamicItem.AuthorForward != null)
                {
                    userId = dynamicItem.AuthorForward.Uid;
                }
            }

            MessageCenter.NavigateToPage(dynamic, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = userId
            });
        }

        public static async Task LaunchUrlEx(this IUserDynamicCommands dynamic, string url)
        {
            var result = await MessageCenter.HandelUrl(url);
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
        }

        public static async void LaunchUrl(string url)
        {
            var result = await MessageCenter.HandelUrl(url);
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
        }

        public static void OpenImage(object data)
        {
            if (!(data is UserDynamicItemDisplayImageInfo imageInfo))
            {
                return;
            }
            MessageCenter.OpenImageViewer(imageInfo.AllImages, imageInfo.Index);
        }

        public static void CopyDyn(DynamicV2ItemViewModel data)
        {
            var dataStr = data.SourceJson;
            Notify.ShowMessageToast(dataStr.SetClipboard() ? "已复制" : "复制失败");
        }

        public static void OpenWebDetail(string dynId)
        {
            var url = $"https://www.bilibili.com/opus/{dynId}";
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = "加载中...",
                parameters = url
            });
        }

        public static async void DoLike(DynamicV2ItemViewModel item)
        {
            await App.ServiceProvider.GetRequiredService<UserDynamicService>().DoLike(item);
        }

        public static async void OpenSendDynamicDialog(DynamicV2ItemViewModel data)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            var sendDynamicDialog = App.ServiceProvider.GetRequiredService<SendDynamicV2Dialog>();
            if (data != null)
            {
                sendDynamicDialog.SetRepost(data);
            }
            await sendDynamicDialog.ShowAsync();
        }

        public static void OpenTag(object name)
        {
            //TODO 打开话题
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = name.ToString(),
                parameters = "https://t.bilibili.com/topic/name/" + Uri.EscapeDataString(name.ToString())
            });
        }

        public static void OpenDetail(string dynId)
        {
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(DynamicDetailPage),
                title = "动态详情",
                parameters = dynId
            });
        }
    }
}
