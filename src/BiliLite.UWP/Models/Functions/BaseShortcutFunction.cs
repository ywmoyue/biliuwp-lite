using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace BiliLite.Models.Functions
{
    public abstract class BaseShortcutFunction : IShortcutFunction
    {
        public BaseShortcutFunction()
        {
            TypeName = GetType().ToString();
        }

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string TypeName { get; }

        public abstract string Name { get; }

        public bool Enable { get; set; } = true;

        public abstract Task Action(object param);

        public bool NeedKeyUp { get; set; } = false;

        public bool Canceled { get; set; }

        public List<InputKey> Keys { get; set; }

        public virtual IShortcutFunction ReleaseFunction { get; } = null;
    }

    public static class DefaultShortcuts
    {
        public static List<IShortcutFunction> GetDefaultShortcutFunctions()
        {
            return new List<IShortcutFunction>()
            {
                new PlayPauseFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Space) }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F11) }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F) }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Enter) }
                },
                new CancelFullscreenFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Escape) }
                },
                new ToggleFullWindowShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F12) }
                },
                new ToggleFullWindowShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.W) }
                },
                new LargePositionForwardShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.O) }
                },
                new LargePositionForwardShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.P) }
                },
                new AddVolumeFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Up) }
                },
                new MinusVolumeFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Down) }
                },
                new PositionBackFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Left) }
                },
                new PositionBackPressShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Left) }
                },
                new PositionForwardShortcutFunction()
                {
                    Keys = new List<InputKey>() {  new InputKey(VirtualKey.Right) },
                    NeedKeyUp = true,
                },
                new PositionForwardPressShortcutFunction()
                {
                    Keys = new List<InputKey>() {  new InputKey(VirtualKey.Right) },
                    Enable = false,
                },
                new StartHighRateSpeedPlayShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Right) },
                },
                new CaptureVideoShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F10) }
                },
                new ToggleDanmakuDisplayShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F9) }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Z) }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.N) }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey((VirtualKey)188) }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.X) }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.M) }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<InputKey>() { new InputKey((VirtualKey)190) }
                },
                new ToggleDanmakuDisplayShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.D) }
                },
                new SlowDownShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F1) }
                },
                new SlowDownShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey((VirtualKey)186) }
                },
                new FastUpShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F2) }
                },
                new FastUpShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey((VirtualKey)222) }
                },
                new RestartAppFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Menu), new InputKey(VirtualKey.R) }
                },
                new RefreshShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Control), new InputKey(VirtualKey.R) }
                },
                new RefreshShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F5) }
                },
                new ToggleMiniWindowShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F8) }
                },
                new ToggleMiniWindowShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.T) }
                },
                new ToggleSubtitleShortcutFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.F6) }
                },
                new SaveFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Control), new InputKey(VirtualKey.S) }
                },
                new NewTapFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Control), new InputKey(VirtualKey.T) }
                },
                new CloseTapFunction()
                {
                    Keys = new List<InputKey>() { new InputKey(VirtualKey.Control), new InputKey(VirtualKey.W) }
                },
            };
        }
    }
}
