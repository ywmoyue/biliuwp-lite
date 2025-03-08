using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliLite.ViewModels
{
    public class EmoteViewModel : BaseViewModel
    {
        private readonly EmoteApi m_emoteApi;

        public EmoteViewModel()
        {
            m_emoteApi = new EmoteApi();
        }

        public List<EmotePackageModel> Packages { get; set; }

        public bool Loading { get; set; } = true;

        public async Task GetEmote(EmoteBusiness business)
        {
            try
            {
                Loading = true;
                var api = m_emoteApi.UserEmote(business);

                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Packages = JsonConvert.DeserializeObject<List<EmotePackageModel>>(data.data["packages"].ToString());
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
                var handel = HandelError<EmoteViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }

    public class EmotePackageModel
    {
        public int id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public int type { get; set; }

        public int attr { get; set; }
        public List<EmotePackageItemModel> emote { get; set; }
    }

    public class EmotePackageItemModel
    {
        public int id { get; set; }
        public string text { get; set; }
        public string url { get; set; }
        public int package_id { get; set; }
        public int type { get; set; }
        public bool showImage { get { return type != 4; } }
    }
}
