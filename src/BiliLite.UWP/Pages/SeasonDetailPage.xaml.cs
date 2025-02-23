using BiliLite.Controls;
using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Download;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Season;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SeasonDetailPage : PlayPage
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private readonly SeasonDetailPageViewModel m_viewModel;
        private readonly SeasonReviewViewModel m_seasonReviewViewModel;
        string season_id = "";
        string ep_id = "";
        bool selectProview = false;
        public SeasonDetailPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<SeasonDetailPageViewModel>();
            m_seasonReviewViewModel = App.ServiceProvider.GetRequiredService<SeasonReviewViewModel>();
            this.InitializeComponent();
            Title = "剧集详情";
            this.Loaded += SeasonDetailPage_Loaded;
            this.Player = this.player;
            NavigationCacheMode = NavigationCacheMode.Enabled;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            m_viewModel.DefaultRightInfoWidth = new GridLength(SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320), GridUnitType.Pixel);
            this.RightInfoGridSplitter.IsEnabled = SettingService.GetValue<bool>(SettingConstants.UI.RIGHT_WIDTH_CHANGEABLE, false);
        }

        private void SeasonDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is MyFrame)
            {
                (this.Parent as MyFrame).ClosedPage -= SeasonDetailPage_ClosedPage;
                (this.Parent as MyFrame).ClosedPage += SeasonDetailPage_ClosedPage;
            }
        }

        private void SeasonDetailPage_ClosedPage(object sender, EventArgs e)
        {
            ClosePage();
        }
        private void ClosePage()
        {
            if (m_viewModel != null)
            {
                m_viewModel.Loaded = false;
                m_viewModel.Loading = true;
                m_viewModel.Detail = null;
            }
            changedFlag = true;
            player?.FullScreen(false);
            player?.MiniWidnows(false);
            player?.Dispose();
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = m_viewModel.Detail.Title;
            request.Data.SetWebLink(new Uri("http://b23.tv/ss" + season_id));
        }

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
                if (e.Parameter is object[])
                {
                    var pars = e.Parameter as object[];
                    season_id = pars[0].ToString();
                    ep_id = pars[1].ToString();
                }
                else
                {
                    season_id = e.Parameter.ToString();
                }

                await InitSeasonDetail();
            }
            else
            {
                Title = m_viewModel?.Detail?.Title ?? "视频详情";
                MessageCenter.ChangeTitle(this, Title);
            }
        }

        private async Task InitSeasonDetail()
        {
            await m_viewModel.LoadSeasonDetail(season_id);

            if (m_viewModel.Detail != null)
            {
                ChangeTitle(m_viewModel.Detail.Title);


                m_seasonReviewViewModel.MediaID = m_viewModel.Detail.MediaId;

                InitializePlayInfo();
                await CreateQR();

            }
        }

        private void InitializePlayInfo()
        {
            if (m_viewModel.NothingPlay) return;
            selectProview = !m_viewModel.ShowEpisodes;

            var index = 0;
            if (string.IsNullOrEmpty(ep_id) && m_viewModel.Detail.UserStatus?.Progress != null)
            {
                ep_id = m_viewModel.Detail.UserStatus.Progress.LastEpId.ToString();
                SettingService.SetValue<double>("ep" + ep_id, Convert.ToDouble(m_viewModel.Detail.UserStatus.Progress.LastTime));
            }
            var selectItem = (selectProview ? m_viewModel.Previews : m_viewModel.Episodes).FirstOrDefault(x => x.Id.ToString() == ep_id);
            if (selectItem != null)
            {
                index = (selectProview ? m_viewModel.Previews : m_viewModel.Episodes).IndexOf(selectItem);
            }
            if (selectProview)
            {
                UpdatePlayInfoToPreview(index);
            }
            else
            {
                UpdatePlayInfoToEpisode(index);
            }
        }
        private void UpdatePlayInfoToEpisode(int index)
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in m_viewModel.Episodes)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = item.Aid,
                    cid = item.Cid,
                    duration = 0,
                    season_id = m_viewModel.Detail.SeasonId,
                    season_type = m_viewModel.Detail.Type,
                    ep_id = item.Id.ToString(),
                    is_vip = item.Status != 2,
                    is_interaction = false,
                    order = i,
                    play_mode = VideoPlayType.Season,
                    title = item.Title + " " + item.LongTitle,
                    area = m_viewModel.Detail.Title.ParseArea(m_viewModel.Detail.UpInfo?.Mid ?? 0),
                    EpisodeSkip = item.Skip,
                });
                i++;
            }
            player.InitializePlayInfo(playInfos, index);
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = playInfos[index].avid
            });
        }
        private void UpdatePlayInfoToPreview(int index)
        {
            List<PlayInfo> playInfos = new List<PlayInfo>();
            int i = 0;
            foreach (var item in m_viewModel.Previews)
            {
                playInfos.Add(new PlayInfo()
                {
                    avid = item.Aid,
                    cid = item.Cid,
                    duration = 0,
                    season_id = m_viewModel.Detail.SeasonId,
                    season_type = m_viewModel.Detail.Type,
                    ep_id = item.Id.ToString(),
                    is_vip = item.Status != 2,
                    is_interaction = false,
                    order = i,
                    play_mode = VideoPlayType.Season,
                    title = item.Title + " " + item.LongTitle,
                    area = m_viewModel.Detail.Title.ParseArea(m_viewModel.Detail.UpInfo?.Mid ?? 0)
                });
                i++;
            }
            player.InitializePlayInfo(playInfos, index);
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = playInfos[index].avid
            });
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
            Title = title;
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
        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }
        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            $"{m_viewModel.Detail.Title}\r\nhttp://b23.tv/ss{season_id}".SetClipboard();
            NotificationShowExtensions.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            $"http://b23.tv/ss{season_id}".SetClipboard();
            NotificationShowExtensions.ShowMessageToast("已复制链接到剪切板");
        }



        private void PlayerControl_FullScreenEvent(object sender, bool e)
        {
            if (e)
            {
                if (SettingService.GetValue(SettingConstants.UI.DISPLAY_MODE, 0) <= 0)
                {
                    var marginOffset = SettingService.GetValue(SettingConstants.UI.TAB_HEIGHT,
                        SettingConstants.UI.DEFAULT_TAB_HEIGHT);
                    this.Margin = new Thickness(0, marginOffset * -1, 0, 0);
                }

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

        private async void PlayerControl_ChangeEpisodeEvent(object sender, int e)
        {
            changedFlag = true;
            var aid = "";
            if (selectProview)
            {
                listPreview.SelectedIndex = e;
                ep_id = m_viewModel.Previews[e].Id.ToString();
                aid = m_viewModel.Previews[e].Aid;
            }
            else
            {
                listEpisode.SelectedIndex = e;
                ep_id = m_viewModel.Episodes[e].Id.ToString();
                aid = m_viewModel.Episodes[e].Aid;
            }

            await CreateQR();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = aid
            });
            changedFlag = false;
        }

        private async void listEpisode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listEpisode.SelectedIndex == -1)
            {
                return;
            }
            if (selectProview)
            {
                listPreview.SelectedIndex = -1;
                selectProview = false;
                UpdatePlayInfoToEpisode(listEpisode.SelectedIndex);
            }
            player.ChangePlayIndex(listEpisode.SelectedIndex);
            ep_id = m_viewModel.Episodes[listEpisode.SelectedIndex].Id.ToString();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = m_viewModel.Episodes[listEpisode.SelectedIndex].Aid
            });
            await CreateQR();
        }

        private async void listPreview_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (changedFlag || listPreview.SelectedIndex == -1)
            {
                return;
            }
            if (!selectProview)
            {
                listEpisode.SelectedIndex = -1;
                selectProview = true;
                UpdatePlayInfoToPreview(listPreview.SelectedIndex);
            }
            player.ChangePlayIndex(listPreview.SelectedIndex);
            ep_id = m_viewModel.Episodes[listPreview.SelectedIndex].Id.ToString();
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = (int)CommentApi.CommentType.Video,
                CommentSort = CommentApi.CommentSort.Hot,
                Oid = m_viewModel.Episodes[listPreview.SelectedIndex].Aid
            });
            await CreateQR();
        }

        private async Task CreateQR()
        {
            try
            {
                var qrCodeService = App.ServiceProvider.GetRequiredService<IQrCodeService>();
                var img = await qrCodeService.GenerateQrCode("https://b23.tv/ep" + ep_id);
                imgQR.Source = img;
            }
            catch (Exception ex)
            {
                logger.Log("创建二维码失败epid" + ep_id, LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("创建二维码失败");
            }

        }

        private void SeasonList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SeasonList.SelectedItem == null)
            {
                return;
            }
            var item = SeasonList.SelectedItem as SeasonDetailSeasonItemModel;
            if (item.SeasonId.ToString() != season_id)
            {

                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = item.SeasonId,
                    title = item.Title
                });
                //this.Frame.Navigate(typeof(SeasonDetailPage), item.season_id);
            }
        }

        private void btnOpenIndexWithArea_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as HyperlinkButton).DataContext as SeasonDetailAreaItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = data.Name,
                parameters = new SeasonIndexParameter()
                {
                    Type = (IndexSeasonType)m_viewModel.Detail.Type,
                    Area = data.Id
                }
            });
        }

        private void btnOpenIndexWithStyle_Click(object sender, RoutedEventArgs e)
        {

            var data = (sender as HyperlinkButton).DataContext as SeasonDetailStyleItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Filter,
                page = typeof(Bangumi.AnimeIndexPage),
                title = data.Name,
                parameters = new SeasonIndexParameter()
                {
                    Type = (IndexSeasonType)m_viewModel.Detail.Type,
                    Style = data.Id
                }
            });
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 3 && (m_seasonReviewViewModel.Items == null || m_seasonReviewViewModel.Items.Count == 0))
            {
                await m_seasonReviewViewModel.GetItems();
            }
        }

        private void btnReviewLike_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as SeasonShortReviewItemViewModel;
            m_seasonReviewViewModel.Like(item);
        }


        private void btnReviewDislike_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as HyperlinkButton).DataContext as SeasonShortReviewItemViewModel;
            m_seasonReviewViewModel.Dislike(item);
        }

        private async void btnSendReview_Click(object sender, RoutedEventArgs e)
        {
            if (m_seasonReviewViewModel == null || m_seasonReviewViewModel.MediaID == 0) return;
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }

            SendReviewDialog sendReviewDialog = new SendReviewDialog(m_seasonReviewViewModel.MediaID);
            await sendReviewDialog.ShowAsync();
        }

        private async void btnOpenWeb_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("http://b23.tv/ss" + season_id));
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (m_viewModel.Detail?.Cover == null) return;
            MessageCenter.OpenImageViewer(new List<string>() {
                m_viewModel.Detail.Cover
            }, 0);
        }

        private async void btnDownload_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.Detail == null || m_viewModel.Detail.Episodes == null || m_viewModel.Detail.Episodes.Count == 0) return;
            var downloadItem = new DownloadItem()
            {
                Cover = m_viewModel.Detail.Cover,
                SeasonID = m_viewModel.Detail.SeasonId,
                SeasonType = m_viewModel.Detail.Type,
                Episodes = new List<DownloadEpisodeItem>(),
                Subtitle = m_viewModel.Detail.Subtitle,
                Title = m_viewModel.Detail.Title,
                UpMid = m_viewModel.Detail.UpInfo?.Mid ?? 0,
                Type = DownloadType.Season
            };
            int i = 0;
            foreach (var item in m_viewModel.Detail.Episodes)
            {
                // 检查正在下载及下载完成是否存在此视频
                int state = 0;
                var downloadService = App.ServiceProvider.GetRequiredService<DownloadService>();
                state = downloadService.CheckExist(item.Id.ToString(), true);

                //如果正在下载state=2,下载完成state=3
                downloadItem.Episodes.Add(new DownloadEpisodeItem()
                {
                    CID = item.Cid,
                    EpisodeID = item.Id.ToString(),
                    Index = i,
                    Title = item.Title + " " + item.LongTitle,
                    State = state,
                    AVID = item.Aid,
                    BVID = item.Bvid,
                    ShowBadge = item.ShowBadge,
                    Badge = item.Badge,
                    IsPreview = item.IsPreview
                });
                i++;
            }

            DownloadDialog downloadDialog = new DownloadDialog(downloadItem);
            await downloadDialog.ShowAsync();
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.Loading) return;
            await InitSeasonDetail();
        }

        private void btnOpenQR_Click(object sender, RoutedEventArgs e)
        {
            qrFlyout.ShowAt(btnMore);
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

        private void SeasonDetailPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
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
    }
}
