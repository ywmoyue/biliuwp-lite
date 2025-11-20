using System;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace BiliLite.Controls
{
    public partial class CarouselPanel : Panel
    {
        public CarouselPanel()
        {
            UseLayoutRounding = true;
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Center;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_items.Count > 0)
            {
                ArrangePanes(availableSize.Width);

                int index = Index;
                int itemCount = _items.Count;
                int paneCount = base.Children.Count;
                int leftCount = (paneCount - 1) / 2;

                for (int n = 0; n < paneCount; n++)
                {
                    int paneIndex = (index + n).Mod(paneCount);
                    var pane = base.Children[paneIndex] as ContentControl;

                    int itemIndex = (index + n - leftCount).Mod(itemCount);
                    pane.ContentTemplate = ItemTemplate;
                    pane.Content = _items[itemIndex];
                    pane.Tag = itemIndex;

                    pane.Measure(new Size(ItemWidth, ItemHeight));
                }

                return availableSize;
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (_items.Count > 0)
            {
                int index = Index;
                int paneCount = base.Children.Count;

                double itemWidth = ItemWidth;
                double x = index * ItemWidth;

                x -= (paneCount * ItemWidth - finalSize.Width) / 2.0;

                for (int n = 0; n < paneCount; n++)
                {
                    int paneIndex = (index + n).Mod(paneCount);
                    var pane = base.Children[paneIndex] as ContentControl;

                    pane.Arrange(new Rect(x, 0, itemWidth, finalSize.Height));

                    x += itemWidth;
                }

                return new Size(0, finalSize.Height);
            }
            return base.ArrangeOverride(finalSize);
        }

        #region ArrangePanes
        private void ArrangePanes(double availableWidth)
        {
            var window = this.GetCurrentWindow();
            double visibleWidth = Math.Min(window.Bounds.Width, availableWidth);
            double viewportWidth = visibleWidth + 2 * ItemWidth;

            int visibleItems = (int)Math.Ceiling(viewportWidth / ItemWidth);
            int totalItems = visibleItems;

            totalItems = totalItems + (totalItems + 1) % 2;

            int diff = totalItems - base.Children.Count;

            if (diff > 0)
            {
                for (int n = 0; n < diff; n++)
                {
                    var pane = CreatePane();
                    base.Children.Add(pane);
                }
            }
            else
            {
                for (int n = 0; n < -diff; n++)
                {
                    base.Children.RemoveAt(base.Children.Count - 1);
                }
            }
        }

        private ContentControl CreatePane()
        {
            var pane = new ContentControl
            {
                UseLayoutRounding = true,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Stretch
            };
            pane.Tapped += OnPaneTapped;
            return pane;
        }
        #endregion
    }
}
