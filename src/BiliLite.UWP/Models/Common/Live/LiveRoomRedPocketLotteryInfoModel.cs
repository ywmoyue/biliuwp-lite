using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomRedPocketLotteryInfoModel
    {
        /// <summary>
        /// 红包id
        /// </summary>
        [JsonProperty("lot_id")]
        public string LotteryId { get; set; }

        /// <summary>
        /// 发送者名称
        /// </summary>
        [JsonProperty("sender_name")]
        public string SenderName { get; set; }

        /// <summary>
        /// 发送者UID
        /// </summary>
        [JsonProperty("sender_uid")]
        public string SenderUid { get; set; }

        /// <summary>
        /// 发送者头像
        /// </summary>
        [JsonProperty("sender_face")]
        public string SenderFace {  get; set; }

        /// <summary>
        /// 抽取条件? 未知
        /// </summary>
        [JsonProperty("join_requirement")]
        public int JoinRequirement { get; set; }

        /// <summary>
        /// 抽取红包所发送的弹幕
        /// </summary>
        [JsonProperty("danmu")]
        public string Danmu { get; set; }

        /// <summary>
        /// 礼物列表
        /// </summary>
        [JsonProperty("awards")]
        public ObservableCollection<LiveRoomRedPocketLotteryAwardInfoModel> Awards { get; set; }

        public RichTextBlock AwardsList { get => AwardsToRichTextBlock(); }

        /// <summary>
        /// 红包开始时间
        /// </summary>
        [JsonProperty("start_time")]
        public int StartTime { get; set; }

        /// <summary>
        /// 红包结束时间
        /// </summary>
        [JsonProperty("end_time")]
        public int EndTime { get; set; }

        /// <summary>
        /// 红包持续时间(即EndTime - StartTime)
        /// </summary>
        [JsonProperty("last_time")]
        public int LastTime { get; set; }

        /// <summary>
        /// 红包按钮移除时间
        /// </summary>
        [JsonProperty("remove_time")]
        public int RemoveTime { get; set; }

        /// <summary>
        /// ?待研究
        /// </summary>
        [JsonProperty("replace_time")]
        public int ReplaceTime { get; set; }

        /// <summary>
        /// 目前时间
        /// </summary>
        [JsonProperty("current_time")]
        public int CurrentTime { get; set; }

        /// <summary>
        /// 抽奖状态 1为正在倒计时 2为已经开奖
        /// </summary>
        [JsonProperty("lot_status")]
        public int LotteryStatus { get; set; }

        /// <summary>
        /// 用户状态(可能是是否可以参与)? 待研究
        /// </summary>
        [JsonProperty("user_status")]
        public int UserStatus { get; set; }

        /// <summary>
        /// 红包总共金额
        /// </summary>
        [JsonProperty("total_price")]
        public int TotalPrice { get; set; }

        /// <summary>
        /// 可能是用于多个红包排队? 待研究
        /// </summary>
        [JsonProperty("wait_num")]
        public int WaitNumber { get; set; }

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private RichTextBlock AwardsToRichTextBlock()
        {
            try
            {
                //var result = (RichTextBlock)XamlReader.Load("<RichTextBlock Margin=\"0 4\" HorizontalAlignment=\"Center\" LineHeight=\"28\"></RichTextBlock>");
                var result = new RichTextBlock() { LineHeight = 28 };
                if (Awards != null)
                {
                    foreach (var item in Awards)
                    {
                        var p = string.Format(
                        @"<Paragraph xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                     xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" 
                                     xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
                                     xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"">
                            <InlineUIContainer>
                                <Image Source=""{0}"" Margin=""2 2 2 -6"" Width=""{1}"" Height=""{1}""/>
                            </InlineUIContainer>
                            <Run Text=""{2}  ×  {3}""/>
                        </Paragraph>",
                        item.GiftPicture, 24, item.GiftName, item.GiftNumber);

                        var paragraph = (Paragraph)XamlReader.Load(p);
                        result.Blocks.Add(paragraph);
                    }
                    return result;
                }
                else
                {
                    throw new CustomizedErrorException("红包奖品为空");
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("红包奖品富文本转换失败");
                _logger.Error("红包奖品富文本转换失败", ex);
                var tx = new RichTextBlock();
                Paragraph paragraph = new Paragraph();
                Run run = new Run();
                paragraph.Inlines.Add(run);
                tx.Blocks.Add(paragraph);
                return tx;
            }
        }
    }

    public class LiveRoomRedPocketLotteryAwardInfoModel
    {
        /// <summary>
        /// 礼物ID
        /// </summary>
        [JsonProperty("gift_id")]
        public string GiftId { get; set; }

        /// <summary>
        /// 礼物名字
        /// </summary>
        [JsonProperty("gift_name")]
        public string GiftName { get; set; }

        /// <summary>
        /// 礼物图片
        /// </summary>
        [JsonProperty("gift_pic")]
        public string GiftPicture { get; set; }

        /// <summary>
        /// 礼物数量
        /// </summary>
        [JsonProperty("num")]
        public int GiftNumber { get; set; }
    }
}
