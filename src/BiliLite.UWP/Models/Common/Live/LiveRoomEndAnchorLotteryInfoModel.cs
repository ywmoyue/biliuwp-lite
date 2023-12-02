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
        
        public StackPanel WinnerList => AwardUsers == null ? new StackPanel() : GenerateWinnerList(AwardUsers);

        public StackPanel GenerateWinnerList(ObservableCollection<LiveRoomEndAnchorLotteryInfoUserModel> awardUsers)
        {
            var result = new StackPanel()
            {
                // <StackPanel Grid.Column="1" Margin="0 4" Orientation="Vertical" HorizontalAlignment="Center">
                Orientation = Orientation.Vertical,
            };
            if (awardUsers == null || awardUsers.ToArray().Length <= 0) return result;
            foreach (var awardUser in awardUsers)
            {
                var awardUserItemPanel = new StackPanel()
                {
                    // <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" Margin="0 4">
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 4, 0, 4),
                };
                var userFaceImage = new Ellipse()
                {
                    Width = 30,
                    Height = 30,
                    Fill = new ImageBrush()
                    {
                        // <ImageBrush ImageSource="whatever" Stretch="UniformToFill"/>
                        ImageSource = new BitmapImage(new Uri(awardUser.Face + "@30h")),
                        Stretch = Stretch.UniformToFill,
                    }
                };
                var userNameText = new TextBlock()
                {
                    // <TextBlock Margin="4 0" TextWrapping="Wrap" Text="TestUser测试文字" VerticalAlignment="Center">
                    Margin = new Thickness(4, 0, 4, 0),
                    TextWrapping = TextWrapping.Wrap,
                    Text = awardUser.Uname,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                awardUserItemPanel.Children.Add(userFaceImage);
                awardUserItemPanel.Children.Add(userNameText);
                result.Children.Add(awardUserItemPanel);
            }
            return result;
        }
    }
}