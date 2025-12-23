using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls
{
    public class FakeFrame : Frame
    {
        public bool IsReleased { get; set; }
        public Type CurrentPageType { get; set; }
        public object CurrentPageParameter { get; set; }
        public List<(Type PageType, object Parameter)> BackStackInfo { get; set; }
    }
}
