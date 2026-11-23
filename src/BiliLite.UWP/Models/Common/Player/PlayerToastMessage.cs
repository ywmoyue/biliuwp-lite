namespace BiliLite.Models.Common.Player
{
    /// <summary>
    /// 播放器内部 Toast 消息
    /// </summary>
    public class PlayerToastMessage
    {
        public PlayerToastMessage(string text)
        {
            Text = text;
        }

        /// <summary>
        /// 提示内容
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 是否显示「关闭」按钮，仅关闭当前这次提示
        /// </summary>
        public bool ShowCloseButton { get; set; }
    }
}
