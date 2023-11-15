using System.Linq;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;

namespace BiliLite.Models.Common.Live
{
    public class DanmuMsgModel
    {
        /// <summary>
        /// 内容文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 弹幕颜色，默认白色
        /// </summary>
        public string DanmuColor { get; set; } = "#FFFFFFFF";

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户名颜色,默认灰色
        /// </summary>
        public string UserNameColor { get; set; } = "#FF808080";

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
        /// 用户上舰名称
        /// </summary>
        public string UserCaptain {  get; set; }

        public string UserCaptainImage
        {
            get
            {
                if (UserCaptain == null)
                {
                    return null;
                }

                return UserCaptain switch
                {
                    "舰长" => "/Assets/Live/ic_live_guard_3.png",
                    "提督" => "/Assets/Live/ic_live_guard_2.png",
                    "总督" => "/Assets/Live/ic_live_guard_1.png",
                    _ => null,
                };
            }
        }

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