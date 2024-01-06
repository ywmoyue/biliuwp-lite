using Windows.UI;
using Windows.UI.Xaml;

namespace BiliLite.Models.Common.Live
{
    public class GuardBuyMsgModel
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 大航海等级1: 总督 2: 提督 3:舰长
        /// </summary>
        public int GuardLevel { get; set; }

        /// <summary>
        /// 字体颜色
        /// </summary>
        public string FontColor
        {
            get => GuardLevel switch
            {
                3 => "#FF23709E",
                2 => "#FF7B166F",
                1 => "#FFC01039",
                _ => null
            };
        }

        /// <summary>
        /// 卡片颜色
        /// </summary>
        public Color CardColor
        {
            get => GuardLevel switch
            {
                3 => Color.FromArgb(255, 192, 216, 255),
                2 => Color.FromArgb(255, 250, 187, 255),
                1 => Color.FromArgb(255, 255, 185, 187),
                _ => (Color)Application.Current.Resources["CardColor"],
            };
        }

        /// <summary>
        /// 数量
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// 礼物ID
        /// </summary>
        public int GiftId { get; set; }

        /// <summary>
        /// 礼物名称
        /// </summary>
        public string GiftName { get; set; }
    }
}