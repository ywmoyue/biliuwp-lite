namespace BiliLite.Models.Common.Search
{
    public class SearchTopicItem
    {
        public string arcurl { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {
                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }

        private string _description;

        public string description
        {
            get { return _description; }
            set
            {
                _description = value.Replace("<em class=\"keyword\">", "").Replace("</em>", "");
            }
        }


        public long pubdate { get; set; }


        private string _pic;
        public string cover
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }


    }
}