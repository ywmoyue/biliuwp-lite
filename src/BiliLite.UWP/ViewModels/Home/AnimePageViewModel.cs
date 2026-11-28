using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Pages.Bangumi;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace BiliLite.ViewModels.Home
{
    public class AnimePageViewModel : BaseViewModel
    {
        #region Fields

        private readonly FollowAPI m_followApi;
        private readonly AnimeBizService m_animeService;
        private readonly IMapper m_mapper;
        private AnimeType m_animeType;

        #endregion

        #region Constructors

        public AnimePageViewModel()
        {
            m_mapper = App.ServiceProvider.GetService<IMapper>();
            m_animeService = App.ServiceProvider.GetRequiredService<AnimeBizService>();
            m_followApi = new FollowAPI();
        }

        #endregion

        #region Properties

        public ObservableCollection<AnimeFallViewModel> AnimeFalls { get; set; }

        [DoNotNotify]
        public List<PageEntranceModel> Entrances { get; set; }

        public bool ShowFollows { get; set; }

        public bool Loading { get; set; } = true;

        public bool LoadingFollow { get; set; } = true;

        public ObservableCollection<FollowSeasonModel> Follows { get; set; }

        public AnimeHomeModel HomeData { get; set; }

        /// <summary>
        /// 推荐流是否还有下一页
        /// </summary>
        public bool CanLoadMoreFeed { get; set; }

        public bool LoadingMoreFeed { get; set; }

        #endregion

        #region Private Methods

        private void SeasonItemOpen(object sender, object seasonId, string title, bool dontGoTo = false)
        {
            if (seasonId == null) return;
            MessageCenter.NavigateToPage(sender, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(Pages.SeasonDetailPage),
                parameters = seasonId,
                title = title.ToString(),
                dontGoTo = dontGoTo,
            });
        }

        #endregion

        #region Public Methods

        public void SetAnimeType(AnimeType animeType)
        {
            m_animeType = animeType;

            Entrances = new List<PageEntranceModel>() {
                new PageEntranceModel(){
                    Logo=Constants.Images.RANK_ICON_IMAGE,
                    Name="热门榜单",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.FourBars,
                            page=typeof(SeasonRankPage),
                            title="热门榜单",
                            parameters=(int)animeType
                    }
                },
                new PageEntranceModel(){
                    Logo=Constants.Images.INDEX_ICON_IMAGE,
                    Name="索引",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(AnimeIndexPage),
                            title="索引",
                            parameters=new SeasonIndexParameter()
                            {
                                Type=  IndexSeasonType.Anime,
                                Area=m_animeType== AnimeType.Bangumi?"-1":"1,6,7"
                            }
                    }
                },
                  new PageEntranceModel(){
                    Logo=Constants.Images.TIMELINE_ICON_IMAGE,
                    Name="时间表",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.Filter,
                            page=typeof(TimelinePage),
                            title="时间表",
                            parameters=m_animeType
                    }
                },
                  new PageEntranceModel(){
                    Logo=Constants.Images.MY_ICON_IMAGE,
                    Name="我的追番",
                    NavigationInfo=new NavigationInfo(){
                            icon= Symbol.OutlineStar,
                            page=typeof(FavoritePage),
                            title="我的追番",
                            parameters=OpenFavoriteType.Bangumi
                    }
                },
            };
        }

        public void SeasonItemClick(object sender, ItemClickEventArgs e)
        {
            var seasonId = e.ClickedItem.GetType().GetProperty(nameof(ISeasonItem.SeasonId)).GetValue(e.ClickedItem, null);
            var title = e.ClickedItem.GetType().GetProperty(nameof(ISeasonItem.Title))?.GetValue(e.ClickedItem, null) ?? "";
            SeasonItemOpen(sender, seasonId, title.ToString());
        }

        public void SeasonItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var seasonId = element.DataContext.GetType()?.GetProperty(nameof(ISeasonItem.SeasonId))?.GetValue(element.DataContext, null);
            var title = element.DataContext.GetType()?.GetProperty(nameof(ISeasonItem.Title))?.GetValue(element.DataContext, null) ?? "";
            SeasonItemOpen(sender, seasonId, title?.ToString(), true);
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

        public async Task GetBangumiHome()
        {
            try
            {
                Loading = true;
                HomeData = await m_animeService.GetHome(GetSeasonType());
                AnimeFalls = new ObservableCollection<AnimeFallViewModel>(
                    m_mapper.Map<List<AnimeFallViewModel>>(HomeData.Falls));
                CanLoadMoreFeed = HomeData.HasMoreFeed;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
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
                var results = await m_followApi.MyFollowBangumi().Request();
                if (!results.status)
                {
                    throw new Exception(results.message);
                }

                var data = await results.GetJson<ApiResultModel<JObject>>();
                if (!data.success)
                {
                    throw new Exception(data.message);
                }

                Follows = await data.result["follow_list"].ToString()
                    .DeserializeJson<ObservableCollection<FollowSeasonModel>>();
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeHomeModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingFollow = false;
            }
        }

        /// <summary>
        /// 首页推荐流加载更多（追加下一个题材榜 + 下一批猜你喜欢卡片）
        /// </summary>
        public async Task LoadMoreFeed()
        {
            try
            {
                if (LoadingMoreFeed || HomeData == null || !HomeData.HasMoreFeed)
                {
                    return;
                }

                LoadingMoreFeed = true;

                var feed = await m_animeService.GetFeedMore(GetSeasonType(), HomeData.FeedCursor);
                if (feed.Sections.Count == 0 && !feed.HasMore)
                {
                    HomeData.HasMoreFeed = false;
                    CanLoadMoreFeed = false;
                    return;
                }

                if (AnimeFalls == null)
                {
                    return;
                }

                // 只追加全新的分组（题材榜）；已存在的同名分组（猜你喜欢）不再增长——
                // 往已有 GridView 里塞条目会让整行重排、重渲染，视觉上就是"闪一下"
                foreach (var section in feed.Sections)
                {
                    if (AnimeFalls.Any(x => x.Title == section.Title))
                    {
                        continue;
                    }

                    AnimeFalls.Add(m_mapper.Map<AnimeFallViewModel>(section));
                }

                HomeData.FeedCursor = feed.NextCursor;
                HomeData.HasMoreFeed = feed.HasMore;
                CanLoadMoreFeed = feed.HasMore;
            }
            catch (Exception ex)
            {
                var handel = HandelError<List<AnimeFallItemModel>>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                LoadingMoreFeed = false;
            }
        }

        private int GetSeasonType()
        {
            return m_animeType == AnimeType.GuoChuang ? (int)AnimeType.GuoChuang : (int)AnimeType.Bangumi;
        }

        #endregion
    }
}