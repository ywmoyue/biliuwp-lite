using System;
using System.Threading;
using System.Threading.Tasks;
using BiliLite.Extensions.Notifications;

namespace BiliLite.Services
{
    /// <summary>
    /// BackgroundDownloader下载操作包装类
    /// </summary>
    public class BackgroundDownloadOperation : IDownloadOperation
    {
        private readonly Windows.Networking.BackgroundTransfer.DownloadOperation m_downloadOperation;
        private readonly BackgroundDownloadProgress m_progress;

        public BackgroundDownloadOperation(Windows.Networking.BackgroundTransfer.DownloadOperation downloadOperation)
        {
            m_downloadOperation = downloadOperation;
            m_progress = new BackgroundDownloadProgress(downloadOperation.Progress);
        }

        public Guid Guid => m_downloadOperation.Guid;

        public string Group => m_downloadOperation.Group;

        public IDownloadProgress Progress => m_progress;

        public Task StartAsync()
        {
            return m_downloadOperation.StartAsync().AsTask();
        }

        public Task AttachAsync(CancellationToken cancellationToken = default, IProgress<IDownloadOperation>? progress = null)
        {
            // 创建一个进度回调，将原始的DownloadOperation转换为IDownloadOperation
            var innerProgress = new Progress<Windows.Networking.BackgroundTransfer.DownloadOperation>(op =>
            {
                m_progress.UpdateProgress(op.Progress);
                progress?.Report(this);
            });

            // 调用原始的AttachAsync方法，并转换为Task
            return m_downloadOperation.AttachAsync().AsTask(cancellationToken, innerProgress);
        }

        public void Pause()
        {
            try
            {
                m_downloadOperation.Pause();
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("暂停失败");
            }
        }

        public void Resume()
        {
            m_downloadOperation.Resume();
        }

        /// <summary>
        /// 获取原始的DownloadOperation对象
        /// </summary>
        /// <returns></returns>
        public Windows.Networking.BackgroundTransfer.DownloadOperation GetRawOperation()
        {
            return m_downloadOperation;
        }

        /// <summary>
        /// BackgroundDownloader下载进度包装类
        /// </summary>
        private class BackgroundDownloadProgress : IDownloadProgress
        {
            private Windows.Networking.BackgroundTransfer.BackgroundDownloadProgress m_progress;

            public BackgroundDownloadProgress(Windows.Networking.BackgroundTransfer.BackgroundDownloadProgress progress)
            {
                m_progress = progress;
            }

            public ulong BytesReceived => m_progress.BytesReceived;

            public ulong TotalBytesToReceive => m_progress.TotalBytesToReceive;

            public DownloadStatus Status => MapStatus(m_progress.Status);

            /// <summary>
            /// 将BackgroundTransferStatus映射为DownloadStatus
            /// </summary>
            /// <param name="status"></param>
            /// <returns></returns>
            private DownloadStatus MapStatus(Windows.Networking.BackgroundTransfer.BackgroundTransferStatus status)
            {
                switch (status)
                {
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Idle:
                        return DownloadStatus.Idle;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Running:
                        return DownloadStatus.Running;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.PausedByApplication:
                        return DownloadStatus.PausedByApplication;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.PausedCostedNetwork:
                        return DownloadStatus.PausedCostedNetwork;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.PausedNoNetwork:
                        return DownloadStatus.PausedNoNetwork;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Completed:
                        return DownloadStatus.Completed;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Canceled:
                        return DownloadStatus.Canceled;
                    case Windows.Networking.BackgroundTransfer.BackgroundTransferStatus.Error:
                        return DownloadStatus.Error;
                    default:
                        return DownloadStatus.Idle;
                }
            }

            public void UpdateProgress(Windows.Networking.BackgroundTransfer.BackgroundDownloadProgress progress)
            {
                m_progress = progress;
            }
        }
    }
}