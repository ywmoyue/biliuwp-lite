using System.Collections.Generic;

namespace BiliLite.Models.Common.Article;

public class ArticleReaderHostFetchData
{
    public string Id { get; set; }

    public string Url { get; set; }

    public string Method { get; set; } = "GET";

    public Dictionary<string,string> Headers { get; set; }
}