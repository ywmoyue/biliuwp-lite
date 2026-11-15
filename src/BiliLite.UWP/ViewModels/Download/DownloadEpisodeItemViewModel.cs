using BiliLite.ViewModels.Common;
using PropertyChanged;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.ViewModels.Download
{
    public class DownloadEpisodeItemViewModel : BaseViewModel
    {
        public int Index { get; set; }
        public string EpisodeID { get; set; }
        public string AVID { get; set; }
        public string BVID { get; set; }
        public string CID { get; set; }
        public string Title { get; set; }
        public bool ShowBadge { get; set; } = false;
        public string Badge { get; set; }

        /// <summary>
        /// 0=等待下载，1=读取视频链接中，2=正在下载，3=已下载，99=下载失败
        /// </summary>
        public int State { get; set; }

        public string ErrorMessage { get; set; }
        public bool IsPreview { get; set; } = false;

        /// <summary>
        /// 添加下载时的状态文案，用于右下角进度悬浮窗
        /// </summary>
        [DependsOn(nameof(State), nameof(ErrorMessage))]
        public string StateText
        {
            get
            {
                switch (State)
                {
                    case 0:
                        return "等待中";
                    case 1:
                        return "读取地址中";
                    case 2:
                        return "已加入下载列表";
                    case 3:
                        return "已下载，跳过";
                    case 99:
                        return string.IsNullOrEmpty(ErrorMessage) ? "添加失败" : $"添加失败：{ErrorMessage}";
                    default:
                        return string.Empty;
                }
            }
        }

        [DependsOn(nameof(State))]
        public bool IsStateError => State == 99;
    }
}
