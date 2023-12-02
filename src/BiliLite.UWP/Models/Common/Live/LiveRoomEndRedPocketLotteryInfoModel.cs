using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomEndRedPocketLotteryInfoModel
    {
        /// <summary>
        /// 抽奖的id号
        /// </summary>
        [JsonProperty("lot_id")]
        public string LotId { get; set; }

        /// <summary>
        /// 总中奖人数
        /// </summary>
        [JsonProperty("total_num")]
        public int TotalNumber { get; set; }

        /// <summary>
        /// 中奖者详细信息
        /// </summary>
        [JsonProperty("winner_info")]
        public ObservableCollection<ObservableCollection<string>> Winners { get; set; }

        public RichTextBlock WinnersList => WinnerToRichTextBlock();

        /// <summary>
        /// 中奖的礼物信息
        /// </summary>
        [JsonProperty("awards")]
        public IDictionary<string, LiveRoomEndRedPocketLotteryInfoAwardModel> Awards { get; set; }

        /// <summary>
        /// 版本号? 待调查, 可能用于区别不同版本的Json反馈
        /// </summary>
        [JsonProperty("version")]
        public int Version { get; set; }

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private RichTextBlock WinnerToRichTextBlock()
        {
            try
            {
                //var result = (RichTextBlock)XamlReader.Load("<RichTextBlock Margin=\"0 4\" HorizontalAlignment=\"Center\" LineHeight=\"28\"></RichTextBlock>");
                var result = new RichTextBlock() { LineHeight = 28 };
                if (Winners == null) throw new CustomizedErrorException("红包中奖名单为空");
                foreach (var winner in Winners)
                {
                    var name = winner[1];
                    var giftId = winner[3];
                    var p = $@"<Paragraph xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                     xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" 
                                     xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
                                     xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"">
                            {winner[1]} 获得
                            <InlineUIContainer>
                                <Image Source=""{Awards[giftId].AwardPic}"" Margin=""0 2 -2 -6"" Width=""24"" Height=""24""/>
                            </InlineUIContainer>
                            {Awards[giftId].AwardName}
                        </Paragraph>";

                    var paragraph = (Paragraph)XamlReader.Load(p);
                    result.Blocks.Add(paragraph);
                }
                return result;
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("红包中奖名单富文本转换失败");
                _logger.Error("红包中奖名单富文本转换失败", ex);
                var text = new RichTextBlock();
                var paragraph = new Paragraph();
                var run = new Run();
                paragraph.Inlines.Add(run);
                text.Blocks.Add(paragraph);
                return text;
            }
        }
    }

    public class LiveRoomEndRedPocketLotteryInfoAwardModel
    {
        /// <summary>
        /// 奖品类型? 待调查
        /// </summary>
        [JsonProperty("award_type")]
        public int AwardType { get; set; }

        /// <summary>
        /// 奖品名字
        /// </summary>
        [JsonProperty("award_name")]
        public string AwardName { get; set; }

        /// <summary>
        /// 奖品图片
        /// </summary>
        [JsonProperty("award_pic")]
        public string AwardPic { get; set; }

        /// <summary>
        /// 奖品价格(以金瓜子计算)
        /// </summary>
        [JsonProperty("award_price")]
        public int AwardPrice {  get; set; }
    }
}
