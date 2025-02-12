using Windows.UI;

namespace BiliLite.Models.Theme
{
    public class ColorItemModel(string name, string hexCode, Color color)
    {
        public string Name { get; set; } = name;
        public string HexCode { get; set; } = hexCode;
        public Color Color { get; set; } = color;
    }
}
