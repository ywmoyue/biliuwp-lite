using Windows.UI;

namespace BiliLite.Models.Theme
{
    public class ColorItemModel(string id, string hexCode, Color color)
    {
        public string ID { get; set; } = id;
        public string HexCode { get; set; } = hexCode;
        public Color Color { get; set; } = color;
    }
}
