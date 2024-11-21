using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Windows.System;
using WebSocket4Net;
using System.Timers;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Pages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Services
{
    public class PluginService
    {
        private readonly SettingSqlService m_settingSqlService;
        private List<WebSocketPlugin> m_plugins;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public PluginService(SettingSqlService settingSqlService)
        {
            m_settingSqlService = settingSqlService;
            m_plugins = m_settingSqlService.GetValue(SettingConstants.Other.PLUGIN_LIST, new List<WebSocketPlugin>());
        }

        public async Task Start()
        {
            foreach (var plugin in m_plugins)
            {
                await plugin.Start();
            }
        }

        public async Task Stop()
        {
            foreach (var plugin in m_plugins)
            {
                await plugin.Stop();
            }
        }

        public List<WebSocketPlugin> GetPlugins()
        {
            return m_plugins;
        }

        public async Task AddPlugin(WebSocketPlugin plugin)
        {
            var oldPlugin = m_plugins.FirstOrDefault(x => x.Name == plugin.Name);
            if (oldPlugin != null)
            {
                await oldPlugin.Stop();

                oldPlugin.CheckUrl = plugin.CheckUrl;
                oldPlugin.WakeProto = plugin.WakeProto;
                oldPlugin.WebSocketUrl = plugin.WebSocketUrl;

                try
                {
                    await oldPlugin.Start();
                }
                catch (Exception ex)
                {
                    _logger.Error("更新插件失败", ex);
                    throw ex;
                }

                m_settingSqlService.SetValue(SettingConstants.Other.PLUGIN_LIST, m_plugins);
                return;
            }
            try
            {
                await plugin.Start();
            }
            catch (Exception ex)
            {
                _logger.Error("添加插件失败", ex);
                throw ex;
            }

            m_plugins.Add(plugin);
            m_settingSqlService.SetValue(SettingConstants.Other.PLUGIN_LIST, m_plugins);
        }

        public async Task RemovePlugin(string name)
        {
            var plugin = m_plugins.FirstOrDefault(x => x.Name == name);
            if (plugin == null) return;
            await plugin.Stop();
            m_plugins.Remove(plugin);
        }
    }

    public class WebSocketPlugin
    {
        private WebSocket m_webSocket;
        private bool m_autoReconnect = true;
        private int m_retryCount = 0;
        private Timer m_clearRetryTimer;

        public WebSocketPlugin()
        {
            m_clearRetryTimer = new Timer(5000);
            m_clearRetryTimer.AutoReset = true;
            m_clearRetryTimer.Elapsed += ClearRetryTimer_Elapsed;
        }

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

        public async Task Start()
        {
            if (!await CheckIsEnable())
            {
                await WakePlugin();
            }
            await Connect();
            PluginCenter.BroadcastEvent += OnBroadcastEvent;
        }

        public async Task Stop()
        {
            PluginCenter.BroadcastEvent -= OnBroadcastEvent;
            await DisConnect();
        }

        private void OnBroadcastEvent(object sender,object msg)
        {
            // 将msg序列化为json字符串，通过ws发送出去
            var json = JsonConvert.SerializeObject(msg);
            if (m_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    m_webSocket.Send(json);
                }
                catch (Exception ex)
                {
                    // 处理发送消息时可能出现的异常
                    Debug.WriteLine($"Error sending message: {ex.Message}");
                }
            }
            else
            {
                // WebSocket连接不是打开状态，可能需要重连或记录错误
                Debug.WriteLine("WebSocket is not in the Open state.");
            }
        }

        private async Task Connect()
        {
            m_webSocket = new WebSocket(WebSocketUrl);
            m_webSocket.Opened += OnWebSocketOpened;
            m_webSocket.Closed += OnWebSocketClosed;
            m_webSocket.MessageReceived += OnWebSocketMessageReceived;
            m_webSocket.Error += OnWebSocketError;
            await OpenConnect();
        }

        private async Task OpenConnect()
        {
            try
            {
                await m_webSocket.OpenAsync();
            }
            catch (Exception ex)
            {
                await RetryConnect();
            }
        }

        private async Task DisConnect()
        {
            if (m_webSocket != null && m_webSocket.State == WebSocketState.Open)
            {
                m_webSocket.Opened -= OnWebSocketOpened;
                m_webSocket.Closed -= OnWebSocketClosed;
                m_webSocket.MessageReceived -= OnWebSocketMessageReceived;
                m_webSocket.Error -= OnWebSocketError;
                m_autoReconnect = false;
                await m_webSocket.CloseAsync();
                m_webSocket = null;
            }
        }

        private void ClearRetryTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_webSocket.State == WebSocketState.Open)
            {
                m_clearRetryTimer.Stop();
                m_retryCount = 0;
            }
        }

        private async Task RetryConnect()
        {
            var retryTime = 2000 * (m_retryCount);

            m_retryCount++;
            m_clearRetryTimer.Start();

            await Task.Delay(retryTime);

            await OpenConnect();
        }

        private void OnWebSocketOpened(object sender, EventArgs e)
        {
            // WebSocket 连接已打开
        }

        private async void OnWebSocketClosed(object sender, EventArgs e)
        {
            if (m_autoReconnect)
            {
                await RetryConnect();
            }
        }

        private void OnWebSocketMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            // 处理接收到的消息
            var msg = JsonConvert.DeserializeObject<PluginMessage>(e.Message);

            // TODO: 优化代码
            if (msg.Type == "action")
            {
                var mainPage = App.ServiceProvider.GetRequiredService<IMainPage>();
                var mainPageObj = mainPage as Page;
                mainPageObj.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (msg.Action == "ShowMessageToast")
                    {
                        // 解析参数
                        var message = msg.Params["message"]?.ToString();
                        var seconds = msg.Params["seconds"]?.ToObject<int>() ?? 2; // 默认值为2秒

                        // 调用Notify.ShowMessageToast方法
                        Notify.ShowMessageToast(message, seconds);
                    }
                    else if (msg.Action == "ExecuteAction")
                    {
                        var name = msg.Params["name"]?.ToString();
                        var shortcutKeyService = App.ServiceProvider.GetRequiredService<ShortcutKeyService>();
                        shortcutKeyService.ExecuteAction(name);
                    }
                    else if (msg.Action == "SetPosition")
                    {
                        var position = msg.Params["position"]?.ToObject<double>() ?? 0;
                        if (!(mainPage.CurrentPage is PlayPage page)) return;
                        page.SetPosition(position);
                    }
                });
            }
        }

        private void OnWebSocketError(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            // 处理 WebSocket 错误
            Debug.WriteLine($"WebSocket error: {e.Exception.Message}");
        }

        private async Task WakePlugin()
        {
            var success = await Launcher.LaunchUriAsync(new Uri(WakeProto));
            if (!success)
            {
                // 处理唤醒失败的情况
                throw new Exception("Failed to wake the plugin.");
            }
            // 等待一段时间让插件启动
            await Task.Delay(5000);
        }

        private async Task<bool> CheckIsEnable()
        {
            try
            {
                var response = await $"{CheckUrl}".GetAsync();
                // 假设如果插件启动了，这个请求会返回200 OK
                return response.StatusCode == 200;
            }
            catch (Exception)
            {
                // 如果请求失败，可能意味着插件没有启动
                return false;
            }
        }
    }

    public static class PluginCenter
    {
        public static event EventHandler<object> BroadcastEvent;

        public static void BroadcastPosition(object sender, double position)
        {
            BroadcastEvent?.Invoke(sender, new
            {
                @event = "PositionChanged",
                position,
            });
        }
    }

    public class PluginMessage
    {
        public string Type { get; set; }

        public string Action { get; set; }

        public JObject Params { get; set; }
    }
}
