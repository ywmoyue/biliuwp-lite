using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.UserDynamic
{
    public class UserDynamicSpaceViewModel : BaseViewModel
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly GrpcService m_grpcService;
        private readonly IMapper m_mapper;

        public UserDynamicSpaceViewModel(GrpcService grpcService, IMapper mapper)
        {
            m_grpcService = grpcService;
            m_mapper = mapper;
        }

        public bool CanLoadMore { get; set; }

        public bool Loading { get; set; }

        public string UserId { get; set; }

        public ObservableCollection<DynamicV2ItemViewModel> DynamicItems { get; set; }

        private void HandleDynamicResults(DynSpaceRsp results)
        {
            CanLoadMore = results.HasMore;
            try
            {
                var items = m_mapper.Map<List<DynamicV2ItemViewModel>>(results.List.ToList());
                DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>(items);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task GetDynamicItems()
        {
            try
            {
                CanLoadMore = false;
                Loading = true;
                var results = await m_grpcService.GetDynSpace(UserId.ToInt64());
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
