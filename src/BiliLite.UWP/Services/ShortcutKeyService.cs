using System;
using BiliLite.Models.Functions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Pages;

namespace BiliLite.Services
{
    public class ShortcutKeyService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private List<IShortcutFunction> m_shortcutKeys;
        private List<IShortcutFunction> m_releaseMapsCache;
        private readonly Dictionary<string, DateTimeOffset> m_keyDownTimeCache;
        private IMainPage m_mainPage;
        private bool m_recording = false;
        private readonly IMapper m_mapper;
        private int m_pressActionDelayTime = 200;
        private readonly SettingSqlService m_settingSqlService;

        public ShortcutKeyService(IMapper mapper, SettingSqlService settingSqlService)
        {
            m_mapper = mapper;
            m_settingSqlService = settingSqlService;
            m_keyDownTimeCache = new Dictionary<string, DateTimeOffset>();
            m_releaseMapsCache = new List<IShortcutFunction>();
            LoadShortcutFunctions();
        }

        public event EventHandler<InputKey> OnRecordKeyDown;

        public event EventHandler OnRecordStoped;

        public List<IShortcutFunction> ShortcutFunctions => m_shortcutKeys;

        public int PressActionDelayTime
        {
            get => m_pressActionDelayTime;
            set
            {
                m_pressActionDelayTime = value;
                SettingService.SetValue(SettingConstants.ShortcutKey.PRESS_ACTION_DELAY_TIME,
                    m_pressActionDelayTime);
            }
        }

        public void SetMainPage(IMainPage mainPage)
        {
            m_mainPage = mainPage;
        }

        public void StartRecord()
        {
            Notify.ShowMessageToast("按键录制开始，在录制结束前请勿离开当前页面");
            m_recording = true;
        }

        public void StopRecord()
        {
            Notify.ShowMessageToast("按键录制结束");
            m_recording = false;
            OnRecordStoped?.Invoke(this,EventArgs.Empty);
        }

        public async void HandleKeyDown(VirtualKey virtualKey)
        {
            var key = new InputKey(virtualKey);
            _logger.Trace("key down: " + key);
            if (!m_keyDownTimeCache.TryAdd(key.ToString(), DateTimeOffset.Now)) return;

            if (m_recording)
            {
                OnRecordKeyDown?.Invoke(this, key);
                return;
            }

            foreach (var shortcutKeyFunction in m_shortcutKeys.Where(x => !x.NeedKeyUp && x.Enable))
            {
                var shortcutKeyCodes = shortcutKeyFunction.Keys;
                if (!shortcutKeyCodes.LastOrDefault().Equals(key)) continue;
                if (shortcutKeyCodes
                        .Select(keyCode => Window.Current.CoreWindow.GetKeyState(key.BoardKey))
                        .Any(keyState => !keyState.HasFlag(CoreVirtualKeyStates.Down)))
                {
                    continue;
                }

                if (shortcutKeyFunction.ReleaseFunction != null)
                {
                    if (!m_releaseMapsCache.Contains(shortcutKeyFunction))
                    {
                        shortcutKeyFunction.Canceled = false;
                        m_releaseMapsCache.Add(shortcutKeyFunction);
                        {
                            try
                            {
                                await Task.Delay(m_pressActionDelayTime + 1);
                                if (!shortcutKeyFunction.Canceled)
                                {
                                    var page = m_mainPage.CurrentPage;
                                    _logger.Trace("keyPressAction: " + shortcutKeyFunction.GetType().ToString());
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
                                // m_releaseMapsCache.Remove(shortcutKeyFunction);
                            }
                        };
                    }
                }
                else
                {
                    var page = m_mainPage.CurrentPage;
                    _logger.Trace("keyDownAction: " + shortcutKeyFunction.GetType().ToString());
                    shortcutKeyFunction.Action(page);
                }
            }
        }

        public void HandleKeyUp(VirtualKey virtualKey)
        {
            var key = new InputKey(virtualKey);
            _logger.Trace("key up: " + key);
            if (!m_keyDownTimeCache.TryGetValue(key.ToString(), out var keyDownTime)) return;
            m_keyDownTimeCache.Remove(key.ToString());

            if (m_recording)
            {
                if (m_keyDownTimeCache.Count == 0)
                {
                    StopRecord();
                }
                return;
            }

            var now = DateTimeOffset.Now;
            if (now - keyDownTime < TimeSpan.FromMilliseconds(m_pressActionDelayTime))
            {
                foreach (var shortcutKeyFunction in m_shortcutKeys.Where(x => x.NeedKeyUp && x.Enable))
                {
                    var shortcutKeyCodes = shortcutKeyFunction.Keys;
                    if (!shortcutKeyCodes.LastOrDefault().Equals(key)) continue;

                    if (shortcutKeyCodes.Take(shortcutKeyCodes.Count - 1)
                        .Select(keyCode => Window.Current.CoreWindow.GetKeyState(keyCode.BoardKey))
                        .Any(keyState => !keyState.HasFlag(CoreVirtualKeyStates.Down)))
                    {
                        continue;
                    }

                    {
                        var page = m_mainPage.CurrentPage;
                        _logger.Trace("keyUpAction: " + shortcutKeyFunction.GetType().ToString());
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
                        _logger.Trace("keyReleaseAction: " + shortcutKeyFunction.GetType().ToString());
                        shortcutKeyFunction.ReleaseFunction.Action(page);
                        m_releaseMapsCache.Remove(shortcutKeyFunction);
                    }
                }
            }
        }

        public void SetDefault()
        {
            m_shortcutKeys = DefaultShortcuts.GetDefaultShortcutFunctions();
        }

        public void RemoveShortcutFunction(string id)
        {
            var shortcutFunction = m_shortcutKeys.FirstOrDefault(x => x.Id == id);
            if (shortcutFunction == null) return;
            m_shortcutKeys.Remove(shortcutFunction);

            var shortcutFunctionModels = m_mapper.Map<List<ShortcutFunctionModel>>(m_shortcutKeys);

            m_settingSqlService.SetValue(SettingConstants.ShortcutKey.SHORTCUT_KEY_FUNCTIONS, shortcutFunctionModels);
        }

        public void UpdateShortcutFunction(ShortcutFunctionModel shortcutFunctionModel)
        {
            var shortcutFunction = m_shortcutKeys.FirstOrDefault(x => x.Id == shortcutFunctionModel.Id);
            if (shortcutFunction != null)
            {
                shortcutFunction.Keys = shortcutFunctionModel.Keys;
                shortcutFunction.NeedKeyUp = shortcutFunctionModel.NeedKeyUp;
                shortcutFunction.Enable = shortcutFunctionModel.Enable;
            }

            var shortcutFunctionModels = m_mapper.Map<List<ShortcutFunctionModel>>(m_shortcutKeys);

            m_settingSqlService.SetValue(SettingConstants.ShortcutKey.SHORTCUT_KEY_FUNCTIONS, shortcutFunctionModels);
        }

        private void LoadShortcutFunctions()
        {
            m_pressActionDelayTime = SettingService.GetValue(SettingConstants.ShortcutKey.PRESS_ACTION_DELAY_TIME,
                SettingConstants.ShortcutKey.DEFAULT_PRESS_ACTION_DELAY_TIME);
            try
            {
                var shortcutFunctionModels = m_settingSqlService.GetValue<List<ShortcutFunctionModel>>(SettingConstants.ShortcutKey.SHORTCUT_KEY_FUNCTIONS, null);

                if (shortcutFunctionModels == null)
                {
                    SetDefault();
                    return;
                }
                var shortcutFunctions = shortcutFunctionModels
                    .Select(x => ShortcutFunctionFactory.Map(x))
                    .ToList();
                m_shortcutKeys = shortcutFunctions;
            }
            catch (Exception ex)
            {
                SetDefault();
            }
        }
    }
}
