using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.User
{
    public class UserAttentionButtonViewModel : BaseViewModel
    {
        private readonly VideoAPI m_videoApi;

        public UserAttentionButtonViewModel()
        {
            m_videoApi = new VideoAPI();
            AttentionCommand = new RelayCommand(DoAttentionUP);
        }

        public event EventHandler AttentionDone;

        public event EventHandler CancelAttention;

        public ICommand AttentionCommand { get; private set; }

        public int Attention { get; set; }

        [DoNotNotify]
        public string UserId { get; set; }

        public async void DoAttentionUP()
        {
            var result = await AttentionUP(UserId, Attention == 1 ? 2 : 1);
            if (!result) return;
            if (Attention == 1)
            {
                Attention = 0;
                CancelAttention?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Attention = 1;
                AttentionDone?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<bool> AttentionUP(string mid, int mode)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return false;
            }

            try
            {
                var results = await m_videoApi.Attention(mid, mode.ToString()).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();

                    if (data.success)
                    {

                        NotificationShowExtensions.ShowMessageToast("操作成功");
                        return true;
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(data.message);
                        return false;
                    }
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(results.message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                return false;
            }
        }
    }
}
