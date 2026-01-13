using System;
using System.Threading.Tasks;

namespace BiliLite.Services
{
    public interface IDownloadTask
    {
        Guid Guid { get; }
        ulong BytesReceived { get; }
        ulong TotalBytesToReceive { get; }
        DownloadStatus Status { get; }
        string Group { get; }
        
        Task PauseAsync();
        Task ResumeAsync();
    }
}