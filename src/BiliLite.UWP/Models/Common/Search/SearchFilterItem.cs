namespace BiliLite.Models.Common.Search
{
    public class SearchFilterItem
    {
        public SearchFilterItem(string name, string value)
        {
            this.name = name;
            this.value = value;
        }
        public string name { get; set; }
        public string value { get; set; }
    }
}
