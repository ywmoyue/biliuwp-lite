using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BiliLite.Models.Common.Player
{
    /// <summary>
    /// 跳过片段项，包含起止时间、类型、视频时长等信息。
    /// </summary>
    public class PlayerSkipItem
    {
        /// <summary>
        /// 片段起始时间（秒）。
        /// </summary>
        public double Start { get; set; }

        /// <summary>
        /// 片段结束时间（秒）。
        /// </summary>
        public double End { get; set; }

        /// <summary>
        /// 片段类型（如 sponsor、intro、outro、selfpromo）。
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 片段类型对应的名称。
        /// </summary>
        public string SegmentName => CategoryEnum switch
        {
            SponsorBlockType.Sponsor => "赞助广告",
            SponsorBlockType.Intro => "过场/开场动画",
            SponsorBlockType.Outro => "鸣谢/结束画面",
            SponsorBlockType.SelfPromo => "无偿/自我推广",
            SponsorBlockType.PoiHighlight => "精彩时刻/重点",
            SponsorBlockType.Interaction => "三连/订阅提醒",
            SponsorBlockType.Preview => "回顾/概要",
            SponsorBlockType.MusicOfftopic => "非音乐部分",
            _ => "",
        };

        /// <summary>
        /// 片段类型对应的枚举类型, 方便进行匹配。
        /// </summary>
        public SponsorBlockType CategoryEnum => Category switch
        {
            "sponsor" => SponsorBlockType.Sponsor,
            "intro" => SponsorBlockType.Intro,
            "outro" => SponsorBlockType.Outro,
            "selfpromo" => SponsorBlockType.SelfPromo,
            "poi_highlight" => SponsorBlockType.PoiHighlight,
            "interaction" => SponsorBlockType.Interaction,
            "preview" => SponsorBlockType.Preview,
            "music_offtopic" => SponsorBlockType.MusicOfftopic,
            _ => SponsorBlockType.None,
        };

        /// <summary>
        /// 片段类型对应的颜色画刷。
        /// </summary>
        public SolidColorBrush Brush => new(Color);

        /// <summary>
        /// 片段类型对应的颜色。
        /// </summary>
        public Color Color => CategoryEnum switch
        {
            SponsorBlockType.Sponsor => Colors.Green, // 赞助广告
            SponsorBlockType.Intro => Colors.Aqua, // 片头
            SponsorBlockType.Outro => Colors.Blue, // 片尾
            SponsorBlockType.SelfPromo => Colors.Yellow, // 无偿/自我推广
            SponsorBlockType.PoiHighlight => Colors.DeepPink, // 精彩时刻/重点
            SponsorBlockType.Interaction => Colors.DarkViolet, // 三连/订阅提醒
            SponsorBlockType.Preview => Colors.DodgerBlue, // 回顾/概要
            SponsorBlockType.MusicOfftopic => Colors.Red, // 非音乐部分
            _ => Colors.Gray,
        };

        /// <summary>
        /// 视频总时长（秒）。用于判断视频是否换源。
        /// </summary>
        public float VideoDuration { get; set; }

        /// <summary>
        /// cid 用于区分一个视频的多个分P
        /// </summary>
        public string Cid { get; set; }

        /// <summary>
        /// 判断片段区间是否合法。
        /// <para>前后可相等是因为有"poi_highlight"即"精彩时刻/重点"这种类型.</para>
        /// </summary>
        public bool IsSectionValid => End >= Start && (Start != 0 || End != 0);

        /// <summary>
        /// 是否需要跳过按钮。
        /// </summary>
        public bool NeedSkipButton => (int)CategoryEnum > 0;
    }
}