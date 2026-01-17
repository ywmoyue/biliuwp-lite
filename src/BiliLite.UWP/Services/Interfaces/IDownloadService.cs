using System.Collections.Generic;
using System.Threading.Tasks;
using BiliLite.Models.Download;

namespace BiliLite.Services
{
    public interface IDownloadService
    {
        Task AddDownload(DownloadInfo downloadInfo);
        Task PauseDownload(string guid);
        Task ResumeDownload(string guid);
        Task<IEnumerable<IDownloadOperation>> GetCurrentDownloads();
    }
}