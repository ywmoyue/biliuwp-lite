namespace BiliLite.Models.Common.Download
{
    /// <summary>
    /// 一次导出任务：输出文件 + 该文件要用的导出选项。
    /// 批量导出时多个任务会一起交给导出器进程顺序处理。
    /// </summary>
    public class VideoExportJob
    {
        /// <summary>显示在导出窗口标题里的名字</summary>
        public string Title { get; set; }

        /// <summary>输出文件的绝对路径</summary>
        public string OutFile { get; set; }

        public VideoExportOptions Options { get; set; } = new();
    }
}
