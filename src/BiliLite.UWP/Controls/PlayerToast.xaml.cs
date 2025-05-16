using BiliLite.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

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

        public bool ShowSkipButton
        {
            get => m_viewModel.ShowSkipButton;
            set => m_viewModel.ShowSkipButton = value;
        }

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
    }
}
