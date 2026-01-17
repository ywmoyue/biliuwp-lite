using System;
using System.Threading;
using System.Threading.Tasks;

namespace BiliLite.Services
{
    /// <summary>
    /// 下载操作状态
    /// </summary>
    public enum DownloadStatus
    {
        Idle,
        Running,
        PausedByApplication,
        PausedCostedNetwork,
        PausedNoNetwork,
        Completed,
        Canceled,
        Error
    }
    
    /// <summary>
    /// 下载操作进度
    /// </summary>
    public interface IDownloadProgress
    {
        ulong BytesReceived { get; }
        ulong TotalBytesToReceive { get; }
        DownloadStatus Status { get; }
    }
    
    /// <summary>
    /// 下载操作接口
    /// </summary>
    public interface IDownloadOperation
    {
        Guid Guid { get; }
        string Group { get; }
        IDownloadProgress Progress { get; }
        
        Task StartAsync();
        Task AttachAsync(CancellationToken cancellationToken = default, IProgress<IDownloadOperation>? progress = null);
        void Pause();
        void Resume();
    }
}