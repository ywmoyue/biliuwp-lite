using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Extensions;
using System.IO.Compression;
using BiliLite.Models.Common.Live;
using BiliLite.ViewModels.Live;

/*
* 参考文档:
* https://github.com/lovelyyoshino/Bilibili-Live-API/blob/master/API.WebSocket.md
* 
*/

namespace BiliLite.Modules.Live
{
    public class LiveMessage : IDisposable
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        public delegate void MessageHandler(MessageType type, object message);
        public event MessageHandler NewMessage;
        ClientWebSocket ws;

        public LiveMessage()
        {
            ws = new ClientWebSocket();
        }
        private static System.Timers.Timer heartBeatTimer;
        public async Task Connect(int roomID, int uid, string token, string buvid, string host, CancellationToken cancellationToken)
        {
            ws.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36 Edg/116.0.1938.69");
            //连接
            await ws.ConnectAsync(new Uri("wss://" + host + "/sub"), cancellationToken);
            //进房
            await JoinRoomAsync(roomID, buvid, token, uid);
            //发送心跳
            await SendHeartBeatAsync();
            heartBeatTimer = new System.Timers.Timer(1000 * 30);
            heartBeatTimer.Elapsed += HeartBeatTimer_Elapsed;
            heartBeatTimer.Start();
            while (!cancellationToken.IsCancellationRequested && ws.State == WebSocketState.Open)
            {
                try
                {
                    WebSocketReceiveResult result;
                    using var ms = new MemoryStream();
                    var buffer = new byte[4096];
                    do
                    {
                        result = await ws.ReceiveAsync(buffer, cancellationToken);
                        ms.Write(buffer, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    ms.Seek(0, SeekOrigin.Begin);
                    var receivedData = new byte[ms.Length];
                    ms.Read(receivedData, 0, receivedData.Length);

                    ParseData(receivedData);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        return;
                    }
                    logger.Log("直播接收包出错", LogType.Error, ex);
                }
            }
        }

        private async void HeartBeatTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            await SendHeartBeatAsync();
        }
        /// <summary>
        /// 发送进房信息
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        private async Task JoinRoomAsync(int roomId, string buvid, string token, int uid = 0)
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(EncodeData(JsonConvert.SerializeObject(new
                {
                    roomid = roomId,
                    uid = uid,
                    buvid = buvid,
                    key = token,
                    protover = 2,
                    //暂时不要加上platform，否则未登录时会隐藏用户名
                    //platform = "web"
                }), 7), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }
        /// <summary>
        /// 发送心跳包
        /// </summary>
        /// <returns></returns>
        private async Task SendHeartBeatAsync()
        {
            if (ws.State == WebSocketState.Open)
            {
                await ws.SendAsync(EncodeData("", 2), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
        }

        /// <summary>
        /// 解析内容
        /// </summary>
        /// <param name="data"></param>
        private async void ParseData(byte[] data)
        {
            //协议版本。
            //0为JSON，可以直接解析；
            //1为房间人气值,Body为Int32；
            //2为zlib压缩过Buffer，需要解压再处理
            //3为brotli压缩过Buffer，需要解压再处理
            int protocolVersion = BitConverter.ToInt32(new byte[4] { data[7], data[6], 0, 0 }, 0);
            //操作类型。
            //3=心跳回应，内容为房间人气值；
            //5=通知，弹幕、广播等全部信息；
            //8=进房回应，空
            int operation = BitConverter.ToInt32(data.Skip(8).Take(4).Reverse().ToArray(), 0);
            //内容
            var body = data.Skip(16).ToArray();
            if (operation == 8)
            {
                NewMessage?.Invoke(MessageType.ConnectSuccess, "弹幕连接成功");
            }
            else if (operation == 5)
            {
                body = DecompressData(protocolVersion, body);

                var text = Encoding.UTF8.GetString(body);
                //可能有多条数据，做个分割
                var textLines = Regex.Split(text, "[\x00-\x1f]+").Where(x => x.Length > 2 && x[0] == '{').ToArray();

                var delay = textLines.Length > 30 ? 0.1 : 0.05; // 大概3秒来一波
                delay = textLines.Length > 60 ? 0.2 : delay;
                foreach (var item in textLines)
                {
                    ParseMessage(item);
                    await Task.Delay(TimeSpan.FromSeconds(delay));
                }
            }
        }

        private void ParseMessage(string jsonMessage)
        {
            try
            {
                var obj = JObject.Parse(jsonMessage);
                var cmd = obj["cmd"].ToString();
                if (cmd == ("DANMU_MSG"))
                {
                    var msg = new DanmuMsgModel();
                    if (obj["info"] != null && obj["info"].ToArray().Length != 0)
                    {
                        // 弹幕内黄豆表情详情
                        if (obj["info"][0][15]["extra"] != null)// && obj["info"][0][15]["extra"].ToArray().Length != 0)
                        {
                            var extra = JObject.Parse(obj["info"][0][15]["extra"].ToString());
                            if (extra["emots"].ToArray().Length != 0)
                            {
                                msg.Emoji = (JObject)extra["emots"];
                            }
                        }

                        // 弹幕内大表情详情
                        if (obj["info"][0][13] != null && obj["info"][0][13].ToArray().Length != 0)
                        {
                            // 如果有大表情, 直接不需要显示任何文字
                            msg.ShowRichText = Visibility.Collapsed;
                            msg.BigSticker = new DanmuMsgModel.BigStickerInfo
                            {
                                Url = (string)obj["info"][0][13]["url"],
                                Height = (int)obj["info"][0][13]["height"],
                                Width = (int)obj["info"][0][13]["width"],
                            };
                            //有的表情特别大 :(
                            msg.BigSticker.Height = (msg.BigSticker.Height * 60 / msg.BigSticker.Width).ToInt32();
                            msg.BigSticker.Width = 60;
                        }

                        // 弹幕内容
                        msg.Text = obj["info"][1].ToString();
                        // 字重调大, 防止与进场弹幕混淆
                        msg.UserNameFontWeight = "SemiBold";
                        msg.RichText = msg.Text.ToRichTextBlock(msg.Emoji, true, fontWeight: "Medium");

                        // 弹幕颜色
                        var color = obj["info"][0][3].ToInt32();
                        if (color != 0)
                        {
                            msg.DanmuColor = color.ToString();
                        }

                        // 是否为房管
                        if (obj["info"][2] != null && obj["info"][2].ToArray().Length != 0)
                        {
                            msg.UserName = obj["info"][2][1].ToString() + ":";
                            if (obj["info"][2][2] != null && Convert.ToInt32(obj["info"][2][2].ToString()) == 1)
                            {
                                msg.Role = "房管";
                                msg.ShowAdmin = Visibility.Visible;
                            }
                        }

                        // 是否为舰长
                        if (obj["info"][3][10] != null && Convert.ToInt32(obj["info"][3][10].ToString()) != 0)
                        {
                            switch (Convert.ToInt32(obj["info"][3][10].ToString()))
                            {
                                case 3:
                                    msg.UserCaptain = "舰长";
                                    msg.UserNameColor = "#FF23709E";
                                    break;
                                case 2:
                                    msg.UserCaptain = "提督";
                                    msg.UserNameColor = "#FF7B166F";
                                    break;
                                case 1:
                                    msg.UserCaptain = "总督";
                                    msg.UserNameColor = "#FFC01039";
                                    break;
                            }
                            msg.ShowCaptain = Visibility.Visible;
                        }

                        // 粉丝牌
                        if (obj["info"][3] != null && obj["info"][3].ToArray().Length != 0)
                        {
                            msg.MedalName = obj["info"][3][1].ToString();
                            msg.MedalLevel = obj["info"][3][0].ToString();
                            msg.MedalColor = obj["info"][3][4].ToString();
                            msg.ShowMedal = Visibility.Visible;
                        }
# if DEBUG
                        // 手动触发测试
                        if (msg.Text == "test123")
                        {
                            //NewMessage?.Invoke(MessageType.RoomBlock, new RoomBlockMsgModel()
                            //{
                            //    UserID = "123",
                            //    UserName = "TestName",
                            //});

                            //NewMessage?.Invoke(MessageType.GuardBuy, new GuardBuyMsgModel()
                            //{
                            //    GiftName = "总督",
                            //    UserName = "TestName",
                            //    UserID = "123",
                            //    GuardLevel = 30,
                            //});

                            //NewMessage?.Invoke(MessageType.WaringOrCutOff, new WarningOrCutOffMsgModel()
                            //{
                            //    Message = "图片内容不适宜，请立即调整",
                            //    Command = "WARNING",
                            //});

                            //NewMessage?.Invoke(MessageType.WaringOrCutOff, new WarningOrCutOffMsgModel()
                            //{
                            //    Message = "违反直播言论规范，请立即调整",
                            //    Command = "CUT_OFF",
                            //});

                            //NewMessage?.Invoke(MessageType.RoomChange, new RoomChangeMsgModel()
                            //{
                            //    Title = "TestLiveRoomName",
                            //});

                            //NewMessage?.Invoke(MessageType.StartLive, "6");

                            //var nowTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                            //var test = "\r\n  {\r\n    \"lot_id\": 15759014,\r\n    \"sender_uid\": 1485563917,\r\n    \"sender_name\": \"fan_wyy\",\r\n    \"sender_face\": \"https://i1.hdslb.com/bfs/face/a46db0a66f1fc2e1756a578beeef05db004694d1.jpg\",\r\n    \"join_requirement\": 1,\r\n    \"danmu\": \"老板大气！点点红包抽礼物\",\r\n    \"awards\": [\r\n      {\r\n        \"gift_id\": 31212,\r\n        \"num\": 2,\r\n        \"gift_name\": \"打call\",\r\n        \"gift_pic\": \"https://s1.hdslb.com/bfs/live/461be640f60788c1d159ec8d6c5d5cf1ef3d1830.png\"\r\n      },\r\n      {\r\n        \"gift_id\": 31214,\r\n        \"num\": 3,\r\n        \"gift_name\": \"牛哇\",\r\n        \"gift_pic\": \"https://s1.hdslb.com/bfs/live/91ac8e35dd93a7196325f1e2052356e71d135afb.png\"\r\n      },\r\n      {\r\n        \"gift_id\": 31216,\r\n        \"num\": 3,\r\n        \"gift_name\": \"小花花\",\r\n        \"gift_pic\": \"https://s1.hdslb.com/bfs/live/5126973892625f3a43a8290be6b625b5e54261a5.png\"\r\n      }\r\n    ],\r\n    \"start_time\": 1700742404,\r\n    \"end_time\": 1700742584,\r\n    \"last_time\": 180,\r\n    \"remove_time\": 1700742599,\r\n    \"replace_time\": 1700742594,\r\n    \"current_time\": 1700742591,\r\n    \"lot_status\": 2,\r\n    \"h5_url\": \"https://live.bilibili.com/p/html/live-app-red-envelope/popularity.html?is_live_half_webview=1&hybrid_half_ui=1,5,100p,100p,000000,0,50,0,0,1;2,5,100p,100p,000000,0,50,0,0,1;3,5,100p,100p,000000,0,50,0,0,1;4,5,100p,100p,000000,0,50,0,0,1;5,5,100p,100p,000000,0,50,0,0,1;6,5,100p,100p,000000,0,50,0,0,1;7,5,100p,100p,000000,0,50,0,0,1;8,5,100p,100p,000000,0,50,0,0,1&hybrid_rotate_d=1&hybrid_biz=popularityRedPacket&lotteryId=15759014\",\r\n    \"user_status\": 2,\r\n    \"lot_config_id\": 3,\r\n    \"total_price\": 1600,\r\n    \"wait_num\": 12\r\n  }\r\n";
                            //var CurrentTime = nowTime;
                            //var StartTime = nowTime - 1;
                            //var EndTime = nowTime + 10;
                            //var RemoveTime = EndTime + 10;
                            //test = test.Replace("1700742591", nowTime.ToString());
                            //test = test.Replace("1700742404", StartTime.ToString());
                            //test = test.Replace(("1700742404".ToInt32() + 180).ToString(), EndTime.ToString());
                            //test = test.Replace(("1700742404".ToInt32() + 180 + 15).ToString(), RemoveTime.ToString());
                            //NewMessage?.Invoke(MessageType.RedPocketLotteryStart, test);

                            //var test = "{\r\n            \"id\": 5395495,\r\n            \"room_id\": 30760185,\r\n            \"status\": 1,\r\n            \"asset_icon\": \"https://i0.hdslb.com/bfs/live/627ee2d9e71c682810e7dc4400d5ae2713442c02.png\",\r\n            \"award_name\": \"情书\",\r\n            \"award_num\": 1,\r\n            \"award_image\": \"\",\r\n            \"danmu\": \"测\",\r\n            \"time\": 233,\r\n            \"current_time\": 1700804967,\r\n            \"join_type\": 0,\r\n            \"require_type\": 1,\r\n            \"require_value\": 0,\r\n            \"require_text\": \"关注主播\",\r\n            \"gift_id\": 0,\r\n            \"gift_name\": \"\",\r\n            \"gift_num\": 0,\r\n            \"gift_price\": 0,\r\n            \"cur_gift_num\": 0,\r\n            \"goaway_time\": 171,\r\n            \"award_users\": [\r\n                {\r\n                    \"uid\": 1277993759,\r\n                    \"uname\": \"皎皎梦丶\",\r\n                    \"face\": \"https://i2.hdslb.com/bfs/face/db9e3407a60e83850d9824cc8f1189c491639a3c.jpg\",\r\n                    \"level\": 27,\r\n                    \"color\": 5805790,\r\n                    \"bag_id\": 7671636,\r\n                    \"gift_id\": 31250,\r\n                    \"num\": 1\r\n                }\r\n            ],\r\n            \"show_panel\": 1,\r\n            \"url\": \"https://live.bilibili.com/p/html/live-lottery/anchor-join.html?is_live_half_webview=1\\u0026hybrid_biz=live-lottery-anchor\\u0026hybrid_half_ui=1,5,100p,100p,000000,0,30,0,0,1;2,5,100p,100p,000000,0,30,0,0,1;3,5,100p,100p,000000,0,30,0,0,1;4,5,100p,100p,000000,0,30,0,0,1;5,5,100p,100p,000000,0,30,0,0,1;6,5,100p,100p,000000,0,30,0,0,1;7,5,100p,100p,000000,0,30,0,0,1;8,5,100p,100p,000000,0,30,0,0,1\",\r\n            \"lot_status\": 2,\r\n            \"web_url\": \"https://live.bilibili.com/p/html/live-lottery/anchor-join.html\",\r\n            \"send_gift_ensure\": 0,\r\n            \"goods_id\": -99998,\r\n            \"award_type\": 1,\r\n            \"award_price_text\": \"价值52电池\",\r\n            \"ruid\": 3493144066788118,\r\n            \"asset_icon_webp\": \"\",\r\n            \"danmu_type\": 0,\r\n            \"danmu_new\": [\r\n                {\r\n                    \"danmu\": \"我就是天选之人！\",\r\n                    \"danmu_view\": \"\",\r\n                    \"reject\": false\r\n                }\r\n            ]\r\n        }";
                            //test = test.Replace("233", "10");
                            //test = test.Replace("171", "10");
                            //NewMessage?.Invoke(MessageType.AnchorLotteryStart, test);
                        }
                        //if (msg.Text == "test321")
                        //{
                        //    var test = "{\r\n\t\t'award_dont_popup': 1,\r\n\t\t'award_image': '',\r\n\t\t'award_name': '艺术头像绘制',\r\n\t\t'award_num': 1,\r\n\t\t'award_users': [{\r\n\t\t\t'uid': 8318700,\r\n\t\t\t'uname': '桥下念喬',\r\n\t\t\t'face': 'http://i0.hdslb.com/bfs/face/dfde2ffc6286c2c5189592cc84fd70bcf977b143.jpg',\r\n\t\t\t'level': 21,\r\n\t\t\t'color': 5805790\r\n\t\t}],\r\n\t\t'id': 2553648,\r\n\t\t'lot_status': 2,\r\n\t\t'url': 'https://live.bilibili.com/p/html/live-lottery/anchor-join.html?is_live_half_webview=1&hybrid_biz=live-lottery-anchor&hybrid_half_ui=1,5,100p,100p,000000,0,30,0,0,1;2,5,100p,100p,000000,0,30,0,0,1;3,5,100p,100p,000000,0,30,0,0,1;4,5,100p,100p,000000,0,30,0,0,1;5,5,100p,100p,000000,0,30,0,0,1;6,5,100p,100p,000000,0,30,0,0,1;7,5,100p,100p,000000,0,30,0,0,1;8,5,100p,100p,000000,0,30,0,0,1',\r\n\t\t'web_url': 'https://live.bilibili.com/p/html/live-lottery/anchor-join.html'\r\n\t}";
                        //    test = test.Replace('\'', '\"');
                        //    NewMessage?.Invoke(MessageType.AnchorLotteryAward, test);
                        //}
#endif
                        // 用户直播等级(已经被b站弃用)
                        //if (obj["info"][4] != null && obj["info"][4].ToArray().Length != 0)
                        //{
                        //    msg.UserLevel = "UL" + obj["info"][4][0].ToString();
                        //    msg.UserLevelColor = obj["info"][4][2].ToString();
                        //}

                        // 用户头衔(基本没用)
                        //if (obj["info"][5] != null && obj["info"][5].ToArray().Length != 0)
                        //{
                        //    msg.UserTitleID = obj["info"][5][0].ToString();
                        //    msg.ShowTitle = Visibility.Visible;
                        //}

                        NewMessage?.Invoke(MessageType.Danmu, msg);
                        return;
                    }
                }
                else if (cmd == "SEND_GIFT" || cmd == "POPULARITY_RED_POCKET_NEW")
                {
                    var msg = new GiftMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.UserName = obj["data"]["uname"].ToString();
                        msg.Action = obj["data"]["action"].ToString();
                        msg.GiftId = Convert.ToInt32(obj["data"]["giftId"].ToString());
                        msg.GiftName = obj["data"]["giftName"].ToString();
                        msg.Number = obj["data"]["num"].ToString();
                        msg.UserID = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                else if (cmd == "COMBO_SEND")
                {
                    var msg = new GiftMsgModel();
                    if (obj["data"] != null)
                    {
                        msg.UserName = obj["data"]["uname"].ToString();
                        msg.Action = obj["data"]["action"].ToString();
                        msg.GiftId = Convert.ToInt32(obj["data"]["gift_id"].ToString());
                        msg.GiftName = obj["data"]["gift_name"].ToString();
                        msg.Number = obj["data"]["total_num"].ToString();
                        msg.UserID = obj["data"]["uid"].ToString();
                        NewMessage?.Invoke(MessageType.Gift, msg);
                    }
                    return;
                }
                else if (cmd == "INTERACT_WORD")
                {
                    var w = new InteractWordModel();
                    if (obj["data"] != null)
                    {
                        w.UserName = obj["data"]["uname"].ToString();
                        w.UserID = obj["data"]["uid"].ToString();
                        w.MsgType = obj["data"]["msg_type"].ToInt32();

                        if (obj["data"]["fans_medal"]["medal_level"].ToInt32() != 0)
                        {
                            w.MedalName = obj["data"]["fans_medal"]["medal_name"].ToString();
                            w.MedalLevel = obj["data"]["fans_medal"]["medal_level"].ToString();
                            w.MedalColor = obj["data"]["fans_medal"]["medal_color"].ToString();
                            w.ShowMedal = Visibility.Visible;
                        }

                        NewMessage?.Invoke(MessageType.InteractWord, w);
                    }
                    return;
                }
                // 没写相关实现, 先注释掉
                //if (cmd == "SYS_MSG")
                //{
                //    NewMessage?.Invoke(MessageType.SystemMsg, obj["msg"].ToString());
                //    return;
                //}
                else if (cmd == "ANCHOR_LOT_START")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.AnchorLotteryStart, obj["data"].ToString());
                    }
                    return;
                }
                else if (cmd == "ANCHOR_LOT_AWARD")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.AnchorLotteryAward, obj["data"].ToString());
                    }
                    return;
                }
                else if (cmd == "POPULARITY_RED_POCKET_START")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.RedPocketLotteryStart, obj["data"].ToString());
                    }
                    return;
                }
                else if (cmd == "POPULARITY_RED_POCKET_WINNER_LIST")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.RedPocketLotteryWinner, obj["data"].ToString());
                    }
                    return;
                }
                else if (cmd == "SUPER_CHAT_MESSAGE")
                {
                    SuperChatMsgViewModel msgView = new SuperChatMsgViewModel();
                    if (obj["data"] != null)
                    {
                        msgView.BackgroundBottomColor = obj["data"]["background_bottom_color"].ToString();
                        msgView.BackgroundColor = obj["data"]["background_color"].ToString();
                        msgView.BackgroundImage = obj["data"]["background_image"].ToString();
                        msgView.EndTime = obj["data"]["end_time"].ToInt32();
                        msgView.StartTime = obj["data"]["start_time"].ToInt32();
                        msgView.Time = obj["data"]["time"].ToInt32();
                        msgView.MaxTime = msgView.EndTime - msgView.StartTime;
                        msgView.Face = obj["data"]["user_info"]["face"].ToString();
                        msgView.FaceFrame = obj["data"]["user_info"]["face_frame"].ToString();
                        msgView.FontColor = obj["data"]["message_font_color"].ToString();
                        msgView.Message = obj["data"]["message"].ToString();
                        msgView.Price = obj["data"]["price"].ToInt32();
                        msgView.Username = obj["data"]["user_info"]["uname"].ToString();
                        NewMessage?.Invoke(MessageType.SuperChat, msgView);
                    }
                    return;
                }
                else if (cmd == "ROOM_CHANGE")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.RoomChange, new RoomChangeMsgModel()
                        {
                            Title = obj["data"]["title"].ToString(),
                            AreaID = obj["data"]["area_id"].ToInt32(),
                            AreaName = obj["data"]["area_name"].ToString(),
                            ParentAreaName = obj["data"]["parent_area_name"].ToString(),
                            ParentAreaID = obj["data"]["parent_area_id"].ToInt32(),
                        });
                    }
                    return;
                }
                else if (cmd == "GUARD_BUY")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.GuardBuy, new GuardBuyMsgModel()
                        {
                            GiftId = obj["data"]["gift_id"].ToInt32(),
                            GiftName = obj["data"]["gift_name"].ToString(),
                            Num = obj["data"]["num"].ToInt32(),
                            Price = obj["data"]["price"].ToInt32(),
                            UserName = obj["data"]["username"].ToString(),
                            UserID = obj["data"]["uid"].ToString(),
                            GuardLevel = obj["data"]["guard_level"].ToInt32(),
                        });
                    }
                    return;
                }
                else if (cmd == "ROOM_BLOCK_MSG")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.RoomBlock, new RoomBlockMsgModel()
                        {
                            UserID = obj["data"]["uid"].ToString(),
                            UserName = obj["data"]["uname"].ToString(),
                        });
                    }
                }
                else if (cmd == "WARNING" || cmd == "CUT_OFF")
                {
                    if (obj["msg"] != null)
                    {
                        NewMessage?.Invoke(MessageType.WaringOrCutOff, new WarningOrCutOffMsgModel()
                        {
                            Message = obj["msg"].ToString(),
                            Command = cmd,
                        });
                    }
                }
                else if (cmd == "LIVE" || cmd == "REENTER_LIVE_ROOM")
                {
                    if (obj["roomid"] != null)
                    {
                        NewMessage?.Invoke(MessageType.StartLive, obj["roomid"].ToString());
                    }
                }
                else if (cmd == "WATCHED_CHANGE")
                {
                    if (obj["data"] != null)
                    {
                        NewMessage?.Invoke(MessageType.WatchedChange, obj["data"]["text_large"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is JsonReaderException)
                {
                    logger.Error("直播解析JSON包出错", ex);
                }
            }

        }

        /// <summary>
        /// 对数据进行编码
        /// </summary>
        /// <param name="msg">文本内容</param>
        /// <param name="action">2=心跳，7=进房</param>
        /// <returns></returns>
        private ArraySegment<byte> EncodeData(string msg, int action)
        {
            var data = Encoding.UTF8.GetBytes(msg);
            //头部长度固定16
            var length = data.Length + 16;
            var buffer = new byte[length];
            using var ms = new MemoryStream(buffer);
            //数据包长度
            var b = BitConverter.GetBytes(buffer.Length).ToArray().Reverse().ToArray();
            ms.Write(b, 0, 4);
            //数据包头部长度,固定16
            b = BitConverter.GetBytes(16).Reverse().ToArray();
            ms.Write(b, 2, 2);
            //协议版本，0=JSON,1=Int32,2=Buffer
            b = BitConverter.GetBytes(0).Reverse().ToArray(); ;
            ms.Write(b, 0, 2);
            //操作类型
            b = BitConverter.GetBytes(action).Reverse().ToArray(); ;
            ms.Write(b, 0, 4);
            //数据包头部长度,固定1
            b = BitConverter.GetBytes(1).Reverse().ToArray(); ;
            ms.Write(b, 0, 4);
            //数据
            ms.Write(data, 0, data.Length);
            ArraySegment<byte> _bytes = new ArraySegment<byte>(ms.ToArray());
            ms.Flush();
            return _bytes;
        }

        /// <summary>
        /// 解压数据
        /// </summary>
        /// <param name="protocolVersion"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private byte[] DecompressData(int protocolVersion, byte[] body)
        {
            body = protocolVersion switch
            {
                2 => DecompressDataWithDeflate(body),
                3 => DecompressDataWithBrotli(body),
                _ => body
            };
            return body;
        }

        /// <summary>
        /// 解压数据 (使用Deflate)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressDataWithDeflate(byte[] data)
        {
            using var outBuffer = new MemoryStream();
            using var compressedzipStream = new DeflateStream(new MemoryStream(data, 2, data.Length - 2), CompressionMode.Decompress);
            var block = new byte[1024];
            while (true)
            {
                var bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            return outBuffer.ToArray();
        }

        /// <summary>
        /// 解压数据 (使用 Brotli)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private byte[] DecompressDataWithBrotli(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            heartBeatTimer?.Stop();
            heartBeatTimer?.Dispose();
            ws.Dispose();
        }
    }
}
