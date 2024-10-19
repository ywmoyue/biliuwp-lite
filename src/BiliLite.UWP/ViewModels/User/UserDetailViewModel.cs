using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.User.UserDetails;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.User
{
    public class UserDetailViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly UserDetailAPI m_userDetailApi;
        private readonly FollowAPI m_followApi;
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors

        public UserDetailViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            m_userDetailApi = new UserDetailAPI();
            m_followApi = new FollowAPI();
            AttentionCommand = new RelayCommand(DoAttentionUP);
        }

        #endregion

        #region Properties

        public ICommand AttentionCommand { get; private set; }

        [DoNotNotify]
        public string Mid { get; set; }

        public bool NeedCaptcha { get; set; }

        public bool HaveLiveRoom => UserSpaceInfo != null && UserSpaceInfo.LiveRoom != null;

        public UserCenterSpaceStatModel Stat { get; set; }

        public UserSpaceInfo UserSpaceInfo { get; set; }

        public bool IsFollowed { get; set; }

        #endregion

        #region Events

        public EventHandler LiveStreaming;

        #endregion

        #region Private Methods

        private async Task<UserCenterInfoStatModel> GetStatCore()
        {
            var result = await m_userDetailApi.UserStat(Mid).Request();

            if (!result.status) throw new CustomizedErrorException(result.message);
            var data = await result.GetData<UserCenterInfoStatModel>();
            if (!data.success) throw new CustomizedErrorException(data.message);
            return data.data;
        }

        private async Task GetSpaceStatCore()
        {
            var result = await m_userDetailApi.Space(Mid).Request();

            if (!result.status)
                throw new CustomizedErrorException(result.message);

            var data = await result.GetData<JObject>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            Stat = new UserCenterSpaceStatModel
            {
                ArticleCount = (data.data["article"]?["count"] ?? 0).ToInt32(),
                VideoCount = (data.data["archive"]?["count"] ?? 0).ToInt32(),
                FavouriteCount = (data.data["favourite2"]?["count"] ?? 0).ToInt32(),
                Follower = data.data["card"]["fans"].ToInt32(),
                Following = data.data["card"]["attention"].ToInt32(),
                CollectionCount = (data.data?["ugc_season"]?["count"] ?? 0).ToInt32() +
                                  (data.data?["series"]?["item"].ToArray().Length ?? 0)
            };

            var vipInfoData = data.data["card"]?["vip"];
            UserSpaceInfoVipInfo vipInfo = new UserSpaceInfoVipInfo();
            if(vipInfoData != null)
            {
                vipInfo = vipInfoData.ToObject<UserSpaceInfoVipInfo>();
            }

            var pendantData = data.data["card"]?["pendant"];
            UserCenterInfoPendantModel pendant = new UserCenterInfoPendantModel();
            if (pendantData != null)
            {
                pendant = pendantData.ToObject<UserCenterInfoPendantModel>();
            }

            var officialData = data.data["card"]?["official_verify"];
            UserCenterInfoOfficialModel official = new UserCenterInfoOfficialModel();
            if (officialData != null)
            {
                official = officialData.ToObject<UserCenterInfoOfficialModel>();
            }

            var liveData = data.data["live"];
            UserCenterInfoLiveRoomModel live = new UserCenterInfoLiveRoomModel();
            if (liveData != null)
            {
                live = liveData.ToObject<UserCenterInfoLiveRoomModel>();
            }

            UserSpaceInfo = new UserSpaceInfo
            {
                Name = (data.data["card"]?["name"] ?? "").ToString(),
                Mid = Mid,
                Face = (data.data["card"]?["face"] ?? "").ToString(),
                Sign = (data.data["card"]?["sign"] ?? "").ToString(),
                Vip = vipInfo,
                TopImageLight = (data.data["images"]?["imgUrl"] ?? "").ToString(),
                TopImageDark = (data.data["images"]?["night_imgurl"] ?? "").ToString(),
                Level = (data.data["card"]?["level_info"]?["current_level"] ?? 0).ToInt32(),
                Pendant = pendant,
                Official = official,
                LiveRoom = live,
            };

            IsFollowed = (data.data["card"]?["relation"]?["is_follow"] ?? 0).ToInt32() == 1;
        }

        private async Task AttentionUPCore(string mid, int mode)
        {
            var results = await m_followApi.Attention(mid, mode.ToString()).Request();
            if (!results.status)
                throw new CustomizedErrorException(results.message);

            var data = await results.GetJson<ApiDataModel<object>>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            Notify.ShowMessageToast("操作成功");
        }

        #endregion

        #region Public Methods

        public async void GetUserInfo()
        {
            try
            {
                await GetSpaceStat();
            }
            catch (Exception ex)
            {
                _logger.Log("读取个人资料失败", LogType.Error, ex);
                Notify.ShowMessageToast("读取个人资料失败");
            }
        }

        public async Task<UserCenterInfoStatModel> GetStat()
        {
            try
            {
                return await GetStatCore();
            }
            catch (Exception ex)
            {
                _logger.Log("读取个人资料失败", LogType.Error, ex);
                return null;
            }
        }

        public async Task GetSpaceStat()
        {
            try
            {
                await GetSpaceStatCore();
            }
            catch (Exception ex)
            {
                _logger.Log("读取个人资料失败", LogType.Error, ex);
            }
        }

        public async void DoAttentionUP()
        {
            var result = await AttentionUP(Mid.ToString(), IsFollowed ? 2 : 1);
            if (result)
            {
                IsFollowed = !IsFollowed;
            }
        }

        public async Task<bool> AttentionUP(string mid, int mode)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录后再操作");
                return false;
            }

            try
            {
                await AttentionUPCore(mid, mode);
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                Notify.ShowMessageToast(handel.message);
                return false;
            }
        }

        #endregion
    }
}
