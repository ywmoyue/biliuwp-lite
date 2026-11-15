using BiliLite.ViewModels.Download;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    /// <summary>
    /// 添加下载时的右下角进度悬浮窗。
    /// 下载对话框点「开始」后会立即关闭，逐集读取地址、加入下载列表的过程在这里显示。
    /// </summary>
    public sealed partial class DownloadAddToast : UserControl
    {
        private static DownloadAddToast s_instance;

        private readonly Popup m_popup;
        private int m_autoCloseToken;

        public ObservableCollection<DownloadAddBatchViewModel> Batches { get; } = new();

        public DownloadAddToast()
        {
            InitializeComponent();
            m_popup = new Popup
            {
                Child = this
            };
            Width = Window.Current.Bounds.Width;
            Height = Window.Current.Bounds.Height;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// 新开一个添加批次并显示悬浮窗
        /// </summary>
        public static DownloadAddBatchViewModel ShowBatch(string title, IEnumerable<DownloadEpisodeItemViewModel> items)
        {
            var batch = new DownloadAddBatchViewModel { Title = title };
            if (items != null)
            {
                foreach (var item in items)
                {
                    if (item != null) batch.Items.Add(item);
                }
            }

            var toast = s_instance ??= new DownloadAddToast();
            toast.AddBatch(batch);
            return batch;
        }

        /// <summary>
        /// 批次处理完成，全部批次都完成后自动关闭悬浮窗
        /// </summary>
        public static void FinishBatch(DownloadAddBatchViewModel batch)
        {
            if (batch == null) return;
            batch.MarkFinished();
            s_instance?.ScheduleAutoClose();
        }

        private void AddBatch(DownloadAddBatchViewModel batch)
        {
            // 有新的批次时取消上一次排队的自动关闭
            m_autoCloseToken++;
            Batches.Add(batch);
            if (!m_popup.IsOpen) m_popup.IsOpen = true;
        }

        private void ScheduleAutoClose()
        {
            if (Batches.Any(x => !x.Finished)) return;

            var hasFailure = Batches.Any(x => x.HasFailure);
            var token = ++m_autoCloseToken;
            _ = AutoCloseAsync(token, hasFailure ? 10 : 4);
        }

        private async Task AutoCloseAsync(int token, int seconds)
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            if (token != m_autoCloseToken) return;
            Close();
        }

        private void Close()
        {
            m_autoCloseToken++;
            m_popup.IsOpen = false;
            Batches.Clear();
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Width = e.Size.Width;
            Height = e.Size.Height;
        }
    }
}
