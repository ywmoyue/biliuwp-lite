using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    [Serializable]
    public class LiveGiftItem
    {
        public int Id { get; set; }

        [JsonProperty("bag_id")]
        public int BagId { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public int Type { get; set; }

        [JsonProperty("coin_type")]
        public string CoinType { get; set; }

        [JsonProperty("is_gold")]
        public bool IsGold => CoinType == "gold";

        [JsonProperty("bag_gift")]

        public int BagGift { get; set; }

        public int Effect { get; set; }

        [JsonProperty("corner_mark")]
        public string CornerMark { get; set; }

        [JsonProperty("show_corner_mark")]
        public bool ShowCornerMark => !string.IsNullOrEmpty(CornerMark);

        [JsonProperty("corner_background")]
        public string CornerBackground { get; set; }

        public int Broadcast { get; set; }

        public int Draw { get; set; }

        [JsonProperty("stay_time")]
        public int StayTime { get; set; }

        [JsonProperty("animation_frame_num")]
        public int AnimationFrameNum { get; set; }

        public string Desc { get; set; }

        public string Rule { get; set; }

        public string Rights { get; set; }

        [JsonProperty("privilege_required")]
        public int PrivilegeRequired { get; set; }

        [JsonProperty("count_map")]
        public List<LiveGiftItemCountMap> CountMap { get; set; }

        [JsonProperty("img_basic")]
        public string ImgBasic { get; set; }

        [JsonProperty("img_dynamic")]
        public string ImgDynamic { get; set; }

        [JsonProperty("frame_animation")]
        public string FrameAnimation { get; set; }

        public string Gif { get; set; }

        public string Webp { get; set; }

        [JsonProperty("full_sc_web")]
        public string FullScWeb { get; set; }

        [JsonProperty("full_sc_horizontal")]
        public string FullScHorizontal { get; set; }

        [JsonProperty("full_sc_vertical")]
        public string FullScVertical { get; set; }

        [JsonProperty("full_sc_horizontal_svga")]
        public string FullScHorizontalSvga { get; set; }

        [JsonProperty("full_sc_vertical_svga")]
        public string FullScVerticalSvga { get; set; }

        [JsonProperty("bullet_head")]
        public string BulletHead { get; set; }

        [JsonProperty("bullet_tail")]
        public string BulletTail { get; set; }

        [JsonProperty("limit_interval")]
        public int LimitInterval { get; set; }

        [JsonProperty("bind_ruid")]
        public long BindRuid { get; set; }

        [JsonProperty("bind_roomid")]
        public int BindRoomid { get; set; }

        [JsonProperty("bag_coin_type")]
        public int BagCoinType { get; set; }

        [JsonProperty("broadcast_id")]
        public int BroadcastId { get; set; }

        [JsonProperty("draw_id")]
        public int DrawId { get; set; }

        [JsonProperty("gift_type")]
        public int GiftType { get; set; }

        public int Weight { get; set; }

        [JsonProperty("max_send_limit")]
        public int MaxSendLimit { get; set; }

        [JsonProperty("gift_num")]
        public int GiftNum { get; set; } = 0;

        [JsonProperty("combo_resources_id")]
        public int ComboResourcesId { get; set; }

        [JsonProperty("goods_id")]
        public long GoodsId { get; set; }

        public int Num { get; set; } = 1;
    }
}