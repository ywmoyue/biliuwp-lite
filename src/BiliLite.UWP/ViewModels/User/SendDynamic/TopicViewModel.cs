using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Models.Requests.Api.User;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.User.SendDynamic
{
    public class TopicViewModel : BaseViewModel
    {
        readonly DynamicAPI dynamicAPI;

        public TopicViewModel()
        {
            dynamicAPI = new DynamicAPI();
            Items = new ObservableCollection<RcmdTopicModel>();
        }

        public ObservableCollection<RcmdTopicModel> Items { get; set; }

        public bool Loading { get; set; } = true;

        public async Task GetTopic()
        {
            try
            {
                Loading = true;
                var api = dynamicAPI.RecommendTopic();

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Items = JsonConvert.DeserializeObject<ObservableCollection<RcmdTopicModel>>(data.data["rcmds"].ToString());

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
                var handel = HandelError<TopicViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
