using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using BiliLite.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomHistoryDanmu
    {
        [JsonProperty("admin")]
        public ObservableCollection<JObject> Admin;

        [JsonProperty("room")]
        public ObservableCollection<JObject> Room;

        private List<JObject> DanmuList => Room.Concat(Admin).ToList();

        public List<DanmuMsgModel> GetHistoryDanmuList()
        {
            var list = new List<DanmuMsgModel>();
            foreach (var danmu in DanmuList)
            {
                // 基本信息
                var msg = new DanmuMsgModel
                {
                    Text = danmu["text"]?.ToString(),
                    UserName = danmu["nickname"]?.ToString(),
                    Uid = danmu["user"]?["uid"]?.ToString(),
                    Face = danmu["user"]?["base"]?["face"]?.ToString(),
                    UserNameFontWeight = "SemiBold",
                };

                // 弹幕颜色, 但是没找到用int表示的颜色值, 暂时不处理

                // 发送时间, 用于排序
                msg.Timestamp = (int)danmu["check_info"]?["ts"];

                // 黄豆表情
                if (danmu["emots"]?.ToArray().Length != 0) msg.Emoji = (JObject)danmu["emots"];

                // 大表情
                if (danmu["emoticon"]?["url"]?.ToString().Length != 0)
                {
                    msg.ShowBigSticker = Visibility.Visible;
                    msg.BigSticker = new DanmuMsgModel.BigStickerInfo
                    {
                        Url = danmu["emoticon"]?["url"]?.ToString(),
                        Height = (int)danmu["emoticon"]?["height"],
                        Width = (int)danmu["emoticon"]?["width"],
                    };

                    //有的表情特别大 :(
                    msg.BigSticker.Height = (msg.BigSticker.Height * 60 / msg.BigSticker.Width).ToInt32();
                    msg.BigSticker.Width = 60;
                }

                // 舰长房管相关
                msg.ShowAdmin = danmu["isadmin"].ToInt32() == 1 ? Visibility.Visible : Visibility.Collapsed;
                msg.Role = danmu["isadmin"].ToInt32() == 1 ? "房管" : ""; 
                msg.UserCaptain = (UserCaptainType)danmu["guard_level"].ToInt32();
                msg.ShowCaptain = msg.UserCaptain > 0 ? Visibility.Visible : Visibility.Collapsed;

                // 粉丝牌
                if (danmu["medal"] != null && danmu["medal"].ToArray().Length != 0)
                {
                    msg.MedalName = danmu["medal"][1]?.ToString();
                    msg.MedalLevel = danmu["medal"][0].ToInt32();
                    msg.MedalColor = danmu["medal"][4]?.ToString();
                    msg.ShowMedal = Visibility.Visible;
                }

                // 构建富文本
                msg.RichText = msg.Text.ToRichTextBlock(msg.Emoji, true, fontWeight: "Medium");

                list.Add(msg);
            }
            return list.OrderBy(danmu => danmu.Timestamp).ToList();
        }
    }
}
    