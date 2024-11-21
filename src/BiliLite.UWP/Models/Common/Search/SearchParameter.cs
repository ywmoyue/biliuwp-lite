namespace BiliLite.Models.Common.Search
{
    public class SearchParameter
    {
        public string Keyword { get; set; }

        public SearchType SearchType { get; set; } = SearchType.Video;
    }
}