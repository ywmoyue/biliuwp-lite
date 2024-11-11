namespace BiliLite.Models.Common.Search
{
    public class SearchAnimeItem
    {
        public string type { get; set; }
        public string season_id { get; set; }
        public string media_id { get; set; }
        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string areas { get; set; }
        public string cv { get; set; }
        public string styles { get; set; }
        public string desc { get; set; }
        public long pubtime { get; set; }
        public string season_type_name { get; set; }

        private string _pic;
        public string cover
        {
            get { return _pic; }
            set { _pic = value; }
        }

        public string angle_title { get; set; }
        public bool showBadge
        {
            get
            {
                return !string.IsNullOrEmpty(angle_title);
            }
        }
    }
}