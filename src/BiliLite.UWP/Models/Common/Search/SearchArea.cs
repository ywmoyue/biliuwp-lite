namespace BiliLite.Models.Common.Search
{
    public class SearchArea
    {
        public SearchArea(string name, string area)
        {
            this.name = name;
            this.area = area;
        }
        public string name { get; set; }
        public string area { get; set; }
    }
}