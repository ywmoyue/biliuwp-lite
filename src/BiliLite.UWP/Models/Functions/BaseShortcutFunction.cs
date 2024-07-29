using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;

namespace BiliLite.Models.Functions
{
    public abstract class BaseShortcutFunction : IShortcutFunction
    {
        public abstract string Name { get; }

        public abstract Task Action(object param);

        public bool NeedKeyUp { get; set; } = false;

        public bool Canceled { get; set; }

        public List<VirtualKey> Keys { get; set; }

        public double DelayTime { get; set; }

        public virtual IShortcutFunction ReleaseFunction { get; } = null;
    }

    public static class DefaultShortcuts
    {
        public static List<IShortcutFunction> GetDefaultShortcutFunctions()
        {
            return new List<IShortcutFunction>()
            {
                new RefreshShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.R}
                },
                new RefreshShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.F5 }
                },
                new PlayPauseFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Space }
                },
                new PositionBackFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Left }
                },
                new AddVolumeFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Up }
                },
                new MinusVolumeFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Down }
                },
                new CancelFullscreenFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Escape }
                },
                new SaveFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.S }
                },
                new PositionForwardShortcutFunction()
                {
                    Keys = new List<VirtualKey>() {  VirtualKey.Right },
                    NeedKeyUp = true,
                },
                new StartHighRateSpeedPlayShortcutFunction()
                {
                    Keys = new List<VirtualKey>() { VirtualKey.Right },
                    DelayTime = 200,
                },
            };
        }
    }
}
