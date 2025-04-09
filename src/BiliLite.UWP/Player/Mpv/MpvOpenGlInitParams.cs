using System;
using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvOpenGlInitParams
{
    public MpvRender.OpenGlRenderContextCallback get_proc_address;
    public IntPtr get_proc_address_ctx;
    public IntPtr extra_exts;
}