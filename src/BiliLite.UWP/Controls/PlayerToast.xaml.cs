using BiliLite.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class PlayerToast : UserControl
    {
        private readonly PlayerToastViewModel m_viewModel;
        public event EventHandler SkipButtonClick;
        public event EventHandler HideToast;

        public PlayerToast(PlayerToastViewModel viewModel)
        {
            m_viewModel = viewModel;
            InitializeComponent();
        }

        public string Text
        {
            set => m_viewModel.Text = value;
        }

        public bool ShowSkipButton { get; set; } = false;

        /// <summary>
        /// 是否显示「关闭」按钮，仅关闭当前这一次提示
        /// </summary>
        public bool ShowCloseButton
        {
            get => BtnClose.Visibility == Visibility.Visible;
            set => BtnClose.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        public SolidColorBrush IconBrush { get; set; }

        public bool ShowIcon => IconBrush != null && IconBrush != new SolidColorBrush(Colors.Transparent);

        public void Show()
        {
            var storyboard = (Storyboard)Resources["ShowToast"];
            storyboard.Begin();
        }

        public async Task Hide()
        {
            var storyboard = (Storyboard)Resources["HideToast"];
            storyboard.Begin();
            var tcs = new TaskCompletionSource<bool>();

            storyboard.Completed += (s, e) =>
            {
                tcs.SetResult(true);
            };
            storyboard.Begin();
            await tcs.Task;
        }

        private async void SkipButton_OnClick(object sender, RoutedEventArgs e)
        {
            SkipButtonClick?.Invoke(this, EventArgs.Empty);
            HideToast?.Invoke(sender, EventArgs.Empty);
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            HideToast?.Invoke(sender, EventArgs.Empty);
        }
    }
}