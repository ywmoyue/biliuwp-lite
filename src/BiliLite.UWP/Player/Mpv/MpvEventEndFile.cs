using System;
using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

[StructLayout(LayoutKind.Sequential)]
public struct MpvEventEndFile
{
    public int reason;
    public int error;
    public IntPtr playlist_entry_id;
}