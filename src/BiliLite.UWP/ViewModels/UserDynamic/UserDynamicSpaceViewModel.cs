using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicSpaceViewModel : BaseViewModel
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly GrpcService m_grpcService;
        private readonly IMapper m_mapper;
        private int m_page = 1;
        private string m_offset = null;

        public UserDynamicSpaceViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        public ICommand LoadMoreCommand { get; private set; }

        public bool CanLoadMore { get; set; }

        public bool Loading { get; set; }

        public string UserId { get; set; }

        public ObservableCollection<DynamicV2ItemViewModel> DynamicItems { get; set; }

        private void HandleDynamicResults(DynSpaceRsp results)
        {
            CanLoadMore = results.HasMore;
            m_offset = results.HistoryOffset;
            var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.List.ToList());
            if (m_page == 1)
                DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(items);
            else
            {
                DynamicItems.AddRange(items);
            }
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

            await GetDynamicItems(m_page + 1);
        }

        public async Task GetDynamicItems(int page = 1)
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                m_page = page;
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
    }
}
