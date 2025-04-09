using System;
using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvEventProperty
{
    public IntPtr name;     // 属性名 (UTF-8)
    public int format;      // 数据格式
    public IntPtr data;     // 数据值
}