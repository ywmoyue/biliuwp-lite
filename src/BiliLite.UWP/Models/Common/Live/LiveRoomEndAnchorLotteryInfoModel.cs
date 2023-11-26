using System;
using Newtonsoft.Json;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using System.Collections.ObjectModel;
using System.Linq;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomEndAnchorLotteryInfoModel
    {
        public int Id { get; set; }

        [JsonProperty("lot_status")]
        public int LotStatus { get; set; }

        [JsonProperty("award_image")]
        public string AwardImage { get; set; }

        [JsonProperty("award_name")]
        public string AwardName { get; set; }

        [JsonProperty("award_num")]
        public int AwardNum { get; set; }

        public string Url { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }

        [JsonProperty("award_users")]
        public ObservableCollection<LiveRoomEndAnchorLotteryInfoUserModel> AwardUsers { get; set; }
        
        public StackPanel WinnerList
        {
            get => AwardUsers == null ? new StackPanel() : GenerateWinnerList(AwardUsers);
        }

        public StackPanel GenerateWinnerList(ObservableCollection<LiveRoomEndAnchorLotteryInfoUserModel> AwardUsers)
        {
            var result = new StackPanel()
            {
                // <StackPanel Grid.Column="1" Margin="0 4" Orientation="Vertical" HorizontalAlignment="Center">
                Orientation = Orientation.Vertical,
            };
            if (AwardUsers != null && AwardUsers.ToArray().Length > 0)
            {
                var award_users = AwardUsers;
                foreach (var item in award_users)
                {
                    var sp = new StackPanel()
                    {
                        // <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="0 4">
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 4, 0, 4),
                    };
                    var e = new Ellipse()
                    {
                        Width = 30,
                        Height = 30,
                        Fill = new ImageBrush()
                        {
                            // <ImageBrush ImageSource="whatever" Stretch="UniformToFill"/>
                            ImageSource = new BitmapImage(new Uri(item.Face + "@30h")),
                            Stretch = Stretch.UniformToFill,
                        }
                    };
                    var t = new TextBlock()
                    {
                        // <TextBlock Margin="4 0" TextWrapping="Wrap" Text="TestUser测试文字" VerticalAlignment="Center">
                        Margin = new Thickness(4, 0, 4, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Text = item.Uname,
                        VerticalAlignment = VerticalAlignment.Center,
                    };

                    sp.Children.Add(e);
                    sp.Children.Add(t);
                    result.Children.Add(sp);
                }
            }
            return result;
        }
    }
}