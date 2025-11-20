using Newtonsoft.Json;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Controls;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomAnchorLotteryInfoModel
    {
        [JsonProperty("asset_icon")]
        public string AssetIcon { get; set; }

        /// <summary>
        /// 奖品的图片(因为有可能只是b站礼物)
        /// </summary>
        [JsonProperty("award_image")]
        public string AwardImage { get; set; }

        /// <summary>
        /// 奖品名称
        /// </summary>
        [JsonProperty("award_name")]
        public string AwardName { get; set; }

        /// <summary>
        /// 奖品个数
        /// </summary>
        [JsonProperty("award_num")]
        public int AwardNum { get; set; }

        /// <summary>
        /// 中奖的用户信息
        /// </summary>
        [JsonProperty("award_users")]
        public ObservableCollection<LiveRoomEndAnchorLotteryInfoUserModel> AwardUsers { get; set; }

        public StackPanel WinnerList => AwardUsers == null ? new StackPanel() : new LiveRoomEndAnchorLotteryInfoModel().GenerateWinnerList(AwardUsers);

        /// <summary>
        /// 需要发礼物才能参与的抽奖中, 已发送的礼物数量
        /// </summary>
        [JsonProperty("cur_gift_num")]
        public int CurGiftNum { get; set; }

        [JsonProperty("current_time")]
        public long CurrentTime { get; set; }

        /// <summary>
        /// 抽奖弹幕
        /// </summary>
        public string Danmu { get; set; }

        /// <summary>
        /// 是否显示弹幕, 有时无须弹幕即可抽奖
        /// </summary>
        public bool ShowDanmu => !string.IsNullOrEmpty(Danmu);

        /// <summary>
        /// 需要发礼物才能参与的抽奖中, 需要的礼物Id
        /// </summary>
        [JsonProperty("gift_id")]
        public int GiftId { get; set; }

        [JsonProperty("show_gift")]
        public bool ShowGift => GiftId != 0;

        /// <summary>
        /// 参与抽奖需要发送的礼物名字
        /// </summary>
        [JsonProperty("gift_name")]
        public string GiftName { get; set; }

        /// <summary>
        /// 参与抽奖需要发送的礼物数量
        /// </summary>
        [JsonProperty("gift_num")]
        public int GiftNum { get; set; }

        /// <summary>
        /// 参与抽奖需要发送的礼物价格
        /// </summary>
        [JsonProperty("gift_price")]
        public int GiftPrice { get; set; }

        /// <summary>
        /// 待调查... 可能用于控制抽奖结束后按钮的关闭时间?
        /// </summary>
        [JsonProperty("goaway_time")]
        public int GoawayTime { get; set; }

        /// <summary>
        /// 抽奖Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 是否已加入, 1为已参与
        /// </summary>
        [JsonProperty("join_type")]
        public int JoinType { get; set; }

        /// <summary>
        /// 抽奖的状态, 1为正在倒计时, 2为已开奖
        /// </summary>
        [JsonProperty("lot_status")]
        public int LotStatus { get; set; }

        [JsonProperty("max_time")]
        public int MaxTime { get; set; }

        /// <summary>
        /// 参与抽奖的需求
        /// </summary>
        [JsonProperty("require_text")]
        public string RequireText { get; set; }

        /// <summary>
        /// 参与抽奖的需求类型?
        /// </summary>
        [JsonProperty("require_type")]
        public int RequireType { get; set; }

        /// <summary>
        /// 参与抽奖的需求等级? 可能和粉丝牌等级有关? 待调查
        /// </summary>
        [JsonProperty("require_value")]
        public int RequireValue { get; set; }

        [JsonProperty("room_id")]
        public int RoomId { get; set; }

        [JsonProperty("send_gift_ensure")]
        public int SendGiftEnsure { get; set; }

        [JsonProperty("show_panel")]
        public int ShowPanel { get; set; }

        public int Status { get; set; }

        /// <summary>
        /// 剩余时间
        /// </summary>
        public int Time { get; set; }

        public string Url { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }
}