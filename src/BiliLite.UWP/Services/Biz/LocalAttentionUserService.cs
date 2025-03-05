using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Common.User;
using BiliLite.Models.Requests.Api.Live;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Models.Exceptions;

namespace BiliLite.Services.Biz;

[RegisterTransientService]
public class LocalAttentionUserService : BaseBizService
{
    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

    public void AttentionUp(string id, string name)
    {
        var user = new LocalAttentionUser()
        {
            Id = id,
            Name = name,
        };
        var localAttentionUsers = SettingService.GetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, null);
        if (localAttentionUsers == null)
        {
            localAttentionUsers = new List<LocalAttentionUser>();
        }

        if (localAttentionUsers.Count > 5)
        {
            NotificationShowExtensions.ShowMessageToast("本地关注人数超出限制");
            return;
        }

        if (localAttentionUsers.Any(x => x.Id == user.Id))
        {
            NotificationShowExtensions.ShowMessageToast("已经关注了");
            return;
        }
        localAttentionUsers.Add(user);
        SettingService.SetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, localAttentionUsers);
        NotificationShowExtensions.ShowMessageToast("已关注");
    }

    public void CancelAttention(string id)
    {
        var localAttentionUsers = SettingService.GetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, null);
        if (localAttentionUsers == null)
        {
            return;
        }

        var user = localAttentionUsers.FirstOrDefault(x => x.Id == id);
        if (user == null)
        {
            return;
        }

        localAttentionUsers.Remove(user);
        SettingService.SetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, localAttentionUsers);

        NotificationShowExtensions.ShowMessageToast("已取消关注");
    }

    public async Task<List<LiveInfoModel>> GetLiveRooms()
    {

        var localAttentionUsers = SettingService.GetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, null);
        if (localAttentionUsers == null || !localAttentionUsers.Any())
        {
            return null;
        }

        var roomList = new List<LiveInfoModel>();

        var liveRoomApi = new LiveRoomAPI();

        foreach (var user in localAttentionUsers)
        {
            var api = liveRoomApi.GetRoomInfoOld(user.Id);
            var results = await api.Request();
            if (!results.status)
            {
                _logger.Warn(results.message);
                continue;
            }

            var data = await results.GetData<LiveRoomInfoOldModel>();
            if (!data.success)
            {
                _logger.Warn(data.message);
                continue;
            }

            if (data.data.RoomStatus == 0) continue;

            var result = await liveRoomApi.LiveRoomInfo(data.data.RoomId.ToString()).Request();
            if (!result.status)
            {
                throw new CustomizedErrorException(result.message);
            }

            var realRoomData = await result.GetData<LiveInfoModel>();
            if (!realRoomData.success)
            {
                _logger.Warn(data.message);
                continue;
            }

            roomList.Add(realRoomData.data);
        }

        return roomList;
    }
}