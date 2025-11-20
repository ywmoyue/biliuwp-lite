using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Player.WebPlayer
{
    public sealed partial class WebPlayerToolbarControl : UserControl
    {
        private double zoomFactor = 1;
        private BaseWebPlayer m_baseWebPlayer;
        private Point m_lastPosition;
        private bool m_isManipulating;

        public WebPlayerToolbarControl()
        {
            this.InitializeComponent();
        }

        public event EventHandler ExitToolbar;

        public void SetPlayer(BaseWebPlayer webPlayer)
        {
            m_baseWebPlayer = webPlayer;
        }

        private void BtnFlipVertical_OnClick(object sender, RoutedEventArgs e)
        {
            m_baseWebPlayer?.FlipVertical();
        }

        private void BtnFlipHorizontal_OnClick(object sender, RoutedEventArgs e)
        {
            m_baseWebPlayer?.FlipHorizontal();
        }

        private void BtnZoomOut_OnClick(object sender, RoutedEventArgs e)
        {
            zoomFactor -= 0.1;
            m_baseWebPlayer?.Zoom(zoomFactor);
        }

        private void BtnZoomIn_OnClick(object sender, RoutedEventArgs e)
        {
            zoomFactor += 0.1;
            m_baseWebPlayer?.Zoom(zoomFactor);
        }

        private void BtnOrigin_OnClick(object sender, RoutedEventArgs e)
        {
            m_baseWebPlayer?.ResetTransforms();
        }

        private void BtnExit_OnClick(object sender, RoutedEventArgs e)
        {
            ExitToolbar?.Invoke(this, EventArgs.Empty);
        }
        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            m_lastPosition = e.Position;
            m_isManipulating = true;
        }

        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (m_baseWebPlayer == null) return;

            // 处理平移
            if (e.Delta.Translation.X != 0 || e.Delta.Translation.Y != 0)
            {
                var currentPosition = e.Position;
                var deltaX = currentPosition.X - m_lastPosition.X;
                var deltaY = currentPosition.Y - m_lastPosition.Y;

                m_baseWebPlayer?.Move(deltaX, deltaY);
                m_lastPosition = currentPosition;
            }

            // 处理缩放
            if (e.Delta.Scale != 1.0)
            {
                zoomFactor *= e.Delta.Scale;
                // 限制缩放范围
                zoomFactor = Math.Max(0.1, Math.Min(zoomFactor, 5.0));
                m_baseWebPlayer?.Zoom(zoomFactor);
            }
        }

        private void Grid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (m_baseWebPlayer == null) return;

            var pointerPoint = e.GetCurrentPoint((UIElement)sender);
            var delta = pointerPoint.Properties.MouseWheelDelta;

            if (delta > 0)
            {
                zoomFactor += 0.1;
            }
            else if (delta < 0)
            {
                zoomFactor -= 0.1;
            }

            zoomFactor = Math.Max(0.1, Math.Min(zoomFactor, 5.0));

            m_baseWebPlayer?.Zoom(zoomFactor);

            e.Handled = true;
        }

        private void BtnPIP_OnClick(object sender, RoutedEventArgs e)
        {
            m_baseWebPlayer?.TogglePictureInPicture();
        }
    }
}
