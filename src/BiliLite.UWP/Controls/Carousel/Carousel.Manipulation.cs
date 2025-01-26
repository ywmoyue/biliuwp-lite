using System;

using Windows.UI.Xaml.Input;

namespace BiliLite.Controls
{
    partial class Carousel
    {
        private bool _isBusy = false;

        #region Position
        public double Position
        {
            get { return _panel.GetTranslateX(); }
            set
            {
                _panel.TranslateX(value);
            }
        }
        #endregion

        #region Offset
        public double Offset
        {
            get
            {
                double position = Position % ItemWidth;
                if (Math.Sign(position) > 0)
                {
                    return ItemWidth - position;
                }
                return -position;
            }
        }
        #endregion

        private int _direction = 0;

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _direction = Math.Sign(e.Delta.Translation.X);
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (_direction > 0)
            {
                MoveBack();
            }
            else
            {
                MoveForward();
            }
        }

        private async void AnimateNext(double duration = 150)
        {
            _isBusy = true;
            double delta = ItemWidth - Offset;
            double position = Position - delta;

            await _panel.AnimateXAsync(position, duration);

            Index = (int)(-position / ItemWidth);
            _isBusy = false;
        }

        private async void AnimatePrev(double duration = 150)
        {
            _isBusy = true;
            double delta = Offset;
            double position = Position + delta;

            await _panel.AnimateXAsync(position, duration);

            Index = (int)(-position / ItemWidth);
            _isBusy = false;
        }
    }
}
