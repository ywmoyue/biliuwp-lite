using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Pages.Bangumi;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace BiliLite.ViewModels.Home
{
    [RegisterTransientViewModel]
    public class CinemaViewModel : BaseViewModel
    {
        #region Fields

        private readonly FollowAPI m_followApi;
        private readonly CinemaAPI m_cinemaApi;
        private readonly IMapper m_mapper;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public CinemaViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            m_cinemaApi = new CinemaAPI();
            m_followApi = new FollowAPI();
            Entrances = new List<PageEntranceModel>() {
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/榜单.png",
                    Name="热门榜单",
                      NavigationInfo=new NavigationInfo(){
                            icon= Symbol.FourBars,
                            page=typeof(SeasonRankPage),
                            title="热门榜单",
                            parameters=2
                    }
                },
                new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/电影.png",
                    Name="电影索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="电影索引",
                            parameters=new SeasonIndexParameter()
                            {
                                Type= IndexSeasonType.Movie
                            }
                    }
                },
                 new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/电视剧.png",
                    Name="电视剧索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="电视剧索引",
                            parameters=new SeasonIndexParameter()
                            {
                                Type= IndexSeasonType.TV
                            }
                    }
                },
                 new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/纪录片.png",
                    Name="纪录片索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="纪录片索引",
                            parameters=new SeasonIndexParameter()
                            {
                                Type= IndexSeasonType.Documentary
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/综艺.png",
                    Name="综艺索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="综艺索引",
                            parameters=new SeasonIndexParameter()
                            {
                                Type= IndexSeasonType.Variety
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo="ms-appx:///Assets/Icon/我的.png",
                    Name="我的追剧",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.OutlineStar,
                            page=typeof(FavoritePage),
                            title="我的追剧",
                            parameters=OpenFavoriteType.Cinema
                    }
                },
            };
        }

        #endregion

        #region Properties

        public bool ShowFollows { get; set; }

        public bool Loading { get; set; } = true;

        public bool LoadingFollow { get; set; } = true;

        public ObservableCollection<FollowSeasonModel> Follows { get; set; }

        public CinemaHomeViewModel HomeData { get; set; }

        public List<PageEntranceModel> Entrances { get; set; }

        #endregion

        #region Public Methods

        public async void SeasonItemClick(object sender, ItemClickEventArgs e)
        {
            var seasonId = e.ClickedItem.GetType().GetProperty(nameof(ISeasonItem.SeasonId)).GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty(nameof(ISeasonItem.SeasonId))?.GetValue(e.ClickedItem, null) ?? "";
            if (seasonId != null && seasonId.ToInt32() != 0)
            {
                MessageCenter.NavigateToPage(sender, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(Pages.SeasonDetailPage),
                    parameters = seasonId,
                    title = title.ToString()
                });
            }
            else
            {
                var weblink = e.ClickedItem.GetType().GetProperty("link").GetValue(e.ClickedItem, null) ?? "";
                var result = await MessageCenter.HandelUrl(weblink.ToString());
                if (!result) NotificationShowExtensions.ShowMessageToast("无法打开此链接");
            }
        }

        public void LinkItemClick(object sender, ItemClickEventArgs e)
        {
            var weblink = e.ClickedItem.GetType().GetProperty("link").GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty("title").GetValue(e.ClickedItem, null) ?? "";
            MessageCenter.NavigateToPage(sender, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(Pages.WebPage),
                parameters = weblink,
                title = title.ToString()
            });
        }

        public async Task GetCinemaHome()
        {
            try
            {
                Loading = true;
                var api = m_cinemaApi.CinemaHome();

                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<CinemaHomeModel>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                HomeData = m_mapper.Map<CinemaHomeViewModel>(data.data);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<CinemaViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task GetFollows()
        {
            try
            {
                LoadingFollow = true;
                var results = await m_followApi.MyFollowCinema().Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiResultModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                Follows = await data.result["follow_list"].ToString()
                    .DeserializeJson<ObservableCollection<FollowSeasonModel>>();
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<CinemaViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingFollow = false;
            }
        }

        public async Task GetFallMore(CinemaHomeFallViewModel animeFallViewModel)
        {
            try
            {
                animeFallViewModel.ShowMore = false;
                var results = await m_cinemaApi.CinemaFallMore(animeFallViewModel.Wid, animeFallViewModel.Items.LastOrDefault().Cursor).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<List<CinemaHomeFallItemModel>>();
                foreach (var item in data)
                {
                    animeFallViewModel.Items.Add(item);
                }
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<List<CinemaHomeFallItemModel>>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                animeFallViewModel.ShowMore = true;
            }
        }

        #endregion
    }
}
