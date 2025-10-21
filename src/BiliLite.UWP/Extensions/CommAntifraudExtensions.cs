using System;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Requests.Api;
using Newtonsoft.Json;

namespace BiliLite.Extensions;

public static class CommAntifraudExtensions
{
    public static async Task CheckReplyAsync(string oid,
        int type,
        string id,
        string message,
        string root,
        bool isManual = false,
        object sourceId = null)
    {
        // 非手动检查时延迟8秒
        if (!isManual)
        {
            await Task.Delay(TimeSpan.FromSeconds(8));
        }

        // 主评论检查
        if (long.Parse(root) == 0)
        {
            await CheckRootReply(oid, type, id, message, isManual);
        }
        else
        {
            await CheckChildReply(oid, type, id, message, root);
        }
    }

    private static async Task CheckRootReply(string oid, int type, string id, string message, bool isManual)
    {
        var api = new CommentApi();
        // 无cookie检查
        var noCookieRequest = await api.CommentV2(oid.ToString(), CommentApi.CommentSort.New, 1, type, login: false);
        var noCookieResponse = await noCookieRequest.Request();

        if (!noCookieResponse.status)
        {
            NotificationShowExtensions.ShowMessageToast($"获取评论主列表时发生错误：{noCookieResponse.message}");
            return;
        }

        if (noCookieResponse.status)
        {
            var dataCommentModel = JsonConvert.DeserializeObject<DataCommentModel>(noCookieResponse.results);

            var replies = dataCommentModel.Replies;
            var index = replies?.FindIndex(r => r.RpId.ToString() == id) ?? -1;

            if (index != -1)
            {
                // 找到评论
                NotificationShowExtensions.ShowCommAntifraudDialog($"无账号状态下找到了你的评论，评论正常！\n\n你的评论：{message}");
                return;
            }

            // 未找到评论，进行带cookie检查
            var cookieResponse = await api.Reply(oid, id, 1, type).Request();

            if (!cookieResponse.status)
            {
                // 带cookie也没找到
                NotificationShowExtensions.ShowCommAntifraudDialog($"无法找到你的评论。\n\n你的评论：{message}");
                return;
            }

            // 带cookie能找到，检查无cookie是否能获取到
            var noCookieDetailResponse = await api.Reply(oid, id, 1, type).Request();

            if (!noCookieDetailResponse.status)
            {
                // 无cookie获取不到
                var isShadowBan = noCookieDetailResponse.message?.StartsWith("12022") == true;
                NotificationShowExtensions.ShowCommAntifraudDialog(
                    isShadowBan
                        ? $"你的评论被shadow ban（仅自己可见）！\n\n你的评论: {message}"
                        : $"评论不可见({noCookieDetailResponse.message}): {message}");
            }
            else
            {
                // 无cookie也能获取到
                NotificationShowExtensions.ShowCommAntifraudDialog(
                    isManual
                        ? $"无账号状态下找到了你的评论，评论正常！\n\n你的评论：{message}"
                        : $"你评论状态有点可疑，虽然无账号翻找评论区获取不到你的评论，但是无账号可通过\n" +
                          $"https://api.bilibili.com/x/v2/reply/reply?oid={oid}&pn=1&ps=20&root={id}&type={type}\n" +
                          $"获取你的评论，疑似评论区被戒严或者这是你的视频。" +
                          $"\n\n 你的评论：{message}");
            }
        }
    }

    private static async Task CheckChildReply(string oid, int type, string id, string message, string root)
    {
        var api = new CommentApi();
        // 无cookie检查
        for (int i = 1;; i++)
        {
            var response = await api.Reply(oid, root, 1, type, login: false).Request();

            if (!response.status)
            {
                break;
            }

            var dataCommentModel = JsonConvert.DeserializeObject<DataCommentModel>(response.results);

            if (dataCommentModel.Code != 0)
            {
                NotificationShowExtensions.ShowMessageToast($"发评反诈：{dataCommentModel.Code}:{dataCommentModel.Message}");
                break;
            }

            if (dataCommentModel.Data.Replies?.Any() != true)
            {
                break;
            }

            var index = dataCommentModel.Data.Replies.FindIndex(r => r.RpId.ToString() == id);
            if (index != -1)
            {
                NotificationShowExtensions.ShowCommAntifraudDialog($"无账号状态下找到了你的评论，评论正常！\n\n你的评论：{message}");
                return;
            }
        }

        // 带cookie检查
        for (int i = 1;; i++)
        {
            var response = await api.Reply(oid, root, 1, type).Request();

            if (!response.status)
            {
                break;
            }

            var dataCommentModel = JsonConvert.DeserializeObject<DataCommentModel>(response.results);

            if (dataCommentModel.Code != 0)
            {
                NotificationShowExtensions.ShowMessageToast($"发评反诈：{dataCommentModel.Code}:{dataCommentModel.Message}");
                break;
            }

            if (dataCommentModel.Data.Replies?.Any() != true)
            {
                break;
            }

            var index = dataCommentModel.Data.Replies.FindIndex(r => r.RpId.ToString() == id);
            if (index != -1)
            {
                NotificationShowExtensions.ShowCommAntifraudDialog($"你的评论被shadow ban（仅自己可见）！\n\n你的评论: {message}");
                return;
            }
        }

        NotificationShowExtensions.ShowCommAntifraudDialog($"评论不可见: {message}");
    }
}