using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Services;
using BiliLite.Services.Biz;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliLite.Modules.Live.LiveCenter
{
    public class LiveAttentionVM : IModules
    {
        readonly LiveCenterAPI liveCenterAPI;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        public LiveAttentionVM()
        {
            liveCenterAPI = new LiveCenterAPI();
            RefreshCommand = new RelayCommand(Refresh);
        }

        public ObservableCollection<LiveInfoModel> LocalFollows { get; set; }

        private ObservableCollection<LiveFollowAnchorModel> _Follow;

        public ObservableCollection<LiveFollowAnchorModel> Follow
        {
            get { return _Follow; }
            set { _Follow = value; DoPropertyChanged("Follow"); }
        }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }
        public ICommand RefreshCommand { get; private set; }
        public async Task GetFollows()
        {
            try
            {
                Loading = true;
                var results = await liveCenterAPI.FollowLive().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (data.data["rooms"] != null)
                        {
                            Follow = await data.data["rooms"].ToString().DeserializeJson<ObservableCollection<LiveFollowAnchorModel>>();
                        }
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
                var handel = HandelError<LiveAttentionVM>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task GetLocalFollows()
        {
            var localAttentionUserService = App.ServiceProvider.GetRequiredService<LocalAttentionUserService>();
            var roomList = await localAttentionUserService.GetLiveRooms();
            if (roomList == null) return;
            LocalFollows = new ObservableCollection<LiveInfoModel>(roomList);
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Follow = null;
            await GetFollows();
            await GetLocalFollows();
        }
    }
    public class LiveFollowAnchorModel
    {
        public int roomid { get; set; }
        public string uid { get; set; }
        public string uname { get; set; }
        public string face { get; set; }
        public string title { get; set; }
        public string live_tag_name { get; set; }
        public int online { get; set; }
        public string area_name { get; set; }
        public string area_v2_name { get; set; }
        public string area_v2_parent_name { get; set; }
        public long live_time { get; set; }
        public string cover { get; set; }
        public string pendent_ru { get; set; }
        public string pendent_ru_color { get; set; }
        public string pendent_ru_pic { get; set; }
        public BitmapImage pendent_pic
        {
            get
            {
                if (string.IsNullOrEmpty(pendent_ru_pic))
                {
                    return new BitmapImage();
                }
                else
                {
                    return new BitmapImage(new Uri(pendent_ru_pic));
                }
            }
        }
        public bool show_pendent
        {
            get
            {
                return !string.IsNullOrEmpty(pendent_ru);
            }
        }
    }
}
