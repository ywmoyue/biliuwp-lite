using BiliLite.Converters;
using System;

namespace BiliLite.Models.Common.Search
{
    public class SearchVideoItem
    {
        public string type { get; set; }
        public string typename { get; set; }
        public string author { get; set; }
        public string id { get; set; }
        public string aid { get; set; }
        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string tag { get; set; }
        public int? play { get; set; }
        public int video_review { get; set; }
        public int review { get; set; }
        public int favorites { get; set; }
        public string duration { get; set; }

        public string DurationShow
        {
            get
            {
                try
                {
                    return TimeSpanStrFormatConverter.Convert(duration);
                }
                catch (Exception ex)
                {
                    return duration;
                }
            }
        }

        private string _pic;
        public string pic
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }

        public string Description { get; set; }
    }
}