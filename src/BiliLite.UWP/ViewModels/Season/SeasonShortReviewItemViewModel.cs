using BiliLite.Models.Common.Season;

namespace BiliLite.ViewModels.Season;

public class SeasonShortReviewItemViewModel
{
    public long Ctime { get; set; }

    public long Mid { get; set; }

    public int ReviewId { get; set; }

    public string Content { get; set; }

    public string Progress { get; set; }

    public int Score { get; set; }

    /// <summary>
    /// 评分，转为5分制
    /// </summary>
    public int Score5 => Score / 2;

    public SeasonShortReviewItemAuthorModel Author { get; set; }

    public SeasonShortReviewItemStatViewModel Stat { get; set; }
}