using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using BiliLite.Modules.SpBlock.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Converters;
using BiliLite.Models.Common.Player;
using Windows.UI.Xaml.Shapes;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Modules.ExtraInterface;
using BiliLite.Services;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Modules.SpBlock.Controls
{
    [RegisterTransientService(typeof(IPlayerSponsorBlockControl))]
    public sealed partial class PlayerSponsorBlockControl : UserControl, IPlayerSponsorBlockControl
    {
        private readonly PlayerSponsorBlockControlViewModel m_viewModel; 
        private readonly bool m_sponsorBlockFlag;
        private readonly ISponsorBlockService m_sponsorBlockService;

        public PlayerSponsorBlockControl()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<PlayerSponsorBlockControlViewModel>();
            m_sponsorBlockService = App.ServiceProvider.GetRequiredService<ISponsorBlockService>();
            m_sponsorBlockFlag = SettingService.GetValue(SettingConstants.Player.SPONSOR_BLOCK,
                SettingConstants.Player.DEFAULT_SPONSOR_BLOCK);
            this.InitializeComponent();
        }

        public List<PlayerSkipItem> SegmentSkipItems => m_viewModel.SponsorBlockSegmentList;

        public event EventHandler<double> UpdatePosition;

        public void LoadSponsorBlock(string bvid, string cid, double duration)
        {
            if (!m_sponsorBlockFlag) return;
            m_viewModel.ShowSponsorBlockBtn = true;

            var vaildSeg = m_sponsorBlockService.GetVideoSponsorBlocks(bvid, cid, duration);
            m_viewModel.SponsorBlockSegmentList = vaildSeg;

            SponsorBlockStackPanel.Children.Clear();
            AddSegmentToStackPanel(m_viewModel.SponsorBlockSegmentList);

            SponsorBlockStackPanel.Visibility =
                SponsorBlockStackPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            SponsorBlockMsg.Text = m_viewModel.SponsorBlockSegmentList.Count > 0 ?
                $"🎉此视频在数据库中有 {m_viewModel.SponsorBlockSegmentList.Count} 个可跳过片段！" :
                "😢在数据库中未找到此视频的可跳过片段";
        }

        public void AddSegmentToStackPanel(List<PlayerSkipItem> list)
        {
            if (list == null || list.Count == 0) return;

            foreach (var item in list)
            {
                // 创建Button
                var button = new Button
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    VerticalContentAlignment = VerticalAlignment.Stretch,
                    Background = new SolidColorBrush(Colors.Transparent),
                };

                // 创建Grid作为Button的内容
                var grid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };

                // 添加列定义
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // 创建圆形提示
                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = item.Brush,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                Grid.SetColumn(ellipse, 0);

                // 创建第一个TextBlock
                var textBlock1 = new TextBlock
                {
                    Padding = new Thickness(0),
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2),
                    Text = item.SegmentName
                };
                Grid.SetColumn(textBlock1, 1);

                // 创建第二个TextBlock
                var textBlock2 = new TextBlock
                {
                    Padding = new Thickness(0),
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 2),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Text = $"{TimeSpanStrFormatConverter.Convert(item.Start)} ➡️ {TimeSpanStrFormatConverter.Convert(item.End)}"
                };
                Grid.SetColumn(textBlock2, 3);

                // 将控件添加到Grid中
                grid.Children.Add(ellipse);
                grid.Children.Add(textBlock1);
                grid.Children.Add(textBlock2);

                // 将Grid设置为Button的内容
                button.Content = grid;

                // 设置按钮点击事件为跳到片段结尾并关闭Flyout
                button.Click += (_, _) =>
                {
                    UpdatePosition?.Invoke(this, item.End);
                    SponsorBlockFlyout.Hide();
                };

                // 将Button添加到StackPanel中
                SponsorBlockStackPanel.Children.Add(button);
            }
        }
    }
}
