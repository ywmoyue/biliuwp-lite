using Windows.UI.Core;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace BiliLite.Extensions
{
    public static class DisableScrollExtension
    {
        public static readonly DependencyProperty DisableScrollProperty =
        DependencyProperty.RegisterAttached(
            "DisableScroll",
            typeof(bool),
            typeof(DisableScrollExtension),
            new PropertyMetadata(false, OnDisableScrollChanged));

        public static bool GetDisableScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisableScrollProperty);
        }

        public static void SetDisableScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(DisableScrollProperty, value);
        }

        private static void OnDisableScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;

            if ((bool)e.NewValue)
            {
                element.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    var scrollViewer = FindScrollViewer(element);
                    if (scrollViewer != null)
                    {
                        scrollViewer.VerticalScrollMode = ScrollMode.Disabled;
                        scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
                    }
                });
            }
        }

        private static ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            return parent?.FindFirstChildByType<ScrollViewer>();
        }
    }
}
