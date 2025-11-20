using Windows.Foundation.Metadata;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace BiliLite.Controls
{
    public class HorizontalGridView : GridView
    {
        public HorizontalGridView()
        {
            DefaultStyleKey = typeof(HorizontalGridView);
        }

        public Grid root;
        public Button btnMoveLeft;
        public Button btnMoveRight;
        public ScrollViewer scrollViewer;
        public Grid itemsPresenterPanel;

        protected override void OnApplyTemplate()
        {
            root = GetTemplateChild("root") as Grid;
            btnMoveLeft = GetTemplateChild("moveLeft") as Button;
            btnMoveRight = GetTemplateChild("moveRight") as Button;
            scrollViewer = GetTemplateChild("scrollViewer") as ScrollViewer;
            itemsPresenterPanel = GetTemplateChild("itemsPresenterPanel") as Grid;

            itemsPresenterPanel.PointerExited += ItemsPresenterPanel_PointerExited;
            itemsPresenterPanel.PointerWheelChanged += ItemsPresenterPanel_PointerWheelChanged;

            root.PointerEntered += Root_PointerEntered;
            root.PointerExited += Root_PointerExited;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            btnMoveLeft.Click += BtnMoveLeft_Click;
            btnMoveRight.Click += BtnMoveRight_Click;

            base.OnApplyTemplate();
        }

        #region 触摸板
        private void ItemsPresenterPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            scrollViewer.HorizontalScrollMode = ScrollMode.Enabled;
        }

        private void ItemsPresenterPanel_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
        }
        #endregion

        #region 附加按钮
        private void Root_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            setButton();
        }

        private void Root_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (AlwaysShowButton)
            {
                return;
            }
            btnMoveLeft.Visibility = Visibility.Collapsed;
            btnMoveRight.Visibility = Visibility.Collapsed;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            setButton();
        }

        void setButton()
        {
            if (AlwaysShowButton)
            {
                btnMoveLeft.Visibility = Visibility.Visible;
                btnMoveRight.Visibility = Visibility.Visible;
                return;
            }
            if (scrollViewer.HorizontalOffset > 0)
            {
                btnMoveLeft.Visibility = Visibility.Visible;
            }
            else
            {
                btnMoveLeft.Visibility = Visibility.Collapsed;
            }
            if (scrollViewer.HorizontalOffset < scrollViewer.ScrollableWidth)
            {
                btnMoveRight.Visibility = Visibility.Visible;
            }
            else
            {
                btnMoveRight.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnMoveRight_Click(object sender, RoutedEventArgs e)
        {
            var move = scrollViewer.HorizontalOffset + MoveOffset;
            if (move >= scrollViewer.ScrollableWidth)
            {
                move = scrollViewer.ScrollableWidth;
            }
            scrollViewer.ChangeView(move, null, null);
        }

        private void BtnMoveLeft_Click(object sender, RoutedEventArgs e)
        {
            var move = scrollViewer.HorizontalOffset - MoveOffset;
            if (move <= 0)
            {
                move = 0;
            }
            scrollViewer.ChangeView(move, null, null);
        }

        public bool AlwaysShowButton
        {
            get { return (bool)GetValue(AlwaysShowButtonProperty); }
            set { SetValue(AlwaysShowButtonProperty, value); }
        }

        public static readonly DependencyProperty AlwaysShowButtonProperty =
            DependencyProperty.Register("AlwaysShowButton", typeof(bool), typeof(HorizontalGridView), new PropertyMetadata(false, OnAlwaysShowButtonChanged));
        private static void OnAlwaysShowButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var data = d as HorizontalGridView;
            if ((bool)e.NewValue)
            {
                data.btnMoveLeft.Visibility = Visibility.Visible;
                data.btnMoveRight.Visibility = Visibility.Visible;
            }
        }
        #endregion

        public double MoveOffset
        {
            get { return (double)GetValue(MoveOffsetProperty); }
            set { SetValue(MoveOffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MoveOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MoveOffsetProperty =
            DependencyProperty.Register("MoveOffset", typeof(double), typeof(HorizontalGridView), new PropertyMetadata((double)200.0));

    }
}