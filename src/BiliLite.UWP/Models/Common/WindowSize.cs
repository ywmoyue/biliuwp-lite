namespace BiliLite.Models.Common
{
    /// <summary>
    /// 窗口尺寸数据模型(平台无关,避免直接依赖平台 SDK 的 Size 类型)
    /// </summary>
    public struct WindowSize
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public WindowSize(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}