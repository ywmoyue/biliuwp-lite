using BiliLite.Models.Attributes;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common;
using BiliLite.ViewModels.User.SendDynamic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Requests.Api.Live;

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
            Notify.ShowMessageToast("本地关注人数超出限制");
            return;
        }

        if (localAttentionUsers.Any(x => x.Id == user.Id))
        {
            Notify.ShowMessageToast("已经关注了");
            return;
        }
        localAttentionUsers.Add(user);
        SettingService.SetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, localAttentionUsers);
        Notify.ShowMessageToast("已关注");
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

        Notify.ShowMessageToast("已取消关注");
    }

    public async Task<List<LiveRoomInfoOldModel>> GetLiveRooms()
    {

        var localAttentionUsers = SettingService.GetValue<List<LocalAttentionUser>>(SettingConstants.UI.LOCAL_ATTENTION_USER, null);
        if (localAttentionUsers == null || !localAttentionUsers.Any())
        {
            return null;
        }

        var roomList = new List<LiveRoomInfoOldModel>();

        foreach (var user in localAttentionUsers)
        {
            var api = new LiveRoomAPI().GetRoomInfoOld(user.Id);
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

            data.data.UserName = user.Name;
            data.data.UserId = user.Id;

            roomList.Add(data.data);
        }

        return roomList;
    }
}