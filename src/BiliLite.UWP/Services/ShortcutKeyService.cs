using System;
using BiliLite.Models.Functions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using BiliLite.Pages;

namespace BiliLite.Services
{
    public class ShortcutKeyService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private List<IShortcutFunction> m_shortcutKeys;
        private List<IShortcutFunction> m_releaseMapsCache;
        private readonly Dictionary<VirtualKey, DateTimeOffset> m_keyDownTimeCache;
        private IMainPage m_mainPage;

        public ShortcutKeyService()
        {
            m_keyDownTimeCache = new Dictionary<VirtualKey, DateTimeOffset>();
            m_releaseMapsCache = new List<IShortcutFunction>();
            m_shortcutKeys = DefaultShortcuts.GetDefaultShortcutFunctions();
        }

        public void SetMainPage(IMainPage mainPage)
        {
            m_mainPage = mainPage;
        }

        public async void HandleKeyDown(VirtualKey key)
        {
            m_keyDownTimeCache.TryAdd(key, DateTimeOffset.Now);
            foreach (var shortcutKeyFunction in m_shortcutKeys.Where(x => !x.NeedKeyUp))
            {
                var shortcutKeyCodes = shortcutKeyFunction.Keys;
                if (shortcutKeyCodes.LastOrDefault() != key) continue;
                if (shortcutKeyCodes
                        .Select(keyCode => Window.Current.CoreWindow.GetKeyState(keyCode))
                        .Any(keyState => !keyState.HasFlag(CoreVirtualKeyStates.Down)))
                {
                    continue;
                }

                if (shortcutKeyFunction.ReleaseFunction != null)
                {
                    shortcutKeyFunction.Canceled = false;
                    if (!m_releaseMapsCache.Contains(shortcutKeyFunction))
                    {
                        m_releaseMapsCache.Add(shortcutKeyFunction);
                        {
                            try
                            {
                                await Task.Delay(250);
                                if (!shortcutKeyFunction.Canceled)
                                {
                                    var page = m_mainPage.CurrentPage;
                                    Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                                        () => { shortcutKeyFunction.Action(page); });
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Warn("按住行为执行失败", ex);
                            }
                            finally
                            {
                                m_releaseMapsCache.Remove(shortcutKeyFunction);
                            }
                        };
                    }
                }
                else
                {
                    var page = m_mainPage.CurrentPage;
                    shortcutKeyFunction.Action(page);
                }
            }
        }

        public void HandleKeyUp(VirtualKey key)
        {
            if (!m_keyDownTimeCache.TryGetValue(key, out var keyDownTime)) return;
            m_keyDownTimeCache.Remove(key);
            var now = DateTimeOffset.Now;
            if (now - keyDownTime < TimeSpan.FromMilliseconds(200))
            {
                foreach (var shortcutKeyFunction in m_shortcutKeys.Where(x => !x.NeedKeyUp))
                {
                    var shortcutKeyCodes = shortcutKeyFunction.Keys;
                    if (shortcutKeyCodes.LastOrDefault() != key) continue;

                    if (shortcutKeyCodes.Take(shortcutKeyCodes.Count - 1)
                        .Select(keyCode => Window.Current.CoreWindow.GetKeyState(keyCode))
                        .Any(keyState => !keyState.HasFlag(CoreVirtualKeyStates.Down)))
                    {
                        continue;
                    }

                    {
                        var page = m_mainPage.CurrentPage;
                        shortcutKeyFunction.Action(page);
                    }
                }

                foreach (var shortcutKeyFunction in m_releaseMapsCache)
                {
                    if (shortcutKeyFunction.Keys.Contains(key))
                    {
                        shortcutKeyFunction.Canceled = true;
                    }
                }
            }
            else
            {
                foreach (var (shortcutKeyFunction, shortcutKeyCodes) in
                         m_releaseMapsCache.ToDictionary(x => x, y => y.Keys))
                {
                    if (shortcutKeyCodes.Contains(key))
                    {
                        shortcutKeyFunction.Canceled = true;
                        var page = m_mainPage.CurrentPage;
                        shortcutKeyFunction.ReleaseFunction.Action(page);
                        m_releaseMapsCache.Remove(shortcutKeyFunction);
                    }
                }
            }
        }
    }
}
