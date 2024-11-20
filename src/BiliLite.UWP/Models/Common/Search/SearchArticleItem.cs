using System.Collections.Generic;

namespace BiliLite.Models.Common.Search
{
    public class SearchArticleItem
    {

        public string mid { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {
                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string category_name { get; set; }
        public string type { get; set; }
        public string desc { get; set; }
        public long like { get; set; }
        public long view { get; set; }
        public long reply { get; set; }
        public string id { get; set; }
        public List<string> image_urls { get; set; }
        public string cover
        {
            get
            {
                if (image_urls != null && image_urls.Count != 0)
                {
                    return "https:" + image_urls[0];
                }
                return null;
            }
        }
    }
}