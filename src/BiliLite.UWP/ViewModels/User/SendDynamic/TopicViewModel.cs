﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Models.Requests.Api.User;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<TopicViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
