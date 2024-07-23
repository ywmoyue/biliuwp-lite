using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using BiliLite.ViewModels.Common;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Common
{
    public sealed partial class PieControl : UserControl
    {
        private readonly PieControlViewModel m_viewModel;

        public PieControl()
        {
            m_viewModel = new PieControlViewModel();
            this.InitializeComponent();
        }

        public event EventHandler<TappedRoutedEventArgs> FirstBtnTapped;

        public event EventHandler<TappedRoutedEventArgs> SecondBtnTapped;

        public event EventHandler<TappedRoutedEventArgs> ThirdBtnTapped;

        public IconElement FirstBtnIcon
        {
            get => (IconElement)GetValue(FirstBtnIconProperty);
            set => SetValue(FirstBtnIconProperty, value);
        }

        public static readonly DependencyProperty FirstBtnIconProperty =
            DependencyProperty.Register(nameof(IconElement),
                typeof(FrameworkElement),
                typeof(PieControl),
                new PropertyMetadata(null));

        public IconElement SecondBtnIcon
        {
            get => (IconElement)GetValue(SecondBtnIconProperty);
            set => SetValue(SecondBtnIconProperty, value);
        }

        public static readonly DependencyProperty SecondBtnIconProperty =
            DependencyProperty.Register(nameof(IconElement),
                typeof(FrameworkElement),
                typeof(PieControl),
                new PropertyMetadata(null));

        public IconElement ThirdBtnIcon
        {
            get => (IconElement)GetValue(ThirdBtnIconProperty);
            set => SetValue(ThirdBtnIconProperty, value);
        }

        public static readonly DependencyProperty ThirdBtnIconProperty =
            DependencyProperty.Register(nameof(IconElement),
                typeof(FrameworkElement),
                typeof(PieControl),
                new PropertyMetadata(null));

        public string FirstBtnToolTip
        {
            get => (string)GetValue(FirstBtnToolTipProperty);
            set => SetValue(FirstBtnToolTipProperty, value);
        }

        public static readonly DependencyProperty FirstBtnToolTipProperty =
            DependencyProperty.Register(nameof(FirstBtnToolTip),
                typeof(string),
                typeof(PieControl),
                new PropertyMetadata(null));

        public string SecondBtnToolTip
        {
            get => (string)GetValue(SecondBtnToolTipProperty);
            set => SetValue(SecondBtnToolTipProperty, value);
        }

        public static readonly DependencyProperty SecondBtnToolTipProperty =
            DependencyProperty.Register(nameof(SecondBtnToolTip),
                typeof(string),
                typeof(PieControl),
                new PropertyMetadata(null));

        public string ThirdBtnToolTip
        {
            get => (string)GetValue(ThirdBtnToolTipProperty);
            set => SetValue(ThirdBtnToolTipProperty, value);
        }

        public static readonly DependencyProperty ThirdBtnToolTipProperty =
            DependencyProperty.Register(nameof(ThirdBtnToolTip),
                typeof(string),
                typeof(PieControl),
                new PropertyMetadata(null));

        private async void BtnClosePie_Click(object sender, RoutedEventArgs args)
        {
            await ClosePie();
        }

        private async void BtnExtentPie_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await ExtentPie();
        }

        private async Task ClosePie()
        {
            m_viewModel.ExtendVisibility = true;
            var storyboard = (Storyboard)this.Resources["ClosePieStory"];
            //storyboard.Begin();
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<object> completeAction = (s, e) =>
            {
                tcs.SetResult(true);
            };
            storyboard.Completed += completeAction;
            storyboard.Begin();
            await tcs.Task;
            m_viewModel.ExtendVisibility = false;
            storyboard.Completed -= completeAction;
        }

        private async Task ExtentPie()
        {
            m_viewModel.ExtendVisibility = true;
            var storyboard = (Storyboard)this.Resources["ExtentPieStory"];
            //storyboard.Begin();
            var tcs = new TaskCompletionSource<bool>();

            EventHandler<object> completeAction = (s, e) =>
            {
                tcs.SetResult(true);
            };

            storyboard.Completed += completeAction;
            storyboard.Begin();
            await tcs.Task;
            storyboard.Completed -= completeAction;
        }

        private void ThirdBtn_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            ThirdBtnTapped?.Invoke(this, e);
        }

        private void FirstBtn_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            FirstBtnTapped?.Invoke(this, e);
        }

        private void SecondBtn_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SecondBtnTapped?.Invoke(this, e);
        }
    }
}
