using System.Collections.Generic;

namespace BiliLite.Models.Common.Video;

public static class QualityConstants
{
    private static Dictionary<int, string> m_dictionary = new Dictionary<int, string>()
    {
        {6,"240P 极速" },
        {16,"360P 流畅" },
        {32,"480P 清晰" },
        {64,"720P 高清" },
        {74,"720P60 高帧率" },
        {80,"1080P 高清" },
        {112,"1080P+ 高码率" },
        {116,"1080P60 高帧率" },
        {120,"4K 超清" },
        {125,"HDR 真彩色" },
        {126,"杜比视界" },
        {127,"8K 超高清" }
    };

    public static Dictionary<int, string> Dictionary => m_dictionary;
}