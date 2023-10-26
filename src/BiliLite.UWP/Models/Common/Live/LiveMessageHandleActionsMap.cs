using System;
using System.Collections.Generic;
using System.Linq;
using BiliLite.Modules.Live;
using Windows.UI.Xaml;
using BiliLite.ViewModels.Live;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveMessageHandleActionsMap
    {
        public LiveMessageHandleActionsMap()
        {
            Map = new Dictionary<MessageType, Action<LiveRoomViewModel, object>>
                {
                    { MessageType.ConnectSuccess, ConnectSuccess },
                    { MessageType.Online, Online },
                    { MessageType.Danmu, Danmu },
                    { MessageType.Gift, Gift },
                    { MessageType.Welcome, Welcome },
                    { MessageType.WelcomeGuard, WelcomeGuard },
                    { MessageType.SystemMsg, SystemMsg },
                    { MessageType.SuperChat, SuperChat },
                    { MessageType.SuperChatJpn, SuperChat },
                    { MessageType.AnchorLotteryStart, AnchorLotteryStart },
                    { MessageType.AnchorLotteryEnd, AnchorLotteryEnd },
                    { MessageType.AnchorLotteryAward, AnchorLotteryAward },
                    { MessageType.GuardBuy, GuardBuy },
                    { MessageType.RoomChange, RoomChange },
                };
        }

        public Dictionary<MessageType, Action<LiveRoomViewModel, object>> Map;

        public event EventHandler<DanmuMsgModel> AddNewDanmu;

        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> LotteryEnd;

        private void ConnectSuccess(LiveRoomViewModel viewModel, object message)
        {
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                UserName = message.ToString(),
            });
        }

        private void Online(LiveRoomViewModel viewModel, object message)
        {
            viewModel.Online = (int)message;
        }

        private void Danmu(LiveRoomViewModel viewModel, object message)
        {
            var m = message as DanmuMsgModel;
            m.ShowUserLevel = Visibility.Visible;
            if (viewModel.Messages.Count >= viewModel.CleanCount)
            {
                viewModel.Messages.Clear();
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

        private void Welcome(LiveRoomViewModel viewModel, object message)
        {
            var info = message as WelcomeMsgModel;
            if (!viewModel.ReceiveWelcomeMsg) return;
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                UserName = info.UserName,
                UserNameColor = "#FFFF69B4",//Colors.HotPink
                Text = " 进入直播间"
            });
        }

        private void WelcomeGuard(LiveRoomViewModel viewModel, object message)
        {
            var info = message as WelcomeMsgModel;
            if (!viewModel.ReceiveWelcomeMsg) return;
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                UserName = info.UserName,
                UserNameColor = "#FFFF69B4",//Colors.HotPink
                Text = " (舰长)进入直播间"
            });
        }

        private void SystemMsg(LiveRoomViewModel viewModel, object message)
        {
        }

        private void SuperChat(LiveRoomViewModel viewModel, object message)
        {
            viewModel.SuperChats.Add(message as SuperChatMsgViewModel);
        }

        private void AnchorLotteryStart(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = message.ToString();
            viewModel.AnchorLotteryViewModel.SetLotteryInfo(JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(info));
        }

        private void AnchorLotteryEnd(LiveRoomViewModel viewModel, object message)
        {
        }

        private void AnchorLotteryAward(LiveRoomViewModel viewModel, object message)
        {
            if (!viewModel.ReceiveLotteryMsg) return;
            var info = JsonConvert.DeserializeObject<LiveRoomEndAnchorLotteryInfoModel>(message.ToString());
            LotteryEnd?.Invoke(this, info);
        }

        private void GuardBuy(LiveRoomViewModel viewModel, object message)
        {
            var info = message as GuardBuyMsgModel;
            viewModel.Messages.Add(new DanmuMsgModel()
            {
                UserName = info.UserName,
                UserNameColor = "#FFFF69B4",//Colors.HotPink
                Text = $"成为了{info.GiftName}"
            });
            // 刷新舰队列表
            _ = viewModel.GetGuardList();
        }

        private void RoomChange(LiveRoomViewModel viewModel, object message)
        {
            var info = message as RoomChangeMsgModel;
            viewModel.RoomTitle = info.Title;
        }
    }
}
