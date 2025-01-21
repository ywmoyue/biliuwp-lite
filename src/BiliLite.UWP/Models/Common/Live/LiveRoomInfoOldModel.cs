using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live;

public class LiveRoomInfoOldModel
{
    /// <summary>
    /// 直播间状态
    /// 0：无房间，1：有房间
    /// </summary>
    [JsonProperty("roomStatus")]
    public int RoomStatus { get; set; }

    /// <summary>
    /// 轮播状态
    /// 0：未轮播，1：轮播
    /// </summary>
    [JsonProperty("roundStatus")]
    public int RoundStatus { get; set; }

    /// <summary>
    /// 直播状态
    /// 0：未开播，1：直播中
    /// </summary>
    [JsonProperty("live_status")]
    public int LiveStatus { get; set; }

    /// <summary>
    /// 直播间网页URL
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// 直播间标题
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 直播间封面URL
    /// </summary>
    [JsonProperty("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 直播间人气
    /// 值为上次直播时刷新
    /// </summary>
    [JsonProperty("online")]
    public int Online { get; set; }

    /// <summary>
    /// 直播间ID（短号）
    /// </summary>
    [JsonProperty("roomid")]
    public int RoomId { get; set; }

    /// <summary>
    /// 广播类型
    /// </summary>
    [JsonProperty("broadcast_type")]
    public int BroadcastType { get; set; }

    /// <summary>
    /// 在线隐藏状态
    /// </summary>
    [JsonProperty("online_hidden")]
    public int OnlineHidden { get; set; }

    public string UserName { get; set; }

    public string UserId { get; set; }
}