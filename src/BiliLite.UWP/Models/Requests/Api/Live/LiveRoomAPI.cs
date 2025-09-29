using BiliLite.Extensions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using Newtonsoft.Json;

namespace BiliLite.Models.Requests.Api.Live
{
    public class LiveRoomAPI : BaseApi
    {
        private readonly CookieService m_cookieService;

        public LiveRoomAPI()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        /// <summary>
        /// 直播间信息
        /// </summary>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public ApiModel LiveRoomInfo(string roomid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/index/getInfoByRoom",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomid}&device=android"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 钱包
        /// </summary>
        /// <returns></returns>
        public ApiModel MyWallet()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/pay/v2/Pay/myWallet",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播头衔列表
        /// </summary>
        /// <returns></returns>
        public ApiModel LiveTitles()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/rc/v1/Title/getTitle",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播礼物列表
        /// </summary>
        /// <returns></returns>
        public ApiModel GiftList(int areaV2Id, int areaV2ParentId, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v4/Live/giftConfig",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&area_v2_id={areaV2Id}&area_v2_parent_id={areaV2ParentId}&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播背包
        /// </summary>
        /// <returns></returns>
        public ApiModel BagList(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/gift/bag_list",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播房间可用礼物列表
        /// </summary>
        /// <returns></returns>
        public ApiModel RoomGifts(int areaV2Id, int areaV2ParentId, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v3/live/room_gift_list",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&area_v2_id={areaV2Id}&area_v2_parent_id={areaV2ParentId}&roomid={roomId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 免费瓜子（宝箱）
        /// </summary>
        /// <returns></returns>
        public ApiModel FreeSilverTime()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/mobile/freeSilverCurrentTask",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 领取免费瓜子（宝箱）
        /// </summary>
        /// <returns></returns>
        public ApiModel GetFreeSilver()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/mobile/freeSilverAward",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 赠送背包礼物
        /// </summary>
        /// <returns></returns>
        public ApiModel SendBagGift(long ruid, int giftId, int num, int bagId, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.live.bilibili.com/xlive/revenue/v1/gift/sendBag",
                body = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey",
                need_cookie = true,
            };
            api.body += $"&biz_code=live&biz_id={roomId}&gift_id={giftId}&gift_num={num}&price=0&bag_id={bagId}&rnd={TimeExtensions.GetTimestampMs()}&ruid={ruid}&uid={SettingService.Account.UserID}";
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 赠送礼物
        /// </summary>
        /// <returns></returns>
        public ApiModel SendGift(long ruid, int giftId, string coinType, int num, int roomId, int price)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.live.bilibili.com/xlive/revenue/v1/gift/send{char.ToUpper(coinType[0]) + coinType.Substring(1)}",
                body = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey",
                need_cookie = true,
            };
            api.body += $"&biz_code=live&biz_id={roomId}&gift_id={giftId}&gift_num={num}&price={price}&rnd={TimeExtensions.GetTimestampMs()}&ruid={ruid}&uid={SettingService.Account.UserID}";
            api.body += ApiHelper.GetSign(api.body, AppKey);

            return api;
        }

        /// <summary>
        /// 发送弹幕
        /// </summary>
        /// <returns></returns>
        public ApiModel SendDanmu(string text, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.live.bilibili.com/api/sendmsg",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey",
            };
            api.body = $"cid={roomId}&mid={SettingService.Account.UserID}&msg={Uri.EscapeDataString(text)}&rnd={TimeExtensions.GetTimestampMs()}&mode=1&pool=0&type=json&color=16777215&fontsize=25&playTime=0.0";
            api.parameter += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// ios端使用的发送弹幕api
        /// 更加高级, 可用于发送大表情等
        /// </summary>
        /// <param name="roomId">直播间号</param>
        /// <param name="emoji">表情信息</param>
        /// <returns></returns>
        public ApiModel SendDanmu(int roomId, LiveRoomEmoticon emoji)
        {
            var jsonObj = new
            {
                content = $"{emoji.Text}",
                emoticon_unique = $"{emoji.Unique}",
                dm_type = 1,
            };
            var extra = JsonConvert.SerializeObject(jsonObj);

            var api = new ApiModel
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/dM/sendmsg",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey",
                body = $"msg={emoji.Unique}&dm_type=1&extra={extra}&cid={roomId}&mid={SettingService.Account.UserID}&ts={TimeExtensions.GetTimestampS()}&rnd={TimeExtensions.GetTimestampMs()}&mode=1&pool=0&type=json&color=16777215&fontsize=25"
            };
            api.parameter += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 主播详细信息
        /// </summary>
        /// <returns></returns>
        public ApiModel AnchorProfile(long uid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/live_user/v1/card/card_up",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&uid={uid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 舰队列表
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ApiModel GuardList(long ruid, int roomId, int page)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/guardTab/topList",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&page={page}&page_size=20&roomid={roomId}&ruid={ruid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 粉丝榜
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        public ApiModel FansList(long ruid, int roomId, int page)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/rankdb/v2/RoomRank/mobileMedalRank",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&page={page}&roomid={roomId}&ruid={ruid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 房间榜单
        /// </summary>
        /// <param name="ruid">主播ID</param>
        /// <param name="roomId">房间号</param>
        /// <param name="rankType">目前仅发现为online_rank</param>
        /// <param name="switchType">目前仅发现为contribution_rank</param>
        /// <returns></returns>
        public ApiModel RoomRankList(long ruid, int roomId, string rankType, string switchType)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/general-interface/v1/rank/queryContributionRank",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomId}&ruid={ruid}&type={rankType}&switch={switchType}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播间抽奖信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="buvid3">buvid鉴权 2025-07-24后强制使用</param>
        /// <returns></returns>
        public ApiModel RoomLotteryInfo(int roomId, string buvid3)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/lottery-interface/v1/lottery/getLotteryInfo",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&roomid={roomId}",
                need_cookie = true,
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", buvid3 } };
            return api;
        }

        /// <summary>
        /// 直播间超级留言(SuperChat)信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomSuperChat(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/av/v1/SuperChat/getMessageList",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomId}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomEntryAction(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Room/room_entry_action",
                body = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomId}",
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 获取弹幕连接信息 
        /// </summary>
        /// <param name="roomId">房间号</param>
        /// <param name="buvid3">BUVID3 2025-06-27后强制使用</param>
        /// <returns></returns>
        public async Task<ApiModel> GetDanmuInfo(int roomId, string buvid3)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo",
                parameter = $"?id={roomId}",
                need_cookie = true
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", buvid3 } };
            return api;
        }

        /// <summary>
        /// 使用App方法获取弹幕连接信息
        /// </summary>
        /// <param name="roomId">房间号</param>
        /// <returns></returns>
        public async Task<ApiModel> GetDanmuInfoApp(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/app-room/v1/index/getDanmuInfo",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&room_id={roomId}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 获取BUVID
        /// </summary>
        /// <returns></returns>
        public ApiModel GetBuvid()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/frontend/finger/spi",
                need_cookie = true
            };
            return api;
        }

        /// <summary>
        /// 参与天选抽奖
        /// </summary>
        /// <param name="roomId">房间号</param>
        /// <param name="lotteryId">抽奖Id</param>
        /// <param name="buvid3">buvid鉴权</param>
        /// <param name="giftId">礼物ID</param>
        /// <param name="giftNum">礼物数量</param>
        /// <returns></returns>
        public ApiModel JoinAnchorLottery(int roomId, int lotteryId, string buvid3, int giftId = 0, int giftNum = 0)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.live.bilibili.com/xlive/lottery-interface/v1/Anchor/Join",
                body = $"room_id={roomId}&id={lotteryId}&platform=pc&csrf={csrf}",
                need_cookie = true,
                headers = ApiHelper.GetDefaultHeaders(),
            };
            if (giftId != 0 && giftNum != 0) { api.body += $"&gift_id={giftId}&gift_num={giftNum}"; }
            api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", buvid3 } };
            return api;
        }

        /// <summary>
        /// 参与人气红包抽奖
        /// </summary>
        /// <param name="uid">用户uid</param>
        /// <param name="roomId">房间号</param>
        /// <param name="ruid">主播uid</param>
        /// <param name="lotteryId">抽奖id</param>
        /// <returns></returns>
        public ApiModel JoinRedPocketLottery(long uid, int roomId, long ruid, int lotteryId)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.live.bilibili.com/xlive/lottery-interface/v1/popularityRedPocket/RedPocketDraw",
                parameter = $"csrf={csrf}",
                body = $"uid={uid}&room_id={roomId}&ruid={ruid}&lot_id={lotteryId}&ts={TimeExtensions.GetTimestampS()}",
                need_cookie = true,
                headers = ApiHelper.GetDefaultHeaders()
            };
            return api;
        }

        /// <summary>
        /// 获取直播间表情
        /// </summary>
        /// <param name="roomId">房间号</param>
        /// <returns></returns>
        public ApiModel GetLiveRoomEmoticon(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-ucenter/v2/emoticon/GetEmoticons",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomId}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 获取用户对应的直播间状态
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public ApiModel GetRoomInfoOld(string userId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld",
                parameter = $"mid={userId}",
            };
            return api;
        }

        /// <summary>
        /// 获取直播间历史弹幕, 使用网页端接口
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="buvid3"></param>
        /// <returns></returns>
        public async Task<ApiModel> GetHistoryDanmu(int roomId, string buvid3)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-room/v1/dM/gethistory",
                parameter = $"roomid={roomId}",
            };

            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", buvid3 } };
            return api;
        }
    }
}
