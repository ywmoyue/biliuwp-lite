using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season;

public class SeasonShortReviewItemModel
{
    public long Ctime { get; set; }

    public long Mid { get; set; }

    [JsonProperty("review_id")]
    public int ReviewId { get; set; }

    public string Content { get; set; }

    public string Progress { get; set; }

    public int Score { get; set; }

    /// <summary>
    /// 评分，转为5分制
    /// </summary>
    public int Score5 => Score / 2;

    public SeasonShortReviewItemAuthorModel Author { get; set; }

    public SeasonShortReviewItemStatModel Stat { get; set; }
}