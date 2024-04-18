using AutoMapper;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using Bilibili.App.Dynamic.V2;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using BiliLite.Dialogs;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Modules;
using BiliLite.Pages;
using BiliLite.Pages.User;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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
        private int m_page = 1;
        private string m_offset = null;
        private string m_baseline = null;

        #endregion

        #region Constructors

        public UserDynamicAllViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
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

        public int CommentControlWidth
        {
            get
            {
                return SettingService.GetValue(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320);
            }
        }

        #endregion

        #region Events

        public event EventHandler<DynamicV2ItemViewModel> OpenCommentEvent;

        #endregion

        #region Private Methods

        private void OpenUser(object userId)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = userId
            });
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
            var result = await MessageCenter.HandelUrl(url);
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
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
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
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
            await sendDynamicDialog.ShowAsync();
        }

        private void CopyDyn(DynamicV2ItemViewModel data)
        {
            var dataStr = JsonConvert.SerializeObject(data);
            Notify.ShowMessageToast(dataStr.SetClipboard() ? "已复制" : "复制失败");
        }

        private void HandleDynamicResults(DynAllReply results)
        {
            CanLoadMore = results.DynamicList.HasMore;
            m_offset = results.DynamicList.HistoryOffset;
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

        private void HandleDynamicSeasonResults(DynVideoReply results)
        {
            CanLoadMore = false;
            var seasonItems = m_mapper.Map<List<UserDynamicSeasonInfo>>(results.VideoFollowList.List.ToList());
            var dynamicItems = seasonItems.Select(x => x.ToDynamicItem()).ToList();
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

            await GetDynamicItems(m_page + 1);
        }

        public async Task GetDynamicItems(int page = 1, UserDynamicShowType showType = UserDynamicShowType.All)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                m_page = page;
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
                        throw new NotImplementedException();
                        break;
                }
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
