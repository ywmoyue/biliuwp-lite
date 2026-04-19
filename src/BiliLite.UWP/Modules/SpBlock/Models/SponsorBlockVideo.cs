using System.Collections.Generic;

namespace BiliLite.Modules.SpBlock.Models;

public class SponsorBlockVideo
{
    /// <summary>
    /// 视频的bv号
    /// </summary>
    public string VideoId { get; set; }

    /// <summary>
    /// 片段列表
    /// </summary>
    public List<SponsorBlockSegment> Segments { get; set; }
}