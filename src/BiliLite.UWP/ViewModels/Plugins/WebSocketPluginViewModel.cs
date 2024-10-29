using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Plugins
{
    public class WebSocketPluginViewModel : BaseViewModel
    {        
        /// <summary>
        /// 插件的唤醒协议, 例如 bilibili://
        /// </summary>
        public string WakeProto { get; set; }

        /// <summary>
        /// 连接插件的WebSocket地址
        /// </summary>
        public string WebSocketUrl { get; set; }

        /// <summary>
        /// 检查插件是否已启动的Http地址
        /// </summary>
        public string CheckUrl { get; set; }

        /// <summary>
        /// 插件名称
        /// </summary>
        public string Name { get; set; }
    }
}
