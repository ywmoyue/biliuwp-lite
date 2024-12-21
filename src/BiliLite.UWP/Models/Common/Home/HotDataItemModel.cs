using BiliLite.Models.Common.Recommend;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class HotDataItemModel
    {
        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("card_goto")]
        public string CardGoto { get; set; }

        public string Param { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        public string Idx { get; set; }

        public string Uri { get; set; }

        [JsonProperty("cover_right_text_1")]
        public string CoverRightText1 { get; set; }

        [JsonProperty("right_desc_1")]
        public string RightDesc1 { get; set; }

        [JsonProperty("right_desc_2")]
        public string RightDesc2 { get; set; }

        [JsonProperty("cover_left_text_1")]
        public string CoverLeftText1 { get; set; }

        [JsonProperty("cover_left_text_2")]
        public string CoverLeftText2 { get; set; }

        [JsonProperty("cover_left_text_3")]
        public string CoverLeftText3 { get; set; }

        public string TextInfo1 => string.IsNullOrEmpty(CoverRightText1) ? CoverLeftText1 : CoverRightText1;

        public string TextInfo2 => string.IsNullOrEmpty(RightDesc1) ? CoverLeftText2 : RightDesc1;

        public string TextInfo3 => string.IsNullOrEmpty(RightDesc2) ? CoverLeftText3 : RightDesc2;

        [JsonProperty("rcmd_reason_style")]
        public RecommendRcmdReasonStyleModel RcmdReasonStyle { get; set; }

        [JsonProperty("top_rcmd_reason_style")]
        public RecommendRcmdReasonStyleModel TopRcmdReasonStyle { get; set; }

        public RecommendRcmdReasonStyleModel RcmdReason => RcmdReasonStyle ?? TopRcmdReasonStyle;
    }
}