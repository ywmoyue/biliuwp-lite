using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Models.Responses;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace BiliLite.Modules
{
    public class VideoDetailPageViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        readonly FavoriteApi favoriteAPI;
        readonly VideoAPI videoAPI;
        readonly PlayerAPI PlayerAPI;
        readonly FollowAPI followAPI;
        readonly SponsorBlockApi sponsorBlockAPI;
        private readonly IMapper m_mapper;
        private readonly ThemeService m_themeService;

        #endregion

        #region Constructors

        public VideoDetailPageViewModel()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            m_themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
            videoAPI = new VideoAPI();
            favoriteAPI = new FavoriteApi();
            PlayerAPI = new PlayerAPI();
            followAPI = new FollowAPI();
            sponsorBlockAPI = new SponsorBlockApi();
            RefreshCommand = new RelayCommand(Refresh);
            LikeCommand = new RelayCommand(DoLike);
            DislikeCommand = new RelayCommand(DoDislike);
            LaunchUrlCommand = new RelayCommand<object>(LaunchUrl);
            CoinCommand = new RelayCommand<string>(DoCoin);
            SetStaffHeightCommand = new RelayCommand<string>(SetStaffHeight);
            OpenRightInfoCommand = new RelayCommand(OpenRightInfo);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }

        public ICommand LikeCommand { get; private set; }

        public ICommand DislikeCommand { get; private set; }

        public ICommand CoinCommand { get; private set; }

        public ICommand LaunchUrlCommand { get; private set; }

        public ICommand SetStaffHeightCommand { get; private set; }

        public ICommand OpenRightInfoCommand { get; private set; }

        public bool Loading { get; set; } = true;

        public bool Loaded { get; set; }

        public bool ShowError { get; set; }

        public string ErrorMsg { get; set; } = "";

        public VideoDetailViewModel VideoInfo { get; set; }

        public double StaffHeight { get; set; } = 88.0;

        public bool ShowMoreStaff { get; set; }

        public ObservableCollection<FavoriteItemViewModel> MyFavorite { get; set; }

        [DoNotNotify]
        public List<string> ExistFavIdList { get; set; }

        public double BottomActionBarHeight { get; set; }

        public double BottomActionBarWidth { get; set; }

        public double VideoDetailListEpisodeDesiredWidth => SettingService.GetValue(
            SettingConstants.UI.VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH,
            SettingConstants.UI.DEFAULT_VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowNormalDownloadBtn => !(BottomActionBarWidth < 460);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowFlyoutDownloadBtn => (BottomActionBarWidth < 460);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowNormalShareBtn => !(BottomActionBarWidth < 390);

        [DependsOn(nameof(BottomActionBarWidth))]
        public bool ShowFlyoutShareBtn => (BottomActionBarWidth < 390);

        public double PageHeight { get; set; }

        public double PageWidth { get; set; }

        [DependsOn(nameof(PageWidth))]
        public int PlayerGridColumnSpan => PageWidth < 1000 ? 2 : 1;

        public GridLength DefaultRightInfoWidth { get; set; } = new GridLength(320);

        public bool IsOpenRightInfo { get; set; }

        [DependsOn(nameof(PageWidth))]
        public bool ShowOpenRightInfoBtn => (PageWidth < 1000);

        [DependsOn(nameof(PageWidth), nameof(IsOpenRightInfo))]
        public GridLength RightInfoWidth
        {
            get
            {
                if (PageWidth < 1000 && !IsOpenRightInfo)
                {
                    return new GridLength(0);
                }

                return DefaultRightInfoWidth;
            }
        }

        [DependsOn(nameof(PageHeight), nameof(PageWidth))]
        public Brush RightInfoBackground
        {
            get
            {
                if (PageWidth < 1000)
                {
                    return (Brush)m_themeService.DefaultThemeResource["PlayerControlAcrylicBrush"];
                }

                return new SolidColorBrush(Colors.Transparent);
            }
        }

        [DependsOn(nameof(PageHeight), nameof(PageWidth))]
        public double RightInfoHeight
        {
            get
            {
                if (PageWidth < 1000)
                {
                    return PageHeight - BottomActionBarHeight;
                }

                return PageHeight;
            }
        }

        public List<BiliVideoTag> Tags { get; set; }

        public List<PlayerSkipItem> SponsorBlockList { get; set; } = [];

        public bool ShowSponsorBlock => 
            SettingService.GetValue(SettingConstants.Player.SPONSOR_BLOCK, SettingConstants.Player.DEFAULT_SPONSOR_BLOCK);

        #endregion

        #region Private Methods

        private async void LaunchUrl(object paramenter)
        {
            await MessageCenter.HandelUrl(paramenter.ToString());
        }

        private void SetStaffHeight(string height)
        {
            var h = Convert.ToDouble(height);
            ShowMoreStaff = h > StaffHeight;
            StaffHeight = h;
        }

        private void OpenRightInfo()
        {
            IsOpenRightInfo = !IsOpenRightInfo;
        }

        private async Task LoadVideoTags(string avid)
        {
            var api = videoAPI.Tags(avid);
            var results = await api.Request();
            if (!results.status)
            {
                _logger.Warn(results.message);
            }

            var data = await results.GetJson<ApiDataModel<List<BiliVideoTag>>>();
            if (!data.success)
            {
                _logger.Warn(data.message);
            }

            Tags = data.data;
        }

        #endregion

        #region Public Methods

        public async Task LoadFavorite(string avid)
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    return;
                }
                var results = await favoriteAPI.MyCreatedFavorite(avid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var myFavorite = await data.data["list"].ToString().DeserializeJson<List<FavoriteItemModel>>();
                        MyFavorite = m_mapper.Map<ObservableCollection<FavoriteItemViewModel>>(myFavorite);
                        ExistFavIdList = myFavorite.Where(x => x.FavState == 1).Select(x => x.Id).ToList();
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
                var handel = HandelError<VideoDetailPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async Task LoadSponsorBlock(string bvid)
        {
            SponsorBlockList.Clear();

            var results = await sponsorBlockAPI.GetSponsorBlock(bvid).Request();
            if (!results.status)
            {
                if(results.code is 400 or 404) NotificationShowExtensions.ShowMessageToast($"视频{bvid} SponsorBlock API请求错误: {results.code}");
                _logger.Warn(results.message);
                return;
            }
            
            var data = await results.GetJson<List<SponsorBlockVideo>>();
            if (data is null)
            {
                _logger.Warn("SponsorBlock转换错误");
                return;
            }

            var video = data.FirstOrDefault(video => video.VideoId == bvid);
            if (video is null) return;
            foreach (var seg in video.Segments)
            {
                var item = new PlayerSkipItem
                {
                    Start = seg.Segment[0] != 0 ? seg.Segment[0] : seg.Segment[0] + 0.75, //完全贴合视频开头的片段会报错. 加偏移量
                    End = Math.Abs(seg.Segment[1] - seg.VideoDuration) > 0.5 ? seg.Segment[1] : seg.Segment[1] - 1.5, //完全贴合视频结尾的片段会卡死播放器，加偏移量
                    Category = seg.Category,
                    VideoDuration = seg.VideoDuration,
                    Cid = seg.Cid,
                };
                if (!item.IsSectionValid || 
                    item.CategoryEnum == SponsorBlockType.PoiHighlight || 
                    item.CategoryEnum == SponsorBlockType.None) continue; //暂不支持精彩时刻
                SponsorBlockList.Add(item);
            }
        }

        public async Task LoadVideoDetail(string id, bool isbvid = false)
        {
            try
            {
                if (id.Length == 0) { throw new ArgumentException(nameof(id)); }
                Loaded = false;
                Loading = true;
                ShowError = false;
                var needGetUserReq = false;
                // 正常app获取视频详情
                var results = await videoAPI.Detail(id, isbvid).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }

                var data = await results.GetJson<ApiDataModel<VideoDetailModel>>();

                // 通过web获取, 作为后备使用
                if (!data.success)
                {
                    // 通过web获取视频详情
                    var webResult = await videoAPI.DetailWebInterface(id, isbvid).Request();
                    // 通过web获取推荐视频
                    var webRelatesResult = await videoAPI.RelatesWebInterface(id, isbvid).Request();
                    if (webResult.status && webRelatesResult.status)
                    {
                        data = await webResult.GetJson<ApiDataModel<VideoDetailModel>>();
                        data.data.ShortLink = "https://b23.tv/" + data.data.Bvid;

                        // 解析推荐视频
                        var relatesData = await webRelatesResult.GetJson<ApiDataModel<List<VideoDetailRelatesModel>>>();
                        if (!relatesData.success)
                        {
                            throw new CustomizedErrorException(relatesData.message);
                        }
                        data.data.Relates = relatesData.data;

                        needGetUserReq = true;
                    }
                }

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                var webResults = await videoAPI.DetailWebInterface(id, isbvid).Request();
                if (!webResults.status)
                {
                    throw new CustomizedErrorException(webResults.message);
                }
                var webData = await webResults.GetJson<ApiDataModel<VideoDetailModel>>();
                if (!webData.success)
                {
                    throw new CustomizedErrorException(webData.message);
                }
                if (data.data.UgcSeason == null && webData.data.UgcSeason != null)
                {
                    data.data.UgcSeason = webData.data.UgcSeason;
                }

                var videoInfoViewModel = m_mapper.Map<VideoDetailViewModel>(data.data);
                VideoInfo = videoInfoViewModel;
                if (needGetUserReq)
                {
                    await GetAttentionUp();
                }
                Loaded = true;

                await LoadFavorite(data.data.Aid);

                await LoadVideoTags(data.data.Aid);

                if (ShowSponsorBlock) await LoadSponsorBlock(data.data.Bvid);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException customizedErrorException)
                {
                    ShowError = true;
                    ErrorMsg = ex.Message;
                    _logger.Error("视频详情获取失败", ex);
                    return;
                }

                var handel = HandelError<VideoDetailPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                ShowError = true;
                ErrorMsg = handel.message;
            }
            finally
            {
                Loading = false;
            }
        }

        public void Refresh()
        {

        }

        public async void DoLike()
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Like(VideoInfo.Aid, VideoInfo.ReqUser.Dislike, VideoInfo.ReqUser.Like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Like == 1)
                        {
                            VideoInfo.ReqUser.Like = 0;
                            VideoInfo.Stat.Like -= 1;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Like = 1;
                            VideoInfo.ReqUser.Dislike = 0;
                            VideoInfo.Stat.Like += 1;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            NotificationShowExtensions.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            NotificationShowExtensions.ShowMessageToast("操作成功");
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
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }


        }
        public async void DoDislike()
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Dislike(VideoInfo.Aid, VideoInfo.ReqUser.Dislike, VideoInfo.ReqUser.Like).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Dislike == 1)
                        {
                            VideoInfo.ReqUser.Dislike = 0;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Dislike = 1;
                            if (VideoInfo.ReqUser.Like == 1)
                            {
                                VideoInfo.ReqUser.Like = 0;
                                VideoInfo.Stat.Like -= 1;
                            }


                        }
                        if (data.data == null) { }
                        else if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            NotificationShowExtensions.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            NotificationShowExtensions.ShowMessageToast("操作成功");
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
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }



        }
        public async void DoTriple()
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await videoAPI.Triple(VideoInfo.Aid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        VideoInfo.ReqUser.Like = 1;
                        VideoInfo.Stat.Like += 1;
                        VideoInfo.ReqUser.Coin = 1;
                        VideoInfo.Stat.Coin += 1;
                        VideoInfo.ReqUser.Favorite = 1;
                        VideoInfo.Stat.Favorite += 1;
                        if (VideoInfo.ReqUser.Dislike == 1)
                        {
                            VideoInfo.ReqUser.Dislike = 0;
                        }

                        NotificationShowExtensions.ShowMessageToast("三连完成");
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
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }



        }

        public async void DoCoin(string num)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            var coinNum = Convert.ToInt32(num);
            try
            {
                var results = await videoAPI.Coin(VideoInfo.Aid, coinNum).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        if (VideoInfo.ReqUser.Coin == 1)
                        {
                            VideoInfo.ReqUser.Coin = 0;
                            VideoInfo.Stat.Coin -= coinNum;
                        }
                        else
                        {
                            VideoInfo.ReqUser.Coin = 1;
                            VideoInfo.Stat.Coin += coinNum;
                        }
                        if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                        {
                            NotificationShowExtensions.ShowMessageToast(data.data["toast"].ToString());
                        }
                        else
                        {
                            NotificationShowExtensions.ShowMessageToast("操作成功");
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
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async Task GetAttentionUp()
        {
            VideoInfo.ReqUser ??= new VideoDetailReqUserViewModel();
            VideoInfo.ReqUser.Attention = -999;
            if (!SettingService.Account.Logined)
            {
                return;
            }

            var result = await followAPI.GetAttention(VideoInfo.Owner.Mid).Request();
            if (!result.status) return;
            var data = await result.GetJson<ApiDataModel<UserAttentionResponse>>();
            if (data.data.Attribute == 2 || data.data.Attribute == 6)
            {
                VideoInfo.ReqUser.Attention = 1;
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
                var results = await videoAPI.Attention(mid, mode.ToString()).Request();
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

        public async Task UpdateFav(string avid, bool favDefault = false)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }

            try
            {
                var newIdList = new List<string>();

                var defaultUseFav = SettingService.GetValue(SettingConstants.UI.DEFAULT_USE_FAV,
                    SettingConstants.UI.DEFAULT_USE_FAV_VALUE);
                if (!favDefault)
                    newIdList = MyFavorite.Where(x => x.IsFav).Select(x => x.Id).ToList();
                else
                    newIdList.Add(MyFavorite.First(x => x.Title == defaultUseFav).Id);

                var delIdList = ExistFavIdList.Except(newIdList).ToList();
                var addIdList = newIdList.Except(ExistFavIdList).ToList();
                var results = await favoriteAPI.UpdateFavorite(addIdList, delIdList, avid).Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success)
                    throw new CustomizedErrorException(data.message);
                if (newIdList.Count != 0 && VideoInfo.ReqUser.Favorite == 0)
                {
                    VideoInfo.ReqUser.Favorite = 1;
                    VideoInfo.Stat.Favorite += 1;
                }
                else if (VideoInfo.ReqUser.Favorite == 1 && newIdList.Count == 0)
                {
                    VideoInfo.ReqUser.Favorite = 0;
                    VideoInfo.Stat.Favorite -= 1;
                }

                if (!string.IsNullOrEmpty(data.data["toast"]?.ToString()))
                    throw new CustomizedErrorException(data.data["toast"].ToString());
                NotificationShowExtensions.ShowMessageToast("操作成功");
                //立即刷新可能导致取到旧数据
                await Task.Delay(500);
                await LoadFavorite(avid);
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async Task<string> GetPlayUrl()
        {
            try
            {
                var results = await PlayerAPI.VideoPlayUrl(VideoInfo.Aid, VideoInfo.Pages[0].Cid, 80, false).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        return data.data["durl"][0]["url"].ToString();
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(data.message);
                        return "";
                    }
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(results.message);
                    return "";
                }
            }
            catch (Exception ex)
            {

                var handel = HandelError<string>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                return "";
            }
        }

        #endregion
    }
}
