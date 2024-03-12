using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using BiliLite.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class AttentionButton : UserControl
    {
        private readonly UserAttentionButtonViewModel m_viewModel;

        public AttentionButton()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserAttentionButtonViewModel>();
            this.InitializeComponent();
        }

        public static readonly DependencyProperty AttentionProperty =
            DependencyProperty.Register(nameof(Attention), typeof(int), typeof(AttentionButton), new PropertyMetadata(0));

        public int Attention
        {
            get => m_viewModel.Attention;
            set => m_viewModel.Attention = value;
        }

        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(Attention), typeof(string), typeof(AttentionButton), new PropertyMetadata(default(string)));

        public string UserId
        {
            get => m_viewModel.UserId;
            set => m_viewModel.UserId = value;
        }

        public async Task AttentionUp()
        {
            await m_viewModel.AttentionUP(m_viewModel.UserId, 1);
        }

        private void AttendedBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var flyoutShowOptions = new FlyoutShowOptions()
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            AttentionFlyout.ShowAt(sender as DependencyObject, flyoutShowOptions);
        }

        private async void SetFollowingTag_OnClick(object sender, RoutedEventArgs e)
        {
            if(!UserFollowingTagsFlyout.HasInit)
                await UserFollowingTagsFlyout.Init(m_viewModel.UserId);
            UserFollowingTagsFlyout.ShowAt(AttendedBtn);
        }
    }
}
