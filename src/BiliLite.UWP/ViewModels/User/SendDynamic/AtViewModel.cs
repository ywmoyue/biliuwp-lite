using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Modules;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.User.SendDynamic
{
    public class AtViewModel : BaseViewModel
    {
        readonly AtApi m_atApi;

        public AtViewModel()
        {
            m_atApi = new AtApi();
            SearchCommand = new RelayCommand<string>(Search);
            LoadMoreCommand = new RelayCommand(LoadMore);
            Users = new ObservableCollection<AtUserModel>();
        }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        [DoNotNotify]
        public ICommand SearchCommand { get; private set; }

        public ObservableCollection<AtUserModel> Users { get; set; }

        public bool Loading { get; set; } = true;

        [DoNotNotify]
        public int Page { get; set; } = 1;

        [DoNotNotify]
        public string Keyword { get; set; }

        public async Task GetUser()
        {
            try
            {
                Loading = true;
                var api = m_atApi.RecommendAt(Page);
                if (!string.IsNullOrEmpty(Keyword))
                {
                    api = m_atApi.SearchUser(Keyword, Page);
                }
                if (Page == 1)
                {
                    Users.Clear();
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (!string.IsNullOrEmpty(Keyword))
                        {
                            if (data.data.ContainsKey("items"))
                            {
                                foreach (var item in data.data["items"])
                                {
                                    Users.Add(new AtUserModel()
                                    {
                                        Face = item["face"].ToString(),
                                        UserName = item["name"].ToString(),
                                        ID = item["mid"].ToInt64(),
                                    });
                                }
                            }

                        }
                        else
                        {
                            foreach (var item in data.data["recent_attention"]["info"])
                            {
                                Users.Add(new AtUserModel()
                                {
                                    Face = item["face"].ToString(),
                                    UserName = item["uname"].ToString(),
                                    ID = item["uid"].ToInt64(),
                                });
                            }
                        }
                        Page++;
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
                var handel = HandelError<AtViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }

            await GetUser();
        }
        public async void Search(string keyword)
        {

            if (Loading)
            {
                return;
            }
            Keyword = keyword;
            Page = 1;
            await GetUser();
        }
    }
}