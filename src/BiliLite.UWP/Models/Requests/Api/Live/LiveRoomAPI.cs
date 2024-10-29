using BiliLite.Extensions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using BiliLite.Models.Common;

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
        public ApiModel GiftList(int area_v2_id, int area_v2_parent_id, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v4/Live/giftConfig",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&roomid={roomId}"
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
        public ApiModel RoomGifts(int area_v2_id, int area_v2_parent_id, int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/gift/v3/live/room_gift_list",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&area_v2_id={area_v2_id}&area_v2_parent_id={area_v2_parent_id}&roomid={roomId}"
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
            api.body += $"&biz_code=live&biz_id={roomId}&gift_id={giftId}&gift_num={num}&price=0&bag_id={bagId}&rnd={TimeExtensions.GetTimestampMS()}&ruid={ruid}&uid={SettingService.Account.UserID}";
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
            api.body += $"&biz_code=live&biz_id={roomId}&gift_id={giftId}&gift_num={num}&price={price}&rnd={TimeExtensions.GetTimestampMS()}&ruid={ruid}&uid={SettingService.Account.UserID}";
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
            api.body = $"cid={roomId}&mid={SettingService.Account.UserID}&msg={Uri.EscapeDataString(text)}&rnd={TimeExtensions.GetTimestampMS()}&mode=1&pool=0&type=json&color=16777215&fontsize=25&playTime=0.0";
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
        /// <param name="rank_type">目前仅发现为online_rank</param>
        /// <param name="switch_type">目前仅发现为contribution_rank</param>
        /// <returns></returns>
        public ApiModel RoomRankList(long ruid, int roomId, string rank_type, string switch_type)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/general-interface/v1/rank/queryContributionRank",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&room_id={roomId}&ruid={ruid}&type={rank_type}&switch={switch_type}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 直播间抽奖信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        public ApiModel RoomLotteryInfo(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/lottery-interface/v1/lottery/getLotteryInfo",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&roomid={roomId}",
                need_cookie = true,
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
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
        /// <returns></returns>
        public ApiModel GetDanmuInfo(int roomId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo",
                parameter = $"?id={roomId}",
                need_cookie = true
            };
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
        /// <param name="lottery_id">抽奖Id</param>
        /// <returns></returns>
        public ApiModel JoinAnchorLottery(int roomId, int lottery_id, string buvid3, int gift_id = 0, int gift_num = 0)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.live.bilibili.com/xlive/lottery-interface/v1/Anchor/Join",
                body = $"room_id={roomId}&id={lottery_id}&platform=pc&csrf={csrf}",
                need_cookie = true,
                headers = ApiHelper.GetDefaultHeaders(),
            };
            if (gift_id != 0 && gift_num != 0) { api.body += $"&gift_id={gift_id}&gift_num={gift_num}"; }
            api.ExtraCookies = new Dictionary<string, string>() { { "buvid3", buvid3 } };
            return api;
        }

        /// <summary>
        /// 参与人气红包抽奖
        /// </summary>
        /// <param name="uid">用户uid</param>
        /// <param name="room_id">房间号</param>
        /// <param name="ruid">主播uid</param>
        /// <param name="lot_id">抽奖id</param>
        /// <returns></returns>
        public ApiModel JoinRedPocketLottery(long uid, int room_id, long ruid, int lot_id)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.live.bilibili.com/xlive/lottery-interface/v1/popularityRedPocket/RedPocketDraw",
                parameter = $"csrf={csrf}",
                body = $"uid={uid}&room_id={room_id}&ruid={ruid}&lot_id={lot_id}&ts={TimeExtensions.GetTimestampS()}",
                need_cookie = true,
                headers = ApiHelper.GetDefaultHeaders()
            };
            return api;
        }
    }
}
