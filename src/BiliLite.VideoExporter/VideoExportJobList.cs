using System.Collections.Generic;

namespace BiliLite.VideoExporter
{
    /// <summary>
    /// 一次导出的任务列表。UWP 侧把任务写到 LocalFolder 的 json 文件后启动本进程，
    /// 由本进程顺序处理，全部成功且允许时才自动关闭窗口。
    /// </summary>
    public class VideoExportJobList
    {
        public bool CloseOnSuccess { get; set; } = true;

        public List<ConvertFileInfo> Items { get; set; } = new List<ConvertFileInfo>();
    }
}
