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

        public List<VirtualKey> Keys { get; set; }

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
                    Keys = new List<VirtualKey>() { VirtualKey.Space }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F11 }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F }
                },
                new ToggleFullscreenShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Enter }
                },
                new CancelFullscreenFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Escape }
                },
                new ToggleFullWindowShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F12 }
                },
                new ToggleFullWindowShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.W }
                },
                new LargePositionForwardShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.O }
                },
                new LargePositionForwardShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.P }
                },
                new AddVolumeFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Up }
                },
                new MinusVolumeFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Down }
                },
                new PositionBackFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Left }
                },
                new PositionBackPressShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Left }
                },
                new PositionForwardShortcutFunction()
                {
                    Keys = new List<VirtualKey>() {  VirtualKey.Right },
                    NeedKeyUp = true,
                },
                new PositionForwardPressShortcutFunction()
                {
                    Keys = new List<VirtualKey>() {  VirtualKey.Right },
                    Enable = false,
                },
                new StartHighRateSpeedPlayShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Right },
                },
                new CaptureVideoShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F10 }
                },
                new ToggleDanmakuDisplayShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F9 }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Z }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.N }
                },
                new GotoLastVideoFunction()
                {
                    Keys = new List<VirtualKey>() { (VirtualKey)188 }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.X }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.M }
                },
                new GotoNextVideoFunction()
                {
                    Keys = new List<VirtualKey>() { (VirtualKey)190 }
                },
                new ToggleDanmakuDisplayShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.D }
                },
                new SlowDownShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F1 }
                },
                new SlowDownShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { (VirtualKey)186 }
                },
                new FastUpShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F2 }
                },
                new FastUpShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { (VirtualKey)222 }
                },
                new RefreshShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.R}
                },
                new RefreshShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F5 }
                },
                new ToggleMiniWindowShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F8 }
                },
                new ToggleMiniWindowShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.T }
                },
                new SaveFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.S }
                },
            };
        }
    }
}
