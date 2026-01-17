using System;
using System.Threading.Tasks;

namespace BiliLite.Services
{
    public class BackgroundDownloadTask : IDownloadTask
    {
        private readonly BackgroundDownloadOperation m_downloadOperation;
        
        public BackgroundDownloadTask(BackgroundDownloadOperation downloadOperation)
        {
            m_downloadOperation = downloadOperation;
        }
        
        public Guid Guid => m_downloadOperation.Guid;
        
        public ulong BytesReceived => m_downloadOperation.Progress.BytesReceived;
        
        public ulong TotalBytesToReceive => m_downloadOperation.Progress.TotalBytesToReceive;
        
        public DownloadStatus Status => m_downloadOperation.Progress.Status;
        
        public string Group => m_downloadOperation.Group;
        
        public Task PauseAsync()
        {
            m_downloadOperation.Pause();
            return Task.CompletedTask;
        }
        
        public Task ResumeAsync()
        {
            m_downloadOperation.Resume();
            return Task.CompletedTask;
        }
    }
}