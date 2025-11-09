using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Live;
using BiliLite.Modules.Live.LiveCenter;
using BiliLite.Pages.Live;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Home;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LivePage : Page, IRefreshablePage
    {
        private LiveViewModel m_viewModel;
        public LivePage()
        {
            this.InitializeComponent();
            m_viewModel = App.ServiceProvider.GetRequiredService<LiveViewModel>();
            if (SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true))
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }

        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.Banners == null)
            {
                await LoadData();
            }
        }
        private async Task LoadData()
        {
            await m_viewModel.GetLiveHome();
            if (SettingService.Account.Logined)
            {
                m_viewModel.ShowFollows = true;
                await m_viewModel.LiveAttentionVm.GetFollows();
                await m_viewModel.LiveAttentionVm.GetLocalFollows();
                if (m_viewModel.LiveAttentionVm.LocalFollows != null && m_viewModel.LiveAttentionVm.LocalFollows.Any())
                {
                    m_viewModel.ShowLocalFollows = true;
                }
            }
        }

        public async Task Refresh()
        {
            await LoadData();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private async void BannerItem_Click(object sender, RoutedEventArgs e)
        {
            var result = await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as LiveHomeBannerModel).Link);
            if (!result)
            {
                NotificationShowExtensions.ShowMessageToast("不支持打开的链接");
            }
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            await LoadData();
        }

        private void FollowLive_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenItem(e.ClickedItem);
        }

        private void OpenItem(object liveItem, bool dontGoTo = false)
        {
            if (liveItem is LiveFollowAnchorModel data)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Video,
                    page = typeof(LiveDetailPage),
                    title = data.uname + "的直播间",
                    parameters = data.roomid,
                    dontGoTo = dontGoTo,
                });
            }
            else if (liveItem is LiveInfoModel info)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Video,
                    page = typeof(LiveDetailPage),
                    title = info.AnchorInfo.BaseInfo.Uname + "的直播间",
                    parameters = info.RoomInfo.RoomId,
                    dontGoTo = dontGoTo,
                });
            }
            else if (liveItem is LiveHomeAreaModel area)
            {
                if (area.Id == 0)
                {
                    MessageCenter.NavigateToPage(this, new NavigationInfo()
                    {
                        icon = Symbol.Document,
                        page = typeof(LiveAreaPage),
                        title = area.Title,
                        dontGoTo = dontGoTo,
                    });
                    return;
                }

                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(LiveAreaDetailPage),
                    title = area.Title,
                    parameters = new LiveAreaPar()
                    {
                        parent_id = area.AreaV2ParentId,
                        area_id = area.AreaV2Id
                    },
                    dontGoTo = dontGoTo,
                });
            }
            else if (liveItem is LiveHomeItemsItemModel homeItem)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Video,
                    page = typeof(LiveDetailPage),
                    title = homeItem.Uname + "的直播间",
                    parameters = homeItem.Roomid,
                    dontGoTo = dontGoTo,
                });
            }
            else if (liveItem is LiveHomeItemsModel home)
            {
                if (home.ModuleInfo.Title == "推荐直播")
                {
                    MessageCenter.NavigateToPage(this, new NavigationInfo()
                    {
                        icon = Symbol.Document,
                        page = typeof(LiveRecommendPage),
                        title = "全部直播",
                        dontGoTo = dontGoTo,
                    });
                }

                if (!string.IsNullOrEmpty(home.ModuleInfo.Link))
                {
                    try
                    {
                        var match = Regex.Match(home.ModuleInfo.Link, @"parentAreaId=(\d+)&areaId=(\d+)");
                        if (match.Groups.Count == 3)
                        {
                            MessageCenter.NavigateToPage(this, new NavigationInfo()
                            {
                                icon = Symbol.Document,
                                page = typeof(LiveAreaDetailPage),
                                title = home.ModuleInfo.Title,
                                parameters = new LiveAreaPar()
                                {
                                    parent_id = match.Groups[1].Value.ToInt32(),
                                    area_id = match.Groups[2].Value.ToInt32()
                                },
                                dontGoTo = dontGoTo,
                            });

                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private void LiveItems_ItemClick(object sender, ItemClickEventArgs e)
        {
            OpenItem(e.ClickedItem);
        }

        private void loadMore_Click(object sender, RoutedEventArgs e)
        {
            OpenItem((sender as HyperlinkButton).DataContext);
        }

        private void Area_ItemClick(object sender, ItemClickEventArgs e)
        {
            var area = e.ClickedItem as LiveHomeAreaModel;
            OpenItem(area);
        }

        private async void btnOpenLiveCenter_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(Live.LiveCenterPage),
                title = "直播中心",
            });
        }

        private void CancelLocalAttention_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement { DataContext: LiveInfoModel roomInfo })
            {
                return;
            }

            var localAttentionUserService = App.ServiceProvider.GetRequiredService<LocalAttentionUserService>();
            localAttentionUserService.CancelAttention(roomInfo.RoomInfo.Uid + "");
            m_viewModel.LiveAttentionVm.LocalFollows.Remove(roomInfo);
        }

        private void UIElement_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;

            OpenItem(element.DataContext, true);
        }
    }
}
