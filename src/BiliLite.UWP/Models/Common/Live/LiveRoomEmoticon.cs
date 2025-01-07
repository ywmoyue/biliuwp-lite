using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomEmoticonPackage
    {
        [JsonProperty("pkg_id")]
        public int Id { get; set; }

        [JsonProperty("pkg_name")]
        public string Name { get; set; }

        /// <summary>
        /// 表情包类型
        /// 已知：3为黄豆表情包, 2为房间表情包，5为全站表情包
        /// </summary>
        [JsonProperty("pkg_type")]
        public int Type { get; set; }

        /// <summary>
        /// 表情包封面图
        /// </summary>
        [JsonProperty("current_cover")]
        public string Cover { get; set; }

        [JsonProperty("unlock_identity")]
        public int UnlockIdentity { get; set; }

        [JsonProperty("unlock_need_gift")]
        public int UnlockNeedGift { get; set; }

        [JsonProperty("emoticons")]
        public ObservableCollection<LiveRoomEmoticon> Emoticons { get; set; }
    }

    public class LiveRoomEmoticon
    {
        [JsonProperty("bulge_display")]
        public int BulgeDisplay { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        /// <summary>
        /// 是否为大表情
        /// </summary>
        public bool IsBigSticker => Width > 0;

        [JsonProperty("emoticon_id")]
        public int Id { get; set; }

        /// <summary>
        /// 表情包ID, 似乎可以唯一标识一个表情
        /// </summary>
        [JsonProperty("emoticon_unique")]
        public string Unique { get; set; }

        [JsonProperty("descript")]
        public string Descript { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// 表情文本, 用于显示至弹幕
        /// </summary>
        [JsonProperty("emoji")]
        public string Text { get; set; }

        /// <summary>
        /// 表情文本 + 解锁需要条件(如有)
        /// </summary>
        public string ShowText => Text + (UnlockShowText.Length > 0 ? "\n[" + UnlockShowText + "]" : "");

        /// <summary>
        /// 0: 未解锁, 1: 已解锁
        /// </summary>
        [JsonProperty("perm")]
        public int Perm { get; set; }

        /// <summary>
        /// 显示的图片透明度, 未解锁为0.5
        /// </summary>
        public double Opacity => Perm == 0 ? 0.5 : 1.0;

        /// <summary>
        /// 是否显示上锁图标
        /// </summary>
        public bool LockIconVisibility => Perm == 0;

        /// <summary>
        /// 解锁需要的条件
        /// </summary>
        [JsonProperty("unlock_show_text")]
        public string UnlockShowText { get; set; }
    }
}
