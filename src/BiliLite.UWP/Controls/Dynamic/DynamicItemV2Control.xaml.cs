using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.ViewModels.UserDynamic;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace BiliLite.Controls.Dynamic
{
    public sealed partial class DynamicItemV2Control : UserControl
    {
        public DynamicItemV2Control()
        {
            this.InitializeComponent();
        }

        public FrameworkElement CardContent
        {
            get => (FrameworkElement)GetValue(CardContentProperty);
            set => SetValue(CardContentProperty, value);
        }

        public static readonly DependencyProperty CardContentProperty =
            DependencyProperty.Register(nameof(CardContent), typeof(FrameworkElement), typeof(DynamicItemV2Control), new PropertyMetadata(null));

        public DynamicV2ItemViewModel ViewModel
        {
            get => (DynamicV2ItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(DynamicV2ItemViewModel), typeof(DynamicItemV2Control), new PropertyMetadata(null));
    }
}
