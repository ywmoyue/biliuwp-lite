﻿using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class ToggleSubtitleShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "切换字幕开关";

        public override async Task Action(object param)
        {
            if (!(param is IPlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.ToggleSubtitle();
        }
    }
}
