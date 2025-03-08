using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls
{
    public class MyAdaptiveGridView : AdaptiveGridView
    {
        public MyAdaptiveGridView()
        {
            //bool isWin11Supported = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 14);
            //if (isWin11Supported)
            ItemContainerStyle = (Style)App.Current.Resources["LocalGridViewItemStyle"];
        }

        private ICommand _LoadMoreCommand;
        public ICommand LoadMoreCommand
        {
            get { return _LoadMoreCommand; }
            set { _LoadMoreCommand = value; }
        }
        public bool CanLoadMore { get; set; } = false;

        public double ScrollPosition => scrollViewer.VerticalOffset;

        public double LoadMoreBottomOffset
        {
            get { return Convert.ToDouble(GetValue(LoadMoreBottomOffsetProperty)); }
            set { SetValue(LoadMoreBottomOffsetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LoadMoreBottomOffset.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadMoreBottomOffsetProperty =
            DependencyProperty.Register("LoadMoreBottomOffset", typeof(double), typeof(MyAdaptiveGridView), new PropertyMetadata(100));

        public new bool Loading
        {
            get { return (bool)GetValue(LoadingProperty); }
            set { SetValue(LoadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Loading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LoadingProperty =
            DependencyProperty.Register("Loading", typeof(bool), typeof(MyAdaptiveGridView), new PropertyMetadata(true));

        ScrollViewer scrollViewer;

        protected override void OnApplyTemplate()
        {
            scrollViewer = GetTemplateChild("ScrollViewer") as ScrollViewer;
            scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            RegisterPropertyChangedCallback(LoadingProperty, new DependencyPropertyChangedCallback((obj, e) =>
            {
                if (!Loading)
                {
                    if (scrollViewer.ScrollableHeight == 0)
                    {
                        LoadMoreCommand?.Execute(null);
                    }
                }
            }));

            base.OnApplyTemplate();
        }

        private void MyAdaptiveGridView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

            Debug.WriteLine("内容变更");
            if (scrollViewer.ScrollableHeight == 0)
            {
                LoadMoreCommand?.Execute(null);
            }
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight - LoadMoreBottomOffset && CanLoadMore)
            {
                LoadMoreCommand?.Execute(null);
            }
        }

        public void ScrollTo(double offset)
        {
            scrollViewer?.ChangeView(null, offset, null);
        }

        public async Task ScrollRecover()
        {
            var position = ScrollPosition;
            await Task.Delay(50);
            ScrollTo(position);
        }
    }
}
