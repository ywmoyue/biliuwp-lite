using System.Linq;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace BiliLite.Models.Common.Live
{
    public class DanmuMsgModel
    {
        /// <summary>
        /// 内容文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 将Text经过处理后的富文本
        /// </summary>
        public RichTextBlock RichText { get; set; }

        /// <summary>
        /// 弹幕颜色，默认白色
        /// </summary>
        public string DanmuColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户名颜色, 默认灰色
        /// </summary>
        public string UserNameColor { get; set; } = "#FF808080";

        /// <summary>
        /// 用户名字重, 默认Normal
        /// </summary>
        public string UserNameFontWeight { get; set; } = "Normal";

        /// <summary>
        /// 用户uid
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Face { get; set; }

        ///// <summary>
        ///// 等级
        ///// </summary>
        //public string UserLevel { get; set; }

        ///// <summary>
        ///// 等级颜色,默认灰色
        ///// </summary>
        //public string UserLevelColor { get; set; } = "#FF808080";

        /// <summary>
        /// 用户头衔id（对应的是CSS名）
        /// </summary>
        public string UserTitleID { get; set; }

        /// <summary>
        /// 用户头衔图片
        /// </summary>
        public string UserTitleImage => LiveRoomViewModel.Titles.FirstOrDefault(x => x.Id == UserTitleID)?.Img;

        /// <summary>
        /// 用户角色
        /// </summary>
        public string Role { get; set; }

        /// <summary>
        /// 勋章名称
        /// </summary>
        public string MedalName { get; set; }

        /// <summary>
        /// 勋章等级
        /// </summary>
        public string MedalLevel { get; set; }

        /// <summary>
        /// 勋章颜色
        /// </summary>
        public string MedalColor { get; set; }
        
        /// <summary>
        /// 用户上的舰的名称
        /// </summary>
        public string UserCaptain {  get; set; }

        /// <summary>
        /// 用户上的舰的图片
        /// </summary>
        public string UserCaptainImage 
        {
             get => UserCaptain switch
                    {
                        "舰长" => "ms-appx:///Assets/Live/ic_live_guard_3.png",
                        "提督" => "ms-appx:///Assets/Live/ic_live_guard_2.png",
                        "总督" => "ms-appx:///Assets/Live/ic_live_guard_1.png",
                        _ => null,
                    };
        }

        /// <summary>
        /// 黄豆表情
        /// </summary>
        public JObject Emoji {  get; set; }

        /// <summary>
        /// 各类大表情
        /// </summary>
        public BigStickerInfo BigSticker {  get; set; }

        public class BigStickerInfo
        {
            public string Url { get; set; }
            public int Height { get; set; }
            public int Width { get; set; }
        }

        /// <summary>
        /// 弹幕富文本卡片背景颜色
        /// </summary>
        public SolidColorBrush CardColor { get; set; } = new SolidColorBrush((Color)Application.Current.Resources["CardColor"]);

        /// <summary>
        /// 弹幕富文本卡片Padding
        /// </summary>
        public Thickness CardPadding { get; set; } = new Thickness(10, 8, 10, 8);

        /// <summary>
        /// 弹幕富文本卡片水平对齐方式
        /// </summary>
        public HorizontalAlignment CardHorizontalAlignment { get; set; } = HorizontalAlignment.Left;

        /// <summary>
        /// 是否显示用户头像
        /// </summary>
        public Visibility ShowUserFace { get; set; } = Visibility.Visible;

        /// <summary>
        /// 是否显示用户名
        /// </summary>
        public Visibility ShowUserName { get; set; } = Visibility.Visible;

        /// <summary>
        /// 是否显示富文本(用于用户发出大表情时)
        /// </summary>
        public Visibility ShowRichText { get; set; } = Visibility.Visible;

        /// <summary>
        /// 是否显示大表情
        /// </summary>
        
        public Visibility ShowBigSticker { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 是否显示房管
        /// </summary>
        public Visibility ShowAdmin { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 是否显示舰长
        /// </summary>
        public Visibility ShowCaptain { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 是否显示勋章
        /// </summary>
        public Visibility ShowMedal { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 是否显示用户等级
        /// </summary>
        public Visibility ShowTitle { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// 是否显示用户等级
        /// </summary>
        public Visibility ShowUserLevel { get; set; } = Visibility.Collapsed;
    }
}