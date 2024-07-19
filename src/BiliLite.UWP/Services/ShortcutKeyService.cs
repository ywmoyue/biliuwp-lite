using BiliLite.Models.Functions;
using System.Collections.Generic;
using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using BiliLite.Pages;

namespace BiliLite.Services
{
    public class ShortcutKeyService
    {
        private Dictionary<IShortcutFunction, List<VirtualKey>> m_shortcutKeyMaps;
        private IMainPage m_mainPage;

        public ShortcutKeyService()
        {
            m_shortcutKeyMaps = new Dictionary<IShortcutFunction, List<VirtualKey>>();
            // TODO: 读取设置
            m_shortcutKeyMaps.Add(new RefreshShortcutFunction(), new List<VirtualKey>() { VirtualKey.Control, VirtualKey.R });
            m_shortcutKeyMaps.Add(new RefreshShortcutFunction(), new List<VirtualKey>() { VirtualKey.F5 });
            m_shortcutKeyMaps.Add(new PlayPauseFunction(), new List<VirtualKey>() { VirtualKey.Space });
            m_shortcutKeyMaps.Add(new PositionBackFunction(), new List<VirtualKey>() { VirtualKey.Left });
            m_shortcutKeyMaps.Add(new AddVolumeFunction(), new List<VirtualKey>() { VirtualKey.Up });
            m_shortcutKeyMaps.Add(new MinusVolumeFunction(), new List<VirtualKey>() { VirtualKey.Down });
            m_shortcutKeyMaps.Add(new CancelFullscreenFunction(), new List<VirtualKey>() { VirtualKey.Escape });
            m_shortcutKeyMaps.Add(new SaveFunction(), new List<VirtualKey>() { VirtualKey.Control, VirtualKey.S });
        }

        public void SetMainPage(IMainPage mainPage)
        {
            m_mainPage = mainPage;
        }

        public void HandleKeyDown(VirtualKey key)
        {
            foreach (var (shortcutKeyFunction, shortcutKeyCodes) in m_shortcutKeyMaps)
            {
                if (shortcutKeyCodes.LastOrDefault() != key) continue;
                if (shortcutKeyCodes
                        .Select(keyCode => Window.Current.CoreWindow.GetKeyState(keyCode))
                        .Any(keyState => !keyState.HasFlag(CoreVirtualKeyStates.Down)))
                {
                    continue;
                }

                var page = m_mainPage.CurrentPage;
                shortcutKeyFunction.Action(page);
            }
        }
    }
}
