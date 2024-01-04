using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;
using Newtonsoft.Json;
using BiliLite.Extensions;
using BiliLite.Services;
using Windows.UI;
using Windows.UI.Xaml.Media;

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
                    { MessageType.RoomChange, RoomChange },
                    { MessageType.RoomBlock, RoomBlock },
                    { MessageType.WaringOrCutOff, WaringOrCutOff },
                    { MessageType.StartLive, StartLive },
                    { MessageType.WatchedChange, WatchedChange },
                    { MessageType.RedPocketLotteryStart, RedPocketLotteryStart },
                    { MessageType.RedPocketLotteryWinner, RedPocketLotteryWinner },
                    { MessageType.OnlineRankChange, OnlineRankChange },
                    { MessageType.StopLive, StopLive },
                    { MessageType.RoomSlient, RoomSlient },
                };
        }

        public Dictionary<MessageType, Action<LiveRoomViewModel, object>> Map;

        public event EventHandler<DanmuMsgModel> AddNewDanmu;

        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> AnchorLotteryEnd;

        public event EventHandler<LiveRoomEndRedPocketLotteryInfoModel> RedPocketLotteryEnd;

        public event EventHandler<LiveAnchorInfoLiveInfoModel> AnchorInfoLiveInfo;

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
            viewModel.WatchedNum = (string)message;
        }

        private void Danmu(LiveRoomViewModel viewModel, object message)
        {
            var m = message as DanmuMsgModel;
            if (viewModel.Messages.Count >= viewModel.CleanCount)
            {
                viewModel.Messages.RemoveAt(0);
            }
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
            viewModel.SuperChats.Add(message as SuperChatMsgViewModel);
        }

        private void AnchorLotteryStart(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = message.ToString();
            viewModel.LotteryViewModel.SetAnchorLotteryInfo(JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(info));
        }

        private void RedPocketLotteryStart(LiveRoomViewModel viewModel, object message) 
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = message.ToString();
            viewModel.LotteryViewModel.SetRedPocketLotteryInfo(JsonConvert.DeserializeObject<LiveRoomRedPocketLotteryInfoModel>(info));

            viewModel.ShowRedPocketLotteryWinnerList = false;
            viewModel.RedPocketSendDanmuBtnText = viewModel.Attention ? "一键发送弹幕" : "一键关注并发送弹幕";
        }

        private void RedPocketLotteryWinner(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomEndRedPocketLotteryInfoModel>(message.ToString());
            RedPocketLotteryEnd?.Invoke(this, info);
        }

        private void AnchorLotteryAward(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomEndAnchorLotteryInfoModel>(message.ToString());
            AnchorLotteryEnd?.Invoke(this, info);
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
            _ = viewModel.GetGuardList();
        }

        private void RoomChange(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomChangeMsgModel;
            var msg = new DanmuMsgModel
            {
                ShowUserFace = Visibility.Collapsed,
                ShowUserName = Visibility.Collapsed,
                RichText = ($"直播间标题已修改:\n{viewModel.RoomTitle} ➡️ {info.Title}").ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: "#ff1e653a"), //一种绿色
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
                RichText = (info.UserName + " 被直播间禁言🚫").ToRichTextBlock(null, fontWeight: "SemiBold", fontColor: "White"), // 白色
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
                RichText = (text + "\n" + info.Message).ToRichTextBlock(null, fontColor: "White", fontWeight: "SemiBold"), 
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
        
        private void RoomSlient(LiveRoomViewModel viewModel, object level)
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
