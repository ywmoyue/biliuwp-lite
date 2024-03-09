using AutoMapper;
using BiliLite.ViewModels.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules.User.UserDetail;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BiliLite.Models.Common;
using System;
using BiliLite.Services;

namespace BiliLite.ViewModels.User
{
    public class UserFollowingTagsFlyoutViewModel : BaseViewModel
    {
        #region Fields

        private readonly IMapper m_mapper;
        private readonly UserDetailAPI m_userDetailApi;
        private List<UserRelationFollowingTagViewModel> m_followingTags;
        private string m_userId;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public UserFollowingTagsFlyoutViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            m_userDetailApi = new UserDetailAPI();
        }

        #endregion

        #region Properties

        public List<UserRelationFollowingTagViewModel> FollowingTags { get; set; }

        #endregion

        #region Private Methods

        private async Task UpdateFollowingTagUserCore()
        {
            var followingTags = FollowingTags
                .Where(x => x.UserInThisTag)
                .Select(x => x.TagId)
                .ToList();
            var api = m_userDetailApi.AddFollowingTagUsers(new List<long>() { m_userId.ToInt64() }, followingTags);
            var results = await api.Request();

            if (!results.status)
                throw new CustomizedErrorException(results.message);
            var data = await results.GetData<object>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);
            m_followingTags = FollowingTags.ObjectCloneWithoutSerializable();
        }


        private async Task GetFollowingTagsCore()
        {
            var api = m_userDetailApi.FollowingsTag();
            var results = await api.Request();
            if (!results.status) throw new CustomizedErrorException(results.message);

            var data = await results.GetData<JArray>();
            var items = JsonConvert.DeserializeObject<List<FollowTlistItemModel>>(data.data.ToString());
            FollowingTags = m_mapper.Map<List<UserRelationFollowingTagViewModel>>(items);

            m_followingTags = FollowingTags.ObjectCloneWithoutSerializable();
        }

        private async Task GetFollowingTagUser()
        {
            var api = m_userDetailApi.FollowingTagUser(m_userId.ToInt64());
            var results = await api.Request();
            if (!results.status) throw new CustomizedErrorException(results.message);
            var data = await results.GetData<Dictionary<string, string>>();
            foreach (var tag in FollowingTags)
            {
                tag.UserInThisTag = data.data.ContainsKey(tag.TagId.ToString());
            }

            m_followingTags = FollowingTags.ObjectCloneWithoutSerializable();
        }

        #endregion

        #region Public Methods

        public async Task Init(string userId)
        {
            m_userId = userId;
            await GetFollowingTagsCore();
            await GetFollowingTagUser();
        }

        public async Task SaveFollowingTagUser()
        {
            try
            {
                await UpdateFollowingTagUserCore();
            }
            catch (Exception ex)
            {
                _logger.Log("设置关注分组失败", LogType.Error, ex);
                Notify.ShowMessageToast("设置关注分组失败");
            }
        }

        public void CancelSaveFollowingTagUser()
        {
            FollowingTags = m_followingTags.ObjectCloneWithoutSerializable();
        }

        #endregion
    }
}
