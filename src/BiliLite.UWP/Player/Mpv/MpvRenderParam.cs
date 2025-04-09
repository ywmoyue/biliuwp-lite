using System;
using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvRenderParam
{
    public MpvRenderParamType type;
    public IntPtr data;
}