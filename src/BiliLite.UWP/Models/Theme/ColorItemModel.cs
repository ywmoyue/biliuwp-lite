using BiliLite.ViewModels.Common;
using Windows.UI;

namespace BiliLite.Models.Theme
{
    public class ColorItemModel(bool isActived, string name, string hexCode, Color color) : BaseViewModel
    {
        public bool IsActived { get; set; } = isActived;
        public string Name { get; set; } = name;
        public string HexCode { get; set; } = hexCode;
        public Color Color { get; set; } = color;
    }
}
