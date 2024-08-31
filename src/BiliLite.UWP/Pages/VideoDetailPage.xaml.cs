using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Modules;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using BiliLite.Controls;
using Windows.System;
using BiliLite.Dialogs;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using Windows.UI.Xaml.Controls.Primitives;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Download;
using BiliLite.ViewModels.Video;
using BiliLite.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.User;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    public sealed partial class VideoDetailPage : PlayPage, IRefreshablePage, ISavablePage
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private readonly VideoDetailPageViewModel m_viewModel;
        string avid = "";
        string bvid = "";
        bool is_bvid = false;
        private bool isFirstUgcSeasonVideo = false;
        private VideoListView m_videoListView;
        private bool m_loadUgcSeasonData = false;

        public VideoDetailPage()
        {
            this.InitializeComponent();
            Title = "视频详情";
            this.Loaded += VideoDetailPage_Loaded;
            this.Player = this.player;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            m_viewModel = new VideoDetailPageViewModel();
            this.DataContext = m_viewModel;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
            this.RightInfoGridSplitter.IsEnabled = SettingService.GetValue<bool>(SettingConstants.UI.RIGHT_WIDTH_CHANGEABLE, false);
            Unloaded += VideoDetailPage_Unloaded;
        }

        private void VideoDetailPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Bindings.StopTracking();
        }

        private void VideoDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(this.Parent is MyFrame frame)) return;
            frame.ClosedPage -= VideoDetailPage_ClosedPage;
            frame.ClosedPage += VideoDetailPage_ClosedPage;
        }

        private void VideoDetailPage_ClosedPage(object sender, EventArgs e)
        {
            ClosePage();
        }
        private void ClosePage()
        {
            if (m_viewModel != null)
            {
                m_viewModel.Loaded = false;
                m_viewModel.Loading = true;
                m_viewModel.VideoInfo = null;
            }
            changedFlag = true;
            player?.FullScreen(false);
            player?.MiniWidnows(false);
            player?.Dispose();
            if (!(this.Parent is MyFrame frame)) return;
            frame.ClosedPage -= VideoDetailPage_ClosedPage;
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = m_viewModel.VideoInfo.Title;
            request.Data.SetWebLink(new Uri(m_viewModel.VideoInfo.ShortLink));
        }
        bool flag = false;
        string _id = "";
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                if (SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_SCREEN, false))
                {
                    player.IsFullScreen = true;
                }
                else
                {
                    player.IsFullWindow = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_WINDOW, false);
                }
                pivot.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DETAIL_DISPLAY, 0);
                if (e.Parameter is VideoPlaylist videoPlaylist)
                {
                    var videoSections = new List<VideoListSection>();
                    videoSections.Add(new VideoListSection()
                    {
                        Selected = true,
                        Title = videoPlaylist.Title,
                        Items = new List<VideoListItem>(),
                        IsLazyOnlineList = videoPlaylist.IsOnlineMediaList,
                        OnlineListId = videoPlaylist.MediaListId,
                    });
                    foreach (var videoPlaylistItem in videoPlaylist.Playlist)
                    {
                        videoSections.First().Items.Add(new VideoListItem()
                        {
                            Id = videoPlaylistItem.Id,
                            Title = videoPlaylistItem.Title,
                            Author = videoPlaylistItem.Author,
                            Cover = videoPlaylistItem.Cover,
                        });
                    }

                    videoSections.First().SelectedItem = videoSections.First().Items.ElementAt(videoPlaylist.Index);
                    m_videoListView = App.ServiceProvider.GetRequiredService<VideoListView>();
                    m_videoListView.LoadData(videoSections);
                    var pivotItem = PlayListTpl.GetElement(new Windows.UI.Xaml.ElementFactoryGetArgs()) as PivotItem;
                    pivotItem.Content = m_videoListView;
                    m_videoListView.OnSelectionChanged += VideoListView_SelectionChanged;

                    pivot.Items.Insert(0, pivotItem);
                    pivot.SelectedIndex = 0;
                    await InitializeVideo(videoSections.First().SelectedItem.Id);
                }
                else
                {
                    var id = e.Parameter.ToString();
                    await InitializeVideo(id);
                }

            }
            else
            {
                Title = m_viewModel?.VideoInfo?.Title ?? "视频详情";
                MessageCenter.ChangeTitle(this, Title);
            }

        }
        private async Task InitializeVideo(string id)
        {
            _id = id;
            if (flag) return;
            flag = true;
            if (long.TryParse(id, out var aid))
            {
                avid = id;
                is_bvid = false;
            }
            else
            {
                bvid = id;
                is_bvid = true;
            }
            await m_viewModel.LoadVideoDetail(id, is_bvid);
            if (this.VideoCover != null)
            {
                this.VideoCover.Visibility = SettingService.GetValue<bool>(SettingConstants.UI.SHOW_DETAIL_COVER, true) ? Visibility.Visible : Visibility.Collapsed;
            }
            if (SettingService.GetValue<bool>("一键三连提示", true))
            {
                SettingService.SetValue("一键三连提示", false);
                Notify.ShowMessageToast("右键或长按点赞按钮可以一键三连哦~", 5);
            }
            if (m_viewModel.VideoInfo == null)
            {
                flag = false;
                return;
            }

            avid = m_viewModel.VideoInfo.Aid;
            var desc = m_viewModel.VideoInfo.Desc.ToRichTextBlock(null);

            contentDesc.Content = desc;
            ChangeTitle(m_viewModel.VideoInfo.Title);
            await CreateQR();
            if (!string.IsNullOrEmpty(m_viewModel.VideoInfo.RedirectUrl))
            {
                var result = await MessageCenter.HandelSeasonID(m_viewModel.VideoInfo.RedirectUrl);
                if (!string.IsNullOrEmpty(result))
                {
                    this.Frame.Navigate(typeof(SeasonDetailPage), result);
                    //从栈中移除当前页面的历史
                    this.Frame.BackStack.Remove(this.Frame.BackStack.FirstOrDefault(x => x.SourcePageType == this.GetType()));
                    return;
                }
            }
            InitPlayInfo();

            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = m_viewModel.VideoInfo.Aid
            });

            if (!m_viewModel.VideoInfo.ShowUgcSeason)
            {
                flag = false;
                return;
            }

            InitUgcSeason(id);

            flag = false;
        }

        private void InitPlayInfo()
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in m_viewModel.VideoInfo.Pages)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = m_viewModel.VideoInfo.Aid,
                    cid = item.Cid,
                    duration = item.Duration,
                    is_interaction = m_viewModel.VideoInfo.Interaction != null,
                    order = i,
                    play_mode = VideoPlayType.Video,
                    title = "P" + item.Page + " " + item.Part,
                    TitlePage = "P"+item.Page,
                    TitlePart = item.Part.TrimStart(' '),
                    area = m_viewModel.VideoInfo.Title.ParseArea(m_viewModel.VideoInfo.Owner.Mid)
                });
                i++;
            }
            var index = 0;
            if (m_viewModel.VideoInfo.History != null)
            {
                var history = m_viewModel.VideoInfo.Pages.FirstOrDefault(x => x.Cid.Equals(m_viewModel.VideoInfo.History.Cid));
                if (history != null)
                {
                    SettingService.SetValue<double>(history.Cid, Convert.ToDouble(m_viewModel.VideoInfo.History.Progress));
                    index = m_viewModel.VideoInfo.Pages.IndexOf(history);
                    //player.InitializePlayInfo(playInfos, );
                }
            }
            player.InitializePlayInfo(playInfos, index);
        }

        private void InitUgcSeason(string id)
        {
            m_loadUgcSeasonData = true;
            VideoListSection currentSeasonSection = null;
            VideoListItem currentSeasonItem = null;

            var videoSections = new List<VideoListSection>();
            foreach (var section in m_viewModel.VideoInfo.UgcSeason.Sections)
            {
                var videoSection = new VideoListSection()
                {
                    Id = section.Id,
                    Title = section.Title,
                    Items = new List<VideoListItem>(),
                };
                foreach (var item in section.Episodes)
                {
                    var videoItem = new VideoListItem()
                    {
                        Title = item.Title,
                        Author = item.AuthorDesc,
                        Cover = item.Cover,
                        Id = item.Aid,
                    };
                    if (item.Aid == m_viewModel.VideoInfo.Aid)
                    {
                        currentSeasonSection = videoSection;
                        currentSeasonItem = videoItem;
                    }
                    videoSection.Items.Add(videoItem);
                }
                videoSections.Add(videoSection);
            }

            if (m_videoListView == null)
            {
                currentSeasonSection.Selected = true;
                currentSeasonSection.SelectedItem = currentSeasonItem;
                m_videoListView = App.ServiceProvider.GetRequiredService<VideoListView>();
                m_videoListView.LoadData(videoSections);
                m_videoListView.OnSelectionChanged += VideoListView_SelectionChanged;
                var pivotItem = PlayListTpl.GetElement(new Windows.UI.Xaml.ElementFactoryGetArgs()) as PivotItem;
                pivotItem.Content = m_videoListView;
                pivot.Items.Insert(0, pivotItem);
            }
            else
            {
                m_videoListView.LoadData(videoSections);
            }
        }

        private async Task CreateQR()
        {
            try
            {
                var qrCodeService = App.ServiceProvider.GetRequiredService<IQrCodeService>();
                var img = await qrCodeService.GenerateQrCode(m_viewModel.VideoInfo.ShortLink);
                imgQR.Source = img;
            }
            catch (Exception ex)
            {
                logger.Log("创建二维码失败avid" + avid, LogType.Error, ex);
                Notify.ShowMessageToast("创建二维码失败");
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType.Name == "BlankPage")
            {
                ClosePage();
            }

            base.OnNavigatingFrom(e);
        }
        public void ChangeTitle(string title)
        {
            if ((this.Parent as Frame)?.Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }

        private void txtDesc_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stDesc.MaxHeight > 80)
            {
                stDesc.MaxHeight = 80;
            }
            else
            {
                stDesc.MaxHeight = 1000.0;
            }
            Notify.ShowMessageToast("右键或长按可以复制内容");
        }

        private void txtDesc_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if ((sender as TextBlock).Text.SetClipboard())
            {
                Notify.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Notify.ShowMessageToast("复制失败");
            }
        }

        private void txtDesc_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if ((sender as TextBlock).Text.SetClipboard())
            {
                Notify.ShowMessageToast("已将内容复制到剪切板");
            }
            else
            {
                Notify.ShowMessageToast("复制失败");
            }
        }

        private void listRelates_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as VideoDetailRelatesViewModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                parameters = data.Aid,
                title = data.Title
            });

            //this.Frame.Navigate(typeof(VideoDetailPage), data.aid);
        }

        private void AppBarButton_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Notify.ShowMessageToast("长按");
        }

        private void btnLike_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo.ReqUser.Like == 0)
            {
                m_viewModel.DoTriple();
            }
        }

        private void btnTagItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as BiliVideoTag;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                parameters = item.TagName,
                title = "搜索:" + item.TagName
            });
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            $"{m_viewModel.VideoInfo.Title}\r\n{m_viewModel.VideoInfo.ShortLink}".SetClipboard();
            Notify.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.ShortLink.SetClipboard();
            Notify.ShowMessageToast("已复制链接到剪切板");
        }


        private void PlayerControl_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                this.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0) == 0 ? -48 : -48, 0, 0);
                m_viewModel.DefaultRightInfoWidth = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                this.Margin = new Thickness(0);
                m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }

        private void PlayerControl_FullWindowEvent(object sender, bool e)
        {
            if (e)
            {
                m_viewModel.DefaultRightInfoWidth = new GridLength(0, GridUnitType.Pixel);
                BottomInfo.Height = new GridLength(0, GridUnitType.Pixel);
            }
            else
            {
                m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
                BottomInfo.Height = GridLength.Auto;
            }
        }
        bool changedFlag = false;
        private void PlayerControl_ChangeEpisodeEvent(object sender, int e)
        {
            changedFlag = true;
            listEpisode.SelectedIndex = e;
            changedFlag = false;
        }

        private void listEpisode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listEpisode.SelectedIndex == -1)
            {
                return;
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);
        }

        private void btnOpenUser_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext;
            if (data is VideoDetailStaffViewModel)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = (data as VideoDetailStaffViewModel).Name,
                    page = typeof(UserInfoPage),
                    parameters = (data as VideoDetailStaffViewModel).Mid
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = m_viewModel.VideoInfo.Owner.Name,
                    page = typeof(UserInfoPage),
                    parameters = m_viewModel.VideoInfo.Owner.Mid
                });
            }
        }

        private void btnLike_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo.ReqUser.Like == 0)
            {
                m_viewModel.DoTriple();
            }
        }

        private async void btnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(m_viewModel.VideoInfo.ShortLink));
        }

        private void ImageEx_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo?.Pic == null) return;
            MessageCenter.OpenImageViewer(new List<string>() {
                m_viewModel.VideoInfo.Pic
            }, 0);
        }

        private async void btnCreateFavBox_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            CreateFavFolderDialog createFavFolderDialog = new CreateFavFolderDialog();
            await createFavFolderDialog.ShowAsync();
            await m_viewModel.LoadFavorite(m_viewModel.VideoInfo.Aid);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as VideoDetailRelatesViewModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Aid);
        }

        private void BtnWatchLater_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel == null || m_viewModel.VideoInfo == null) return;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(avid);
        }

        private async void VideoListView_SelectionChanged(object sender,VideoListItem item)
        {
            await InitializeVideo(item.Id);
        }

        private void player_AllMediaEndEvent(object sender, EventArgs e)
        {
            if (m_videoListView == null || m_videoListView.IsLast(m_viewModel.VideoInfo.Aid)) return;

            // 切换到播放列表Tab使播放列表控件被渲染事件能触发
            pivot.SelectedIndex = 0;

            m_videoListView.Next(m_viewModel.VideoInfo.Aid);
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.VideoInfo == null || m_viewModel.VideoInfo.Pages == null || m_viewModel.VideoInfo.Pages.Count == 0) return;
            var downloadItem = new DownloadItem()
            {
                Cover = m_viewModel.VideoInfo.Pic,
                ID = m_viewModel.VideoInfo.Aid,
                Episodes = new List<DownloadEpisodeItem>(),
                Subtitle = m_viewModel.VideoInfo.Bvid,
                Title = m_viewModel.VideoInfo.Title,
                Type = DownloadType.Video,
                UpMid = m_viewModel.VideoInfo.Owner.Mid.ToInt64(),
            };
            int i = 0;
            foreach (var item in m_viewModel.VideoInfo.Pages)
            {
                //检查正在下载及下载完成是否存在此视频
                int state = 0;
                var downloadViewModel = App.ServiceProvider.GetRequiredService<DownloadPageViewModel>();
                if (downloadViewModel.Downloadings.FirstOrDefault(x => x.EpisodeID == item.Cid) != null)
                {
                    state = 2;
                }
                if (downloadViewModel.DownloadedViewModels.FirstOrDefault(x => x.Epsidoes.FirstOrDefault(y => y.CID == item.Cid) != null) != null)
                {
                    state = 3;
                }
                //如果正在下载state=2,下载完成state=3
                downloadItem.Episodes.Add(new DownloadEpisodeItem()
                {
                    AVID = m_viewModel.VideoInfo.Aid,
                    BVID = m_viewModel.VideoInfo.Bvid,
                    CID = item.Cid,
                    EpisodeID = "",
                    Index = i,
                    Title = "P" + item.Page + " " + item.Part,
                    State = state
                });
                i++;
            }


            if (m_viewModel.VideoInfo.ShowUgcSeason)
            {
                foreach (var ugcSeasonSection in m_viewModel.VideoInfo.UgcSeason.Sections)
                {
                    foreach (var episode in ugcSeasonSection.Episodes)
                    {
                        //检查正在下载及下载完成是否存在此视频
                        int state = 0;
                        var downloadViewModel = App.ServiceProvider.GetRequiredService<DownloadPageViewModel>();
                        if (downloadViewModel.Downloadings.FirstOrDefault(x => x.EpisodeID == episode.Cid) != null)
                        {
                            state = 2;
                        }
                        if (downloadViewModel.DownloadedViewModels.FirstOrDefault(x => x.Epsidoes.FirstOrDefault(y => y.CID == episode.Cid) != null) != null)
                        {
                            state = 3;
                        }
                        //如果正在下载state=2,下载完成state=3
                        downloadItem.Episodes.Add(new DownloadEpisodeItem()
                        {
                            AVID = episode.Aid,
                            BVID = episode.Bvid,
                            CID = episode.Cid,
                            EpisodeID = "",
                            Index = i,
                            Title = episode.Title,
                            State = state
                        });
                        i++;
                    }
                }
            }

            DownloadDialog downloadDialog = new DownloadDialog(downloadItem);
            await downloadDialog.ShowAsync();
        }

        public async Task Refresh()
        {
            if (m_viewModel.Loading) return;
            await InitializeVideo(_id);
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private void btnOpenQR_Click(object sender, RoutedEventArgs e)
        {
            qrFlyout.ShowAt(btnMore);
        }

        private void TitleText_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var option = new FlyoutShowOptions();
            var element = sender as UIElement;
            option.Position = e.GetPosition(element);
            TitleRightTappedMenu.ShowAt(element, option);
        }

        private void CopyTitleBtn_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.Title.SetClipboard();
        }

        private void CopyAuthorBtn_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.VideoInfo.Owner.Name.SetClipboard();
        }

        private void listEpisode_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void listEpisode_PreviewKeyUp(object sender, KeyRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void BottomActionBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                m_viewModel.BottomActionBarWidth = e.NewSize.Width;
            }
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                m_viewModel.BottomActionBarHeight = e.NewSize.Height;
            }
        }

        private void VideoDetailPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                m_viewModel.PageWidth = e.NewSize.Width;
            }
            if (e.NewSize.Height != e.PreviousSize.Height)
            {
                m_viewModel.PageHeight = e.NewSize.Height;
            }
        }

        private async void SaveFavList_OnClick(object sender, RoutedEventArgs e)
        {
            await Save();
        }

        public async Task Save()
        {
            if (BtnFav.Flyout.IsOpen)
            {
                await m_viewModel.UpdateFav(m_viewModel.VideoInfo.Aid);
                BtnFav.Flyout.Hide();
            }
        }

        private async void FavList_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.IsUseMiddleButton(sender))
            {
                await Save();
            }
        }

        private async void BtnFav_OnClick(object sender, RoutedEventArgs e)
        {
            if (SettingService.GetValue(SettingConstants.UI.QUICK_DO_FAV, SettingConstants.UI.DEFAULT_QUICK_DO_FAV) &&
                m_viewModel.VideoInfo.ReqUser.Favorite != 1)
            {
                await m_viewModel.UpdateFav(m_viewModel.VideoInfo.Aid, true);
            }
        }

        private void Pivot_OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == VirtualKey.Right || e.Key == VirtualKey.Left) &&
                e.OriginalSource.GetType() != typeof(TextBox))
                e.Handled = true;
        }
    }
}
