using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.UserDynamic;

namespace BiliLite.Extensions
{
    public static class DynamicExtensions
    {
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
    }
}
