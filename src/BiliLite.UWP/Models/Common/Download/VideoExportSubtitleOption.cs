namespace BiliLite.Models.Common.Download
{
    /// <summary>
    /// 导出对话框里的字幕轨选项。Index 为 -1 表示不封装字幕。
    /// </summary>
    public class VideoExportSubtitleOption
    {
        public string Name { get; set; } = string.Empty;

        public int Index { get; set; } = -1;

        public override string ToString() => Name;
    }
}
