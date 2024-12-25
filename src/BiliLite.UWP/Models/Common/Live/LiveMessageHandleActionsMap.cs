using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;
using Newtonsoft.Json;
using BiliLite.Extensions;
using BiliLite.Services;
using Windows.UI.Xaml.Media;
using Color = Windows.UI.Color;
using System.Text.RegularExpressions;

namespace BiliLite.Models.Common.Live
{
    public class LiveMessageHandleActionsMap
    {
        public LiveMessageHandleActionsMap()
        {
            Map = new Dictionary<MessageType, Action<LiveRoomViewModel, object>>
                {
                    { MessageType.ConnectSuccess, ConnectSuccess },
                    //{ MessageType.Online, Online },
                    { MessageType.Danmu, Danmu },
                    { MessageType.Gift, Gift },
                    { MessageType.InteractWord, InteractWord },
                    //{ MessageType.SystemMsg, SystemMsg },
                    { MessageType.SuperChat, SuperChat },
                    { MessageType.SuperChatJpn, SuperChat },
                    { MessageType.AnchorLotteryStart, AnchorLotteryStart },
                    { MessageType.AnchorLotteryAward, AnchorLotteryAward },
                    { MessageType.GuardBuy, GuardBuy },
                    { MessageType.GuardBuyNew, GuardBuyNew},
                    { MessageType.RoomChange, RoomChange },
                    { MessageType.RoomBlock, RoomBlock },
                    { MessageType.WaringOrCutOff, WaringOrCutOff },
                    { MessageType.StartLive, StartLive },
                    { MessageType.WatchedChange, WatchedChange },
                    { MessageType.RedPocketLotteryStart, RedPocketLotteryStart },
                    { MessageType.RedPocketLotteryWinner, RedPocketLotteryWinner },
                    { MessageType.OnlineRankChange, OnlineRankChange },
                    { MessageType.StopLive, StopLive },
                    { MessageType.ChatLevelMute, ChatLevelMute },
                    { MessageType.OnlineCountChange, OnlineCountChange }
                };
        }

        public Dictionary<MessageType, Action<LiveRoomViewModel, object>> Map;

        public event EventHandler<DanmuMsgModel> AddNewDanmu;

        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> AnchorLotteryEnd;

        public event EventHandler<LiveRoomEndRedPocketLotteryInfoModel> RedPocketLotteryEnd;

        public event EventHandler<LiveAnchorInfoLiveInfoModel> AnchorInfoLiveInfo;

        public event EventHandler<string> AddShieldWord;

        public event EventHandler<string> DelShieldWord;

