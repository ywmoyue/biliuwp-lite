
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace BiliLite.ViewModels.UserDynamic
{
    [RegisterTransientViewModel]
    public class UserDynamicRepostViewModel : BaseViewModel, IUserDynamicCommands
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly DynamicAPI m_dynamicApi;
        private readonly GrpcService m_grpcService;
        private string m_offset = "";
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors

        public UserDynamicRepostViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
            m_dynamicApi = new DynamicAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            UserCommand = new RelayCommand<object>(OpenUser);
            DetailCommand = new RelayCommand<string>(DynamicExtensions.OpenDetail);
            ImageCommand = new RelayCommand<object>(DynamicExtensions.OpenImage);
            WebDetailCommand = new RelayCommand<string>(DynamicExtensions.OpenWebDetail);
            LikeCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.DoLike);
            RepostCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.OpenSendDynamicDialog);
            LaunchUrlCommand = new RelayCommand<string>(DynamicExtensions.LaunchUrl);
            CopyDynCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.CopyDyn);
            OpenArticleCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.OpenArticle);
            TagCommand = new RelayCommand<object>(DynamicExtensions.OpenTag);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }
        public ICommand WebDetailCommand { get; set; }
        public ICommand DetailCommand { get; set; }
        public ICommand ImageCommand { get; set; }
        public ICommand WatchLaterCommand { get; set; }
        public ICommand CopyDynCommand { get; set; }
        public ICommand TagCommand { get; set; }
        public ICommand LaunchUrlCommand { get; set; }
        public ICommand RepostCommand { get; set; }
        public ICommand LikeCommand { get; set; }
        public ICommand CommentCommand { get; set; }
        public ICommand UserCommand { get; set; }

        public ICommand OpenArticleCommand { get; set; }

        [DoNotNotify]
        public string ID { get; set; }

        public bool Loading { get; set; }

        public bool CanLoadMore { get; set; }

        public ObservableCollection<DynamicV2ItemViewModel> DynamicItems { get; set; } =
            new ObservableCollection<DynamicV2ItemViewModel>();

        #endregion

        #region Public Methods

        public async Task GetDynamicItemReposts(bool reload = false)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                await GetDynamicItemRepostsCore(reload);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<UserDynamicRepostViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
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
            DynamicItems = null;
            await GetDynamicItemReposts(true);
        }

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
            var last = DynamicItems.LastOrDefault();
            await GetDynamicItemReposts();
        }

        public void OpenUser(object id)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = "用户中心",
                parameters = id
            });
        }

        public void Clear()
        {
            m_offset = "";
            DynamicItems = null;
        }

        #endregion

        #region Private Methods

        private void HandleDynamicResults(RepostListRsp results, bool firstPage)
        {
            CanLoadMore = results.HasMore;
            m_offset = results.Offset;
            var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.List.ToList());
            foreach (var item in items)
            {
                item.Parent = this;
            }
            if (firstPage)
                DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(items);
            else
            {
                DynamicItems.AddRange(items);
            }
        }

        private async Task GetDynamicItemRepostsCore(bool reload = false)
        {
            var firstPage = false;
            if (reload)
            {
                m_offset = "";
            }

            if (string.IsNullOrEmpty(m_offset)) firstPage = true;
            var results = await m_grpcService.GetDynRepostList(ID, m_offset);
            HandleDynamicResults(results, firstPage);
        }

        #endregion
    }
}
