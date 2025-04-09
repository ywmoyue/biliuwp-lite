using System;
using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvEventLogMessage
{
    public IntPtr prefix;    // 日志前缀（UTF-8字符串）
    public IntPtr level;     // 日志级别（UTF-8字符串）
    public IntPtr text;      // 日志内容（UTF-8字符串）
}