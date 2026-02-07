using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.User.WatchLater;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.Services.Biz;

[RegisterTransientService]
public class WatchLaterService : BaseBizService
{
    private WatchLaterAPI m_watchLaterAPI;

    public WatchLaterService()
    {
        m_watchLaterAPI = new WatchLaterAPI();
    }

    public async Task<bool> AddToWatchlater(string aid)
    {
        try
        {
            if (!SettingService.Account.Logined && await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return false;
            }
            var results = await m_watchLaterAPI.Add(aid).Request();
            if (results.status)
            {
                var data = await results.GetJson<ApiDataModel<object>>();
                if (data.success)
                {
                    NotificationShowExtensions.ShowMessageToast("已添加到稍后再看");
                    return true;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast(results.message);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }

        return false;
    }

    public async Task<List<WatchlaterItemModel>> GetWatchLaterItems()
    {
        try
        {
            var results = await m_watchLaterAPI.Watchlater().Request();
            if (results.status)
            {
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (data.success)
                {
                    var ls = JsonConvert.DeserializeObject<List<WatchlaterItemModel>>(data.data["list"].ToString());
                    return ls;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast(results.message);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }

        return null;
    }

    public async Task<bool> Clear()
    {
        try
        {

            if (!await NotificationShowExtensions.ShowMessageDialog("清空稍后再看", "确定要清空全部视频吗?")) return false;
            var results = await m_watchLaterAPI.Clear().Request();
            if (results.status)
            {
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (data.success)
                {
                    return true;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast(results.message);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }

        return false;
    }

    public async Task<bool> ClearViewed()
    {
        try
        {
            if (!await NotificationShowExtensions.ShowMessageDialog("清除已观看", "确定要清空已观看视频吗?")) return false;
            var results = await m_watchLaterAPI.Del().Request();
            if (results.status)
            {
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (data.success)
                {
                    return true;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast(results.message);
            }
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }

        return false;
    }

    public async Task<bool> Remove(string aid)
    {
        try
        {
            var results = await m_watchLaterAPI.Del(aid).Request();
            if (results.status)
            {
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (data.success)
                {
                    return true;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast(results.message);
            }
        }
        catch (Exception ex)
        {
           HandleError(ex);
        }
        return false;
    }

    public List<WatchlaterItemModel> FindFinishedVideos(List<WatchlaterItemModel> videos)
    {
        if (videos == null || videos.Count == 0)
        {
            return new List<WatchlaterItemModel>();
        }

        // 找到观看进度大于5且观看进度与视频长度相差1的视频或进度为-1（已观看）的视频
        return videos.Where(v =>
            v.progress > 5 &&
            Math.Abs(v.progress - v.duration) < 1
        || v.progress == -1).ToList();
    }

    public async Task<int> RemoveFinishedVideos(List<WatchlaterItemModel> finishedVideos)
    {
        if (finishedVideos == null || finishedVideos.Count == 0)
        {
            return 0;
        }

        int successCount = 0;
        foreach (var video in finishedVideos)
        {
            if (await Remove(video.aid))
            {
                successCount++;
                await Task.Delay(500);
            }
        }

        return successCount;
    }
}
