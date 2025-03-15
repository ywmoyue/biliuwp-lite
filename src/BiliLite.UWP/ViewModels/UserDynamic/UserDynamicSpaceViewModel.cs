using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Exceptions;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Modules.User;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicSpaceViewModel : BaseViewModel, IUserDynamicCommands
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly GrpcService m_grpcService;
        private readonly IMapper m_mapper;
        private readonly WatchLaterVM m_watchLaterVm;
        private int m_page = 1;
        private string m_offset = null;

        #endregion

        #region Constructors

        public UserDynamicSpaceViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
            m_watchLaterVm = new WatchLaterVM();
            LoadMoreCommand = new RelayCommand(LoadMore);
            UserCommand = new RelayCommand<object>(OpenUser);
            DetailCommand = new RelayCommand<string>(DynamicExtensions.OpenDetail);
            ImageCommand = new RelayCommand<object>(DynamicExtensions.OpenImage);
            WebDetailCommand = new RelayCommand<string>(DynamicExtensions.OpenWebDetail);
            CommentCommand = new RelayCommand<DynamicV2ItemViewModel>(OpenComment);
            LikeCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.DoLike);
            RepostCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.OpenSendDynamicDialog);
            LaunchUrlCommand = new RelayCommand<string>(DynamicExtensions.LaunchUrl);
            CopyDynCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.CopyDyn);
            TagCommand = new RelayCommand<object>(DynamicExtensions.OpenTag);
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

        public bool CanLoadMore { get; set; }

        public bool Loading { get; set; }

        public string UserId { get; set; }

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

        private void HandleDynamicResults(DynSpaceRsp results)
        {
            CanLoadMore = results.HasMore;
            m_offset = results.HistoryOffset;
            var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.List.ToList());
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

        public async Task GetDynamicItems(int page = 1)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                m_page = page;
                if (page == 1)
                {
                    m_offset = null;
                }
                var results = await m_grpcService.GetDynSpace(UserId.ToInt64(), page: page, offset: m_offset);
                HandleDynamicResults(results);
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
