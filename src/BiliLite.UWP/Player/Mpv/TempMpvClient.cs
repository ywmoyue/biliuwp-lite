using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Player.Mpv;

using System;
using System.Runtime.InteropServices;
using System.Text;

public class TempMpvClient : IDisposable
{
    private IntPtr _libMpvDll;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    internal static extern IntPtr LoadLibrary(string dllToLoad);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool FreeLibrary(IntPtr hModule);
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint MpvClientApiVersion();  // 改为返回uint

    private MpvClientApiVersion _mpvClientApiVersion;

    public TempMpvClient(string mpvDllPath)
    {
        if (string.IsNullOrWhiteSpace(mpvDllPath))
            throw new ArgumentException("MPV DLL path cannot be null or empty", nameof(mpvDllPath));

        _libMpvDll = LoadLibrary(mpvDllPath);
        if (_libMpvDll == IntPtr.Zero)
            throw new Exception($"Failed to load MPV library from {mpvDllPath}");

        // 加载必要的函数指针
        _mpvClientApiVersion = (MpvClientApiVersion)GetDllType(typeof(MpvClientApiVersion), "mpv_client_api_version");
    }

    private object GetDllType(Type type, string name)
    {
        IntPtr address = GetProcAddress(_libMpvDll, name);
        if (address != IntPtr.Zero)
            return Marshal.GetDelegateForFunctionPointer(address, type);
        return null;
    }

    /// <summary>
    /// 检查是否是有效的mpv DLL
    /// </summary>
    public bool IsValidMpvDll()
    {
        try
        {
            // 检查几个关键函数是否存在
            return _mpvClientApiVersion != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 获取MPV版本信息
    /// </summary>
    public string GetVersionInfo()
    {
        if (!IsValidMpvDll())
            return "Invalid MPV DLL";

        try
        {
            StringBuilder sb = new StringBuilder();

            // 获取客户端API版本
            if (_mpvClientApiVersion != null)
            {
                uint version = _mpvClientApiVersion();  // 直接调用委托获取版本
                int major = (int)((version >> 16) & 0xFFFF);
                int minor = (int)(version & 0xFFFF);
                sb.AppendLine($"MPV Client API Version: {major}.{minor}");
            }

            return sb.ToString().Trim();
        }
        catch (Exception ex)
        {
            return $"Error getting version info: {ex.Message}";
        }
    }

    public void Dispose()
    {
        if (_libMpvDll != IntPtr.Zero)
        {
            FreeLibrary(_libMpvDll);
            _libMpvDll = IntPtr.Zero;
        }
    }

    ~TempMpvClient()
    {
        Dispose();
    }
}

