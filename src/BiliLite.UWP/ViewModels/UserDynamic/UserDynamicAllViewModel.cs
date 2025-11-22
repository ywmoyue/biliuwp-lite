using MapsterMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Controls.Dialogs;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Modules.User;
using BiliLite.Pages;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.UI.Xaml.Controls;
using DynamicType = Bilibili.App.Dynamic.V2.DynamicType;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicAllViewModel : BaseViewModel, IUserDynamicCommands
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly GrpcService m_grpcService;
        private readonly IMapper m_mapper;
        private readonly DynamicAPI m_dynamicApi;
        private readonly WatchLaterVM m_watchLaterVm;
        private readonly ContentFilterService m_contentFilterService;
        private int m_page = 1;
        private string m_offset = null;
        private string m_baseline = null;
        private UserDynamicShowType m_showType;

        #endregion

        #region Constructors

        public UserDynamicAllViewModel(GrpcService grpcService, IMapper mapper, ContentFilterService contentFilterService)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
            m_contentFilterService = contentFilterService;
            m_dynamicApi = new DynamicAPI();
            m_watchLaterVm = new WatchLaterVM();
            LoadMoreCommand = new RelayCommand(LoadMore);
            UserCommand = new RelayCommand<object>(OpenUser);
            DetailCommand = new RelayCommand<string>(OpenDetail);
            ImageCommand = new RelayCommand<object>(OpenImage);
            WebDetailCommand = new RelayCommand<string>(OpenWebDetail);
            CommentCommand = new RelayCommand<DynamicV2ItemViewModel>(OpenComment);
            LikeCommand = new RelayCommand<DynamicV2ItemViewModel>(DoLike);
            RepostCommand = new RelayCommand<DynamicV2ItemViewModel>(OpenSendDynamicDialog);
            LaunchUrlCommand = new RelayCommand<string>(LaunchUrl);
            CopyDynCommand = new RelayCommand<DynamicV2ItemViewModel>(CopyDyn);
            TagCommand = new RelayCommand<object>(OpenTag);
            WatchLaterCommand = m_watchLaterVm.AddCommandWithAvId;
        }

        #endregion

        #region Properties

        public ICommand LaunchUrlCommand { get; set; }

        public ICommand RepostCommand { get; set; }

        public ICommand LikeCommand { get; set; }

        public ICommand CommentCommand { get; set; }

        public ICommand UserCommand { get; set; }

        public ICommand LoadMoreCommand { get; private set; }

        public ICommand WebDetailCommand { get; set; }

        public ICommand DetailCommand { get; set; }

        public ICommand ImageCommand { get; set; }

        public ICommand WatchLaterCommand { get; set; }

        public ICommand CopyDynCommand { get; set; }

        public ICommand TagCommand { get; set; }

        public bool Loading { get; set; }

        public bool CanLoadMore { get; set; }

        public ObservableCollection<DynamicV2ItemViewModel> DynamicItems { get; set; }

        public double CommentControlWidth => SettingService.GetValue(SettingConstants.UI.DYNAMIC_COMMENT_WIDTH, SettingConstants.UI.DEFAULT_DYNAMIC_COMMENT_WIDTH);

        #endregion

        #region Events

        public event EventHandler<DynamicV2ItemViewModel> OpenCommentEvent;

        #endregion

        #region Private Methods

        private void OpenUser(object parameter)
        {
            this.OpenUserEx(parameter);
        }

        private void OpenComment(DynamicV2ItemViewModel data)
        {
            OpenCommentEvent?.Invoke(this, data);
        }

        private void OpenWebDetail(string dynId)
        {
            var url = $"https://www.bilibili.com/opus/{dynId}";
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = "加载中...",
                parameters = url
            });
        }

        private void OpenTag(object name)
        {
            //TODO 打开话题
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.World,
                page = typeof(WebPage),
                title = name.ToString(),
                parameters = "https://t.bilibili.com/topic/name/" + Uri.EscapeDataString(name.ToString())
            });
        }

        private void OpenImage(object data)
        {
            if (!(data is UserDynamicItemDisplayImageInfo imageInfo))
            {
                return;
            }
            MessageCenter.OpenImageViewer(imageInfo.AllImages, imageInfo.Index);
        }

        private void OpenDetail(string dynId)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(DynamicDetailPage),
                title = "动态详情",
                parameters = dynId
            });
        }

        private async void LaunchUrl(string url)
        {
            await this.LaunchUrlEx(url);
        }

        private async Task DoLikeCore(DynamicV2ItemViewModel item)
        {
            var results = await m_dynamicApi.Like(item.Extend.DynIdStr, item.Liked ? 2 : 1).Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetJson<ApiDataModel<object>>();
            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            if (item.Liked)
            {
                item.Liked = false;
                item.LikeCount -= 1;
            }
            else
            {
                item.Liked = true;
                item.LikeCount += 1;
            }
        }

        private async void DoLike(DynamicV2ItemViewModel item)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            try
            {
                await DoLikeCore(item);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        private async void OpenSendDynamicDialog(DynamicV2ItemViewModel data)
        {
            if (!await BiliExtensions.ActionCheckLogin()) return;

            var sendDynamicDialog = App.ServiceProvider.GetRequiredService<SendDynamicV2Dialog>();
            if (data != null)
            {
                sendDynamicDialog.SetRepost(data);
            }
            await NotificationShowExtensions.ShowContentDialog(sendDynamicDialog);
        }

        private void CopyDyn(DynamicV2ItemViewModel data)
        {
            var dataStr = data.SourceJson;
            NotificationShowExtensions.ShowMessageToast(dataStr.SetClipboard() ? "已复制" : "复制失败");
        }

        private void HandleDynamicResults(DynAllReply results)
        {
            CanLoadMore = results.DynamicList.HasMore;
            m_offset = results.DynamicList.HistoryOffset;
            var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.DynamicList.List
                .Where(x => x.CardType != DynamicType.Banner).ToList());

            items = m_contentFilterService.FilterDynamicItems(items);

            foreach (var item in items)
            {
                item.Parent = this;
            }
            if (m_page == 1)
                DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(items);
            else
            {
                DynamicItems.AddRange(items);
            }
        }

        private void HandleDynamicSeasonResults(DynVideoReply results)
        {
            CanLoadMore = false;
            var seasonItems = m_mapper.Map<List<UserDynamicSeasonInfo>>(results.VideoFollowList.List.ToList());
            var dynamicItems = seasonItems.Select(x => x.ToDynamicItem(this)).ToList();
            DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(dynamicItems);
        }

        private void HandleDynamicVideoResults(DynVideoReply results)
        {
            CanLoadMore = results.DynamicList.HasMore;
            m_offset = results.DynamicList.HistoryOffset;
            m_baseline = results.DynamicList.UpdateBaseline;
            var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.DynamicList.List.ToList());

            foreach (var item in items)
            {
                item.Parent = this;
            }
            if (m_page == 1)
                DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(items);
            else
            {
                DynamicItems.AddRange(items);
            }
        }

        private async Task<NavDynArticles> GetDynArticle()
        {
            if (m_offset == null)
            {
                m_baseline = "0";
            }
            var api = m_dynamicApi.Article(m_baseline);
            var results = await api.Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetData<NavDynArticles>();

            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            return data.data;
        }

        private void HandleDynamicArticleResults(NavDynArticles results)
        {
            CanLoadMore = results.HasMore;
            m_offset = results.Offset;
            m_baseline = results.UpdateBaseline;
            var dynamicItems = results.Items.Select(x => x.ToDynamicItem(this)).ToList();
            DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(dynamicItems);
        }

        #endregion

        #region Public Methods

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (DynamicItems == null || DynamicItems.Count == 0)
            {
                return;
            }

            await GetDynamicItems(m_page + 1, m_showType);
        }

        public async Task GetDynamicItems(int page = 1, UserDynamicShowType showType = UserDynamicShowType.All)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                m_page = page;
                m_showType = showType;
                if (page == 1)
                {
                    m_offset = null;
                    DynamicItems?.Clear();
                }

                switch (showType)
                {
                    case UserDynamicShowType.All:
                        {
                            var results = await m_grpcService.GetDynAll(page: page, offset: m_offset);
                            HandleDynamicResults(results);
                            break;
                        }
                    case UserDynamicShowType.Video:
                        {
                            var results = await m_grpcService.GetDynVideo(page, m_offset, m_baseline);
                            HandleDynamicVideoResults(results);
                            break;
                        }
                    case UserDynamicShowType.Season:
                        {
                            var results = await m_grpcService.GetDynVideo(page, m_offset, m_baseline);
                            HandleDynamicSeasonResults(results);
                            break;
                        }
                    case UserDynamicShowType.Article:
                        {
                            var results = await GetDynArticle();
                            HandleDynamicArticleResults(results);
                            break;
                        }
                }
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
