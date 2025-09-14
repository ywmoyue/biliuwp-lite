using BiliLite.Controls;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common.Player;

namespace BiliLite.Services
{
    public class PlayerToastService
    {
        private PlayerControl m_playerControl;
        private int[] m_bottomList = new[] { 170, 240, 310, 380, 450, 520 };
        private readonly Dictionary<string, PlayerToast> m_showPlayerToasts = new Dictionary<string, PlayerToast>();
        private readonly Dictionary<string, Timer> m_showPlayerToastTimers = new Dictionary<string, Timer>();
        public const string VOLUME_KEY = "Volume";
        public const string BRIGHTNESS_KEY = "Brightness";
        public const string PROGRESS_KEY = "Progress";
        public const string MSG_KEY = "Msg";
        public const string ACCELERATING_KEY = "Accelerating";
        public const string SPEED_KEY = "Speed";
        private readonly IServiceProvider m_serviceProvider;

        public PlayerToastService(IServiceProvider serviceProvider)
        {
            m_serviceProvider = serviceProvider;
        }

        public void Init(PlayerControl control)
        {
            m_playerControl = control;
        }

        public async void KeepStart(string key, string msg)
        {
            if (m_showPlayerToasts.TryGetValue(key, out var toast))
            {
                return;
            }

            var newToast = m_serviceProvider.GetRequiredService<PlayerToast>();
            newToast.Height = 80;
            newToast.Width = 200;
            newToast.Text = msg;
            Canvas.SetLeft(newToast, 0);
            double distanceFromBottom = m_bottomList[m_showPlayerToasts.Count];
            while (m_playerControl.ActualHeight == 0)
            {
                await Task.Delay(500);
            }

            Canvas.SetTop(newToast, m_playerControl.ActualHeight - distanceFromBottom);

            m_showPlayerToasts.Add(key, newToast);
            m_playerControl.PlayerToastContainer.Children.Add(newToast);
            newToast.Show();
        }

        public async void KeepClose(string key)
        {
            if (!m_showPlayerToasts.TryGetValue(key, out var toast)) return;
            m_playerControl.PlayerToastContainer.Children.Remove(toast);
            m_showPlayerToasts.Remove(key);
        }

        /// <summary>
        /// 显示Toast信息
        /// </summary>
        /// <param name="key">ToastId</param>
        /// <param name="msg">显示信息</param>
        /// <param name="showTime">显示时间(毫秒) 默认2000</param>
        /// <param name="seg">用于SponsorBlock功能，可选</param>
        public async void Show(string key, string msg, long showTime = 2000, PlayerSkipItem seg = null, bool showSkipButton = false)
        {
            if (m_showPlayerToasts.TryGetValue(key, out var toast))
            {
                toast.Text = msg;
                var timer = m_showPlayerToastTimers[key];
                timer.Stop();
                timer.Start();
                return;
            }

            var newToast = m_serviceProvider.GetRequiredService<PlayerToast>();
            newToast.Height = 80;
            newToast.Width = 200;
            newToast.Text = msg;
            if (seg != null)
            {
                newToast.Width = 300;
                if (showSkipButton) newToast.ShowSkipButton = true;
                newToast.IconBrush = seg.Brush;
                newToast.SkipButtonClick += (_, _) => m_playerControl.SetPosition(seg.End);
            }

            Canvas.SetLeft(newToast, 0);
            double distanceFromBottom = m_bottomList[m_showPlayerToasts.Count];
            while (m_playerControl.ActualHeight == 0)
            {
                await Task.Delay(500);
            }
            Canvas.SetTop(newToast, m_playerControl.ActualHeight - distanceFromBottom);

            m_showPlayerToasts.Add(key, newToast);
            m_playerControl.PlayerToastContainer.Children.Add(newToast);
            newToast.Show();

            var newTimer = new Timer();
            newTimer.Interval = showTime;
            newTimer.Elapsed += async (_, _) => await Hide(key, newToast, newTimer);
            newToast.HideToast += async (_, _) => await Hide(key, newToast, newTimer);
            m_showPlayerToastTimers.Add(key, newTimer);
            newTimer.Start();
        }

        public async Task Hide(string key, PlayerToast toast, Timer timer)
        {
            await m_playerControl.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                timer.Stop();
                m_playerControl.PlayerToastContainer.Children.Remove(toast);
                m_showPlayerToasts.Remove(key);
                m_showPlayerToastTimers.Remove(key);
            });
        }
    }
}
