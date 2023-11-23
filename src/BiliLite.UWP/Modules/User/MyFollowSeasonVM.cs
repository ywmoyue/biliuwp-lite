using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.Anime;
using BiliLite.Services;
using Newtonsoft.Json;

namespace BiliLite.Modules
{
    public class MyFollowSeasonVM : IModules
    {

        readonly FollowAPI followAPI;
        public MyFollowSeasonVM(bool _isAnime)
        {
            followAPI = new FollowAPI();
            isAnime = _isAnime;
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            StatusCommand = new RelayCommand<object>(ChangeStatus);
            CancelFollowCommand = new RelayCommand<object>(CancelFollow);
            SetWantWatchCommand = new RelayCommand<object>(SetWantWatch);
            SetWatchedCommand = new RelayCommand<object>(SetWatched);
            SetWatchingCommand = new RelayCommand<object>(SetWatching);
        }
        private bool isAnime = true;
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand StatusCommand { get; private set; }
        public ICommand CancelFollowCommand { get; set; }
        public ICommand SetWantWatchCommand { get; set; }
        public ICommand SetWatchedCommand { get; set; }
        public ICommand SetWatchingCommand { get; set; }


        private bool _loading = false;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        private ObservableCollection<FollowSeasonModel> _follows;
        public ObservableCollection<FollowSeasonModel> Follows
        {
            get { return _follows; }
            set { _follows = value; DoPropertyChanged("Follows"); }
        }

        private int _Status = 2;
        public int Status
        {
            get { return _Status; }
            set
            {
                _Status = value;
                DoPropertyChanged("Status");
            }
        }

        public int Page { get; set; } = 1;

        private bool _Nothing = false;
        public bool Nothing
        {
            get { return _Nothing; }
            set { _Nothing = value; DoPropertyChanged("Nothing"); }
        }

        private bool _ShowLoadMore = false;
        public bool ShowLoadMore
        {
            get { return _ShowLoadMore; }
            set { _ShowLoadMore = value; DoPropertyChanged("ShowLoadMore"); }
        }

        public async Task LoadFollows()
        {
            try
            {
                Loading = true;
                Nothing = false;
                ShowLoadMore = false;
                var api = followAPI.MyFollowBangumi(page: Page, status: Status);
                if (!isAnime)
                {
                    api = followAPI.MyFollowCinema(page: Page, status: Status);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        if (data.result["has_next"].ToInt32() == 1)
                        {
                            ShowLoadMore = true;
                        }
                        if (!data.result.ContainsKey("follow_list"))
                        {
                            if (Page == 1)
                            {
                                Nothing = true;
                                Follows = new ObservableCollection<FollowSeasonModel>();
                            }
                            else
                            {
                                Notify.ShowMessageToast("全部加载完了...");
                            }
                            return;
                        }
                        var ls = await data.result["follow_list"].ToString().DeserializeJson<ObservableCollection<FollowSeasonModel>>();
                        if (ls != null && ls.Count != 0)
                        {
                            foreach (var item in ls)
                            {
                                item.Status = Status;
                                item.CancelFollowCommand = CancelFollowCommand;
                                item.SetWantWatchCommand = SetWantWatchCommand;
                                item.SetWatchedCommand = SetWatchedCommand;
                                item.SetWatchingCommand = SetWatchingCommand;
                            }
                            if (Page == 1)
                            {
                                Follows = ls;
                            }
                            else
                            {
                                foreach (var item in ls)
                                {
                                    Follows.Add(item);
                                }
                            }
                            Page++;

                        }
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
                var handel = HandelError<MyFollowSeasonVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            Follows = null;
            await LoadFollows();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Follows == null || Follows.Count == 0)
            {
                return;
            }
            await LoadFollows();
        }

        public async void ChangeStatus(object value)
        {
            var result = Convert.ToInt32(value);
            if (result == Status)
            {
                return;
            }
            Status = result;
            Page = 1;
            await LoadFollows();
        }
        public async void CancelFollow(object par)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            var item = par as FollowSeasonModel;
            try
            {
                var api = followAPI.CancelFollowSeason(item.SeasonId.ToString());
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Follows.Remove(item);
                        if (Follows.Count == 0)
                        {
                            Nothing = true;
                        }
                        if (!string.IsNullOrEmpty(data.result["toast"]?.ToString()))
                        {
                            Notify.ShowMessageToast(data.result["toast"].ToString());
                        }
                        else
                        {
                            Notify.ShowMessageToast("操作成功");
                        }
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }


        }

        private async void SetWantWatch(object par)
        {
            var item = par as FollowSeasonModel;
            await SetSeasonStatus(item, 1);
        }
        private async void SetWatched(object par)
        {
            var item = par as FollowSeasonModel;
            await SetSeasonStatus(item, 3);
        }
        private async void SetWatching(object par)
        {
            var item = par as FollowSeasonModel;
            await SetSeasonStatus(item, 2);
        }
        private async Task SetSeasonStatus(FollowSeasonModel item, int status)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = followAPI.SetSeasonStatus(item.SeasonId.ToString(), status);
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiResultModel<JObject>>();
                    if (data.success)
                    {
                        Follows.Remove(item);
                        if (Follows.Count == 0)
                        {
                            Nothing = true;
                        }
                        Notify.ShowMessageToast("操作成功");
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
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

    }

    public class FollowSeasonModel : ISeasonItem
    {
        public ICommand CancelFollowCommand { get; set; }

        public ICommand SetWantWatchCommand { get; set; }

        public ICommand SetWatchedCommand { get; set; }

        public ICommand SetWatchingCommand { get; set; }

        public int Status { get; set; }

        public bool ShowWatched => Status != 3;

        public bool ShowWatching => Status != 2;

        public bool ShowWantWatch => Status != 1;

        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Badge { get; set; }

        [JsonProperty("square_cover")]
        public string SquareCover { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        public string Url { get; set; }

        public string ProgressText => Progress != null ? Progress.index_show : "尚未观看";

        [JsonProperty("new_ep")]
        public FollowSeasonNewEpModel NewEp { get; set; }

        public FollowSeasonProgressModel Progress { get; set; }
    }
    public class FollowSeasonNewEpModel
    {

        public string cover { get; set; }
        public int duration { get; set; }
        public int id { get; set; }
        public string index_show { get; set; }
    }
    public class FollowSeasonProgressModel
    {
        public int last_ep_id { get; set; }
        public string index_show { get; set; }
    }

}
