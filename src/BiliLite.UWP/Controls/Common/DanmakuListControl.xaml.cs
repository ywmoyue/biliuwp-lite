using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Models.Requests.Api;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Common;
using CommunityToolkit.WinUI;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Common
{
    public sealed partial class DanmakuListControl : UserControl
    {
        private readonly DanmakuListViewModel m_viewModel;

        public DanmakuListControl()
        {
            m_viewModel = new DanmakuListViewModel();
            this.InitializeComponent();
        }

        public PlayerControl PlayerControl { get; set; }

        public IDanmakuController DanmakuController { get; set; }

        private void BtnOpenDanmakuList_OnClick(object sender, RoutedEventArgs e)
        {
            m_viewModel.ShowAllDanmu = false;
            m_viewModel.Danmakus = DanmakuController.FindDanmakus((int)(PlayerControl.PlayerInstance.Position));
        }

        private void BtnReplaceShowMode_OnClick(object sender, RoutedEventArgs e)
        {
            m_viewModel.ShowAllDanmu = !m_viewModel.ShowAllDanmu;
            if (m_viewModel.ShowAllDanmu)
            {
                m_viewModel.Danmakus = DanmakuController.FindDanmakus();
            }
            else
            {
                m_viewModel.Danmakus = DanmakuController.FindDanmakus((int)(PlayerControl.PlayerInstance.Position));
            }
        }

        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element)
            {
                return;
            }

            var listViewItem = element.FindAscendant<ListViewItem>();

            if (listViewItem.Content is not DanmakuSimpleItem danmakuItem)
            {
                return;
            }

            danmakuItem.Content.SetClipboard();
            NotificationShowExtensions.ShowMessageToast("已复制弹幕内容");
        }

        private async void BtnLike_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element)
            {
                return;
            }

            var listViewItem = element.FindAscendant<ListViewItem>();

            if (listViewItem.Content is not DanmakuSimpleItem danmakuItem)
            {
                return;
            }

            var api = new DanmakuApi().Like(danmakuItem.Id, PlayerControl.CurrentPlayItem.cid, 1);
            var results = await api.Request();
            if (results.status)
            {
                var data = await results.GetResult<object>();
                if (data.success)
                {
                    NotificationShowExtensions.ShowMessageToast("已点赞弹幕");
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast("点赞弹幕失败：" + data.message);
                }
            }
            else
            {
                NotificationShowExtensions.ShowMessageToast("点赞弹幕失败：" + results.message);
            }
        }

        private async void BtnBlockUser_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is not FrameworkElement element)
            {
                return;
            }

            var listViewItem = element.FindAscendant<ListViewItem>();

            if (listViewItem.Content is not DanmakuSimpleItem danmakuItem)
            {
                return;
            }

            await PlayerControl.DanmuSettingAddUser(danmakuItem.MidHash);
        }
    }
}
