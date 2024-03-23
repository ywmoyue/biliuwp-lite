using Windows.UI;
using NSDanmaku.Model;

namespace BiliLite.Models.Common.Danmaku
{
    public class BiliDanmakuItem
    {
        public string Text { get; set; }

        public double Size { get; set; }

        public Color Color { get; set; }

        public double Time { get; set; }

        public DanmakuLocation Location { get; set; }
    }
}
