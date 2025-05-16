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
        public string SectionName => Category switch
        {
            "sponsor" => "广告",
            "intro" => "过场/开场动画",
            "outro" => "鸣谢/结束画面",
            "selfpromo" => "无偿/自我推广",
            "poi_highlight" => "精彩时刻/重点",
            _ => "",
        };

        /// <summary>
        /// 视频总时长（秒）。用于判断视频是否换源。
        /// </summary>
        public float VideoDuration { get; set; }

        /// <summary>
        /// 判断片段区间是否合法。
        /// <para>前后可相等是因为有"poi_highlight"即"精彩时刻/重点"这种类型.</para>
        /// </summary>
        public bool IsSectionValid => End >= Start && (Start != 0 || End != 0);
    }
}