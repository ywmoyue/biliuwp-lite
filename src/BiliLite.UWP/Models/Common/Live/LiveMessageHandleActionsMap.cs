using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;
using Newtonsoft.Json;
using BiliLite.Extensions;
using BiliLite.Services;

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
                    { MessageType.RedPocketLotteryStart, RedPocketLotteryStart},
                    { MessageType.RedPocketLotteryWinner, RedPocketLotteryWinner},
                    { MessageType.OnlineRankChange, OnlineRankChange},
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
                UserName = message.ToString(),
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
                UserName = info.UserName,
                // UserNameColor = "#FFFF69B4",//Colors.HotPink
                RichText = info.MsgType == 1 ? "进入直播间".ToRichTextBlock(null, color: "Gray") : "关注了主播".ToRichTextBlock(null, color: "Gray")
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
                UserName = info.UserName,
                UserNameColor = "#FFFF69B4",//Colors.HotPink
                RichText = $"成为了主播的{info.GiftName}🎉".ToRichTextBlock(null, fontWeight: "Medium"),
                UserCaptain = info.GiftName,
                ShowCaptain = Visibility.Visible,
                UserNameFontWeight = "SemiBold", // 字重调大, 防止与进场弹幕混淆
            };

            viewModel.Messages.Add(msg);
            // 刷新舰队列表
            _ = viewModel.GetGuardList();
        }

        private void RoomChange(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomChangeMsgModel;
            viewModel.RoomTitle = info.Title;
        }

        private void RoomBlock(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomBlockMsgModel;
            var msg = new DanmuMsgModel()
            {
                UserName = info.UserName,
                RichText = "被直播间禁言🚫".ToRichTextBlock(null, fontWeight: "Medium"), // 字重调大, 防止与进场弹幕混淆)
                UserNameFontWeight = "SemiBold",
            };

            viewModel.Messages.Add(msg);
        }

        private void WaringOrCutOff(LiveRoomViewModel viewModel, object message)
        {
            var info = message as WarningOrCutOffMsgModel;
            var msg = new DanmuMsgModel()
            {
                UserName = info.Command switch
                {
                    "WARNING" => "⚠️直播间警告",
                    "CUT_OFF" => "⛔直播间切断",
                    _ => null,
                },
                UserNameColor = "FFFF0000",
                RichText = info.Message.ToRichTextBlock(null, color: "Red", fontWeight: "Medium"), // 字重调大, 防止与进场弹幕混淆
                UserNameFontWeight = "SemiBold",
            };

            viewModel.Messages.Add(msg);
        }

        private async void StartLive(LiveRoomViewModel viewModel, object room_Id)
        {
            await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(5)); // 挂起五秒再获取, 否则很大可能一直卡加载而不缓冲
            viewModel.GetPlayUrls(room_Id.ToInt32(), SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000)).RunWithoutAwait();
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                UserName = $"{room_Id} 直播间开始直播",
            });
        }

        private void OnlineRankChange(LiveRoomViewModel viewModel, object message)
        {
            viewModel.Ranks.Where(rank => rank.Title == "高能用户贡献榜").ToList()?[0]?.ReloadData().RunWithoutAwait();
        }
    }
}