        private void ConnectSuccess(LiveRoomViewModel viewModel, object message)
        {
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                CardPadding = new Thickness(6, 4, 6, 4),
                RichText = ("直播间 " + viewModel.RoomID.ToString() + " " + message.ToString()).ToRichTextBlock(null, fontWeight: "Medium"),
                CardHorizontalAlignment = HorizontalAlignment.Center,
            });
        }

        private void WatchedChange(LiveRoomViewModel viewModel, object message)
        {
            viewModel.ViewerNumCount = (string)message;
        }

        private void OnlineCountChange(LiveRoomViewModel viewModel, object message)
        {
            viewModel.ViewerNumCount = (string)message + "人在看";
        }

        private void Danmu(LiveRoomViewModel viewModel, object message)
        {
            var m = message as DanmuMsgModel;

            // 自己的消息和主播的消息特殊处理
            if (m.Uid == SettingService.Account.UserID.ToString() || m.Uid == viewModel.AnchorUid.ToString())
            {
                // 暂时借用了直播间标题修改的颜色... 找不到好看的颜色了
                m.CardColor = new SolidColorBrush(Color.FromArgb(255, 228, 255, 233));
                m.RichText = m.Text.ToRichTextBlock(m.Emoji, true, fontWeight: "Medium", fontColor: "#ff1e653a");
                if (m.Uid == viewModel.AnchorUid.ToString())
                {
                    m.Role = "主播";
                    m.ShowAdmin = Visibility.Visible;
                }
            }

            viewModel.CheckClearMessages();
            viewModel.Messages.Add(m);
            AddNewDanmu?.Invoke(null, m);
        }

        private void Gift(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveGiftMsg)
            {
                return;
            }
            if (viewModel.GiftMessage.Count >= 2)
            {
                viewModel.GiftMessage.RemoveAt(0);
            }
            viewModel.ShowGiftMessage = true;
            viewModel.HideGiftFlag = 1;
            var info = message as GiftMsgModel;
            info.Gif = viewModel.AllGifts.FirstOrDefault(x => x.Id == info.GiftId)?.Gif ?? Constants.App.TRANSPARENT_IMAGE;
            viewModel.GiftMessage.Add(info);
            if (!viewModel.TimerAutoHideGift.Enabled)
            {
                viewModel.TimerAutoHideGift.Start();
            }
        }

        private void InteractWord(LiveRoomViewModel viewModel, object message)
        {
            var info = message as InteractWordModel;
            if (!viewModel.ReceiveWelcomeMsg) return;
            var msg = new DanmuMsgModel()
            {
                ShowUserName = Visibility.Visible,
                ShowUserFace = Visibility.Collapsed,
                ShowRichText = Visibility.Collapsed,
                UserName = info.UserName + " 进入直播间",
            };

            if (info.ShowMedal == Visibility.Visible)
            {
                msg.MedalColor = info.MedalColor;
                msg.MedalName = info.MedalName;
                msg.MedalLevel = info.MedalLevel;
                msg.ShowMedal = info.ShowMedal;
            }

            viewModel.CheckClearMessages();
            viewModel.Messages.Add(msg);
        }

        // 已被b站弃用
        //private void WelcomeGuard(LiveRoomViewModel viewModel, object message)
        //{
        //    var info = message as InteractWordModel;
        //    if (!viewModel.ReceiveWelcomeMsg) return;
        //    viewModel.Messages.Add(new DanmuMsgModel()
        //    {
        //        UserName = info.UserName,
        //        UserNameColor = "#FFFF69B4",//Colors.HotPink
        //        RichText = " (舰长)进入直播间".ToRichTextBlock(null)
        //    });
        //}

        //private void SystemMsg(LiveRoomViewModel viewModel, object message)
        //{
        //}

        private void SuperChat(LiveRoomViewModel viewModel, object message)
        {
            viewModel.SuperChats.Insert(0, message as SuperChatMsgViewModel); // 新sc总会显示在最前

            // SuperChat也是Chat :)
            var info = message as SuperChatMsgViewModel;
            var msg = new DanmuMsgModel
            {
                Text = info.Message,
                DanmuColor = info.FontColor,
                RichText = info.Message.ToRichTextBlock(null, true, fontWeight: "Medium", fontColor: "White"),
                CardColor = new SolidColorBrush(info.BackgroundBottomColor.StrToColor()),
                Face = info.Face,
                UserName = info.Username,
                UserNameFontWeight = "Semibold",
                Uid = info.Uid.ToString(),
                UserCaptain = info.GuardLevel,
                ShowCaptain = Visibility.Visible,
                MedalColor = info.MedalColor,
                MedalName = info.MedalName,
                MedalLevel = info.MedalLevel,
                ShowMedal = (!string.IsNullOrEmpty(info.MedalColor) &&
                             !string.IsNullOrEmpty(info.MedalName) &&
                             info.MedalLevel > 0) ? Visibility.Visible : Visibility.Collapsed,
            };

            Danmu(viewModel, msg);
        }

        private void AnchorLotteryStart(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(message.ToString());
            viewModel.LotteryDanmu["AnchorLottery"] = info.Danmu;
            AddShieldWord?.Invoke(info, info.Danmu);
            viewModel.LotteryViewModel.SetAnchorLotteryInfo(info);
        }

        private void RedPocketLotteryStart(LiveRoomViewModel viewModel, object message) 
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomRedPocketLotteryInfoModel>(message.ToString());
            viewModel.LotteryDanmu["RedPocketLottery"] = info.Danmu;
            AddShieldWord?.Invoke(info, info.Danmu);
            viewModel.LotteryViewModel.SetRedPocketLotteryInfo(info);

            viewModel.ShowRedPocketLotteryWinnerList = false;
            viewModel.RedPocketSendDanmuBtnText = viewModel.Attention ? "一键发送弹幕" : "一键关注并发送弹幕";
        }

        private void RedPocketLotteryWinner(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomEndRedPocketLotteryInfoModel>(message.ToString());
            RedPocketLotteryEnd?.Invoke(this, info);
            DelShieldWord?.Invoke(info, viewModel.LotteryDanmu["RedPocketLottery"]);
        }

        private void AnchorLotteryAward(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomEndAnchorLotteryInfoModel>(message.ToString());
            AnchorLotteryEnd?.Invoke(this, info);
            DelShieldWord?.Invoke(info, viewModel.LotteryDanmu["AnchorLottery"]);
        }

        private void GuardBuy(LiveRoomViewModel viewModel, object message)
        {
            var info = message as GuardBuyMsgModel;
            var msg = new DanmuMsgModel
            {
                ShowUserName = Visibility.Collapsed,
                ShowUserFace = Visibility.Collapsed,
                RichText = (info.UserName + $" 成为了主播的{info.GiftName}🎉").ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: info.FontColor),
                CardColor = new SolidColorBrush(info.CardColor),
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };
            viewModel.Messages.Add(msg);
            // 刷新舰队列表
            viewModel.ReloadGuardList().RunWithoutAwait();
        }

        private void GuardBuyNew(LiveRoomViewModel viewModel, object message)
        {
            var info = message as GuardBuyMsgModel;

            var isNewGuard = info.Message.Contains("开通");

            int accompanyDays = -1;
            var match = Regex.Match(info.Message, @"今天是TA陪伴主播的第(\d+)天");
            if (match.Success) accompanyDays = match.Groups[1].Value.ToInt32();

            var text = info.UserName + 
                       (isNewGuard ? "\n开通了" : "\n续费了") +
                       $"主播的{info.GiftName}" + 
                       (info.Num > 1 ? $"×{info.Num}个{info.Unit}" : "") +
                       "🎉" +
                       ((match.Success && accompanyDays > 1) ? $"\nTA已陪伴主播{accompanyDays}天💖" : "");

            var msg = new DanmuMsgModel
            {
                ShowUserName = Visibility.Collapsed,
                ShowUserFace = Visibility.Collapsed,
                RichText = text.ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: info.FontColor, textAlignment: "Center"),
                CardColor = new SolidColorBrush(info.CardColor),
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };
            
            viewModel.Messages.Add(msg);
            if (isNewGuard) viewModel.ReloadGuardList().RunWithoutAwait();

            if (info.UserID == SettingService.Account.UserID.ToString()) viewModel.GetEmoticons().RunWithoutAwait(); // 自己开通了舰长, 有些表情即可解锁
        }

        private void RoomChange(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomChangeMsgModel;
            var msg = new DanmuMsgModel
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = ($"直播间标题已修改:\n{viewModel.RoomTitle}\n🔽\n{info.Title}").ToRichTextBlock(null, fontWeight: "SemiBold", textAlignment:"Center", fontColor: "#ff1e653a"), //一种绿色
                CardColor = new SolidColorBrush(Color.FromArgb(255, 228, 255, 233)),
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };
            viewModel.Messages.Add(msg);
            viewModel.RoomTitle = info.Title;
        }

        private void RoomBlock(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomBlockMsgModel;
            var msg = new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = (info.UserName + " 被直播间禁言🚫").ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: "White", textAlignment: "Center"), 
                CardColor = new SolidColorBrush(Color.FromArgb(255, 235, 45, 80)), // 一种红色
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };

            viewModel.Messages.Add(msg);

            var text = info.UserName + " 发言历史:\n";
            var previousChatList = viewModel.Messages
                                    .Where(item => item.Uid == info.UserID)                       // 筛选出符合条件的对象
                                    .OrderByDescending(item => viewModel.Messages.IndexOf(item))  // 根据索引倒序排序
                                    .Take(3)                                                      // 取前三个
                                    .Select(item => item.Text);                                   // 提取Text字段
            if (previousChatList.Count() == 0) return;

            text += string.Join("\n", previousChatList);
            msg = new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = (text).ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: "White", textAlignment: "Center"), 
                CardColor = new SolidColorBrush(Color.FromArgb(255, 235, 45, 80)), // 一种红色
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };
            viewModel.Messages.Add(msg);
        }

        private void WaringOrCutOff(LiveRoomViewModel viewModel, object message)
        {
            var info = message as WarningOrCutOffMsgModel;
            var text = info.Command switch
            {
                "WARNING" => "⚠️直播间警告",
                "CUT_OFF" => "⛔直播间切断",
                _ => null,
            };
            var cardColor = info.Command switch
            {
                "WARNING" => new SolidColorBrush(Color.FromArgb(255, 235, 156, 0)), // 一种橙黄色
                "CUT_OFF" => new SolidColorBrush(Color.FromArgb(255, 210, 20, 54)), // 一种深红色
                _ => null,
            };
            var msg = new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = (text + "\n" + info.Message).ToRichTextBlock(null, fontColor: "White", fontWeight: "SemiBold", textAlignment: "Center"), 
                CardColor = cardColor,
                CardHorizontalAlignment = HorizontalAlignment.Center,
            };

            viewModel.Messages.Add(msg);
        }

        private void StartLive(LiveRoomViewModel viewModel, object room_Id)
        {
            viewModel.GetPlayUrls(room_Id.ToInt32(), SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000)).RunWithoutAwait();
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = $"直播间 {room_Id} 开始直播".ToRichTextBlock(null, fontWeight: "Medium"),
                CardHorizontalAlignment = HorizontalAlignment.Center,
                CardPadding = new Thickness(6, 4, 6, 4),
            });
        }

        private void OnlineRankChange(LiveRoomViewModel viewModel, object message)
        {
            viewModel.Ranks.Where(rank => rank.RankType == "contribution-rank").ToList()?[0]?.ReloadData().RunWithoutAwait();
        }

        private void StopLive(LiveRoomViewModel viewModel, object message)
        {
            //viewModel.GetPlayUrls(viewModel.RoomID.ToInt32(), SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000)).RunWithoutAwait();
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = $"直播间 {viewModel.RoomID} 停止直播".ToRichTextBlock(null, fontWeight: "Medium"),
                CardHorizontalAlignment = HorizontalAlignment.Center,
                CardPadding = new Thickness(6, 4, 6, 4),
            });
        }
        
        private void ChatLevelMute(LiveRoomViewModel viewModel, object level)
        {
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = ((int)level > 0 ? $"直播间开启了等级{level}禁言🤐" : "直播间关闭了等级禁言🗣️").ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: "White"),
                CardHorizontalAlignment = HorizontalAlignment.Center,
                CardColor = new SolidColorBrush(Color.FromArgb(255, 235, 45, 80)), // 一种红色
            });
        }
    }
}
