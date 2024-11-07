namespace BiliLite.Models.Common.Search
{
    public class SearchLiveRoomItem
    {

        public string roomid { get; set; }

        private string _title;

        public string title
        {
            get { return _title; }
            set
            {

                _title = System.Web.HttpUtility.HtmlDecode(value.Replace("<em class=\"keyword\">", "").Replace("</em>", ""));
            }
        }
        public string uname { get; set; }
        public string tags { get; set; }
        public string cate_name { get; set; }
        public int online { get; set; }


        private string _user_cover;
        public string user_cover
        {
            get { return _user_cover; }
            set { _user_cover = "https:" + value; }
        }
        private string _uface;
        public string uface
        {
            get { return _uface; }
            set { _uface = "https:" + value; }
        }
        private string _cover;
        public string cover
        {
            get { return _cover; }
            set { _cover = "https:" + value; }
        }

    }
}