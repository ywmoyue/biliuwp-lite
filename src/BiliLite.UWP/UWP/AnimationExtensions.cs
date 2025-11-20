using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.UWP
{
    public static class AnimationExtensions
    {
        public static async Task Offset(this UIElement element,
            double offsetX,
            double offsetY,
            int duration,
            int delay = 0)
        {
            // 确保元素有TranslateTransform
            if (element.RenderTransform is not TranslateTransform translateTransform)
            {
                translateTransform = new TranslateTransform();
                element.RenderTransform = translateTransform;
            }

            var storyboard = new Storyboard();

            // 记录当前位置
            double currentX = translateTransform.X;
            double currentY = translateTransform.Y;

            // X轴动画
            var xAnimation = new DoubleAnimation
            {
                From = currentX,
                To = offsetX,
                Duration = TimeSpan.FromMilliseconds(duration),
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };

            // Y轴动画
            var yAnimation = new DoubleAnimation
            {
                From = currentY,
                To = offsetY,
                Duration = TimeSpan.FromMilliseconds(duration),
                BeginTime = TimeSpan.FromMilliseconds(delay)
            };

            Storyboard.SetTarget(xAnimation, translateTransform);
            Storyboard.SetTargetProperty(xAnimation, "X");

            Storyboard.SetTarget(yAnimation, translateTransform);
            Storyboard.SetTargetProperty(yAnimation, "Y");

            storyboard.Children.Add(xAnimation);
            storyboard.Children.Add(yAnimation);

            var tcs = new TaskCompletionSource<bool>();

            storyboard.Completed += (s, e) =>
            {
                // 动画完成后保持最终位置
                translateTransform.X = offsetX;
                translateTransform.Y = offsetY;
                tcs.SetResult(true);
            };

            storyboard.Begin();

            await tcs.Task;
        }
    }

    public enum EasingType
    {
        Default,
        Linear,
        Quadratic,
        Cubic,
        Quartic,
        Quintic,
        Sine,
        Exponential,
        Circle,
        Elastic,
        Bounce,
        Back
    }
}
