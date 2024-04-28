using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using BiliLite.ViewModels;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class PlayerToast : UserControl
    {
        private readonly PlayerToastViewModel m_viewModel;

        public PlayerToast(PlayerToastViewModel viewModel)
        {
            m_viewModel = viewModel;
            this.InitializeComponent();
        }

        public string Text
        {
            set => m_viewModel.Text = value;
        }

        public void Show()
        {
            var storyboard = (Storyboard)this.Resources["ShowToast"];
            storyboard.Begin();
        }

        public async Task Hide()
        {
            var storyboard = (Storyboard)this.Resources["HideToast"];
            storyboard.Begin();
            var tcs = new TaskCompletionSource<bool>();

            storyboard.Completed += (s, e) =>
            {
                tcs.SetResult(true);
            };
            storyboard.Begin();
            await tcs.Task;
        }
    }
}
