﻿namespace BiliLite.Models.Common.Season;

public class SeasonShortReviewItemStatModel
{
    /// <summary>
    /// 是否已经点踩👎
    /// </summary>
    public int Disliked { get; set; }

    /// <summary>
    /// 是否已经点赞👍
    /// </summary>
    public int Liked { get; set; }

    /// <summary>
    /// 点赞数量
    /// </summary>
    public int Likes { get; set; }
}