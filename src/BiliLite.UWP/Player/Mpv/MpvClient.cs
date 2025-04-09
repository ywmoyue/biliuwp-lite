using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace BiliLite.Player.Mpv
{
    public class MpvClient : IDisposable
    {
        private const int MpvFormatString = 1; 
        private const int MpvEventError = 2; 
        private const int MpvEventLogMessage = 11;
        private IntPtr _libMpvDll;
        private IntPtr _mpvHandle;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreate();
        private MpvCreate _mpvCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvInitialize(IntPtr mpvHandle);
        private MpvInitialize _mpvInitialize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvCommand(IntPtr mpvHandle, IntPtr strings);
        private MpvCommand _mpvCommand;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvTerminateDestroy(IntPtr mpvHandle);
        private MpvTerminateDestroy _mpvTerminateDestroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetOption(IntPtr mpvHandle, byte[] name, int format, ref long data);
        private MpvSetOption _mpvSetOption;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int MpvSetOptionStringFunc(IntPtr mpvHandle, byte[] name, byte[] value);
        public MpvSetOptionStringFunc MpvSetOptionString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvGetPropertystringFunc(IntPtr mpvHandle, byte[] name, int format, ref IntPtr data);
        private MpvGetPropertystringFunc MpvGetPropertyString;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int MpvSetPropertyFunc(IntPtr mpvHandle, byte[] name, int format, ref byte[] data);
        public MpvSetPropertyFunc MpvSetProperty;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvFree(IntPtr data);
        private MpvFree _mpvFree;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MpvEventCallback(IntPtr userData);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvCreateClient(IntPtr mpvHandle, string name);
        private MpvCreateClient _mpvCreateClient;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvObserveProperty(IntPtr mpvHandle, ulong replyUserData, byte[] name, int format);
        private MpvObserveProperty _mpvObserveProperty;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvRequestEvent(IntPtr mpvHandle, int eventId, int enable);
        private MpvRequestEvent _mpvRequestEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MpvWaitEvent(IntPtr mpvHandle, double timeout);
        private MpvWaitEvent _mpvWaitEvent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvSetWakeupCallback(IntPtr mpvHandle, MpvEventCallback callback, IntPtr userData);
        private MpvSetWakeupCallback _mpvSetWakeupCallback;

        public IntPtr MpvHandle => _mpvHandle;

        public IntPtr LibMpvDll => _libMpvDll;

        public double Duration
        {
            get
            {
                if (_mpvHandle == IntPtr.Zero)
                    return 0;

                IntPtr durationPtr = IntPtr.Zero;
                try
                {
                    // 获取 duration 属性（mpv 返回的是字符串格式的浮点数）
                    MpvGetPropertyString(
                        _mpvHandle,
                        MpvUtilsExtensions.GetUtf8Bytes("duration"),
                        MpvFormatString,
                        ref durationPtr
                    );

                    if (durationPtr != IntPtr.Zero)
                    {
                        string durationStr = Marshal.PtrToStringAnsi(durationPtr);
                        if (double.TryParse(durationStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double duration))
                        {
                            return duration;
                        }
                    }
                    return 0;
                }
                finally
                {
                    if (durationPtr != IntPtr.Zero)
                    {
                        _mpvFree(durationPtr); // 释放内存
                    }
                }
            }
        }

        public event EventHandler FileLoaded;

        private object GetDllType(Type type, string name)
        {
            IntPtr address = GetProcAddress(_libMpvDll, name);
            if (address != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer(address, type);
            return null;
        }

        private void LoadMpvDynamic()
        {
            _libMpvDll = LoadLibrary(MpvConstants.MpvPath);
            _mpvCreate = (MpvCreate)GetDllType(typeof(MpvCreate), "mpv_create");
            _mpvInitialize = (MpvInitialize)GetDllType(typeof(MpvInitialize), "mpv_initialize");
            _mpvTerminateDestroy = (MpvTerminateDestroy)GetDllType(typeof(MpvTerminateDestroy), "mpv_terminate_destroy");
            _mpvCommand = (MpvCommand)GetDllType(typeof(MpvCommand), "mpv_command");
            _mpvSetOption = (MpvSetOption)GetDllType(typeof(MpvSetOption), "mpv_set_option");
            MpvSetOptionString = (MpvSetOptionStringFunc)GetDllType(typeof(MpvSetOptionStringFunc), "mpv_set_option_string");
            MpvGetPropertyString = (MpvGetPropertystringFunc)GetDllType(typeof(MpvGetPropertystringFunc), "mpv_get_property");
            MpvSetProperty = (MpvSetPropertyFunc)GetDllType(typeof(MpvSetPropertyFunc), "mpv_set_property");
            _mpvFree = (MpvFree)GetDllType(typeof(MpvFree), "mpv_free");
            _mpvCreateClient = (MpvCreateClient)GetDllType(typeof(MpvCreateClient), "mpv_create_client");
            _mpvObserveProperty = (MpvObserveProperty)GetDllType(typeof(MpvObserveProperty), "mpv_observe_property");
            _mpvRequestEvent = (MpvRequestEvent)GetDllType(typeof(MpvRequestEvent), "mpv_request_event");
            _mpvWaitEvent = (MpvWaitEvent)GetDllType(typeof(MpvWaitEvent), "mpv_wait_event");
            _mpvSetWakeupCallback = (MpvSetWakeupCallback)GetDllType(typeof(MpvSetWakeupCallback), "mpv_set_wakeup_callback");
        }

        public void DoMpvCommand(params string[] args)
        {
            IntPtr[] byteArrayPointers;
            var mainPtr = AllocateUtf8IntPtrArrayWithSentinel(args, out byteArrayPointers);
            _mpvCommand(_mpvHandle, mainPtr);
            foreach (var ptr in byteArrayPointers)
            {
                Marshal.FreeHGlobal(ptr);
            }
            Marshal.FreeHGlobal(mainPtr);
        }

        private static IntPtr AllocateUtf8IntPtrArrayWithSentinel(string[] arr, out IntPtr[] byteArrayPointers)
        {
            int numberOfStrings = arr.Length + 1;
            byteArrayPointers = new IntPtr[numberOfStrings];
            IntPtr rootPointer = Marshal.AllocCoTaskMem(IntPtr.Size * numberOfStrings);
            for (int index = 0; index < arr.Length; index++)
            {
                var bytes = MpvUtilsExtensions.GetUtf8Bytes(arr[index]);
                IntPtr unmanagedPointer = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
                byteArrayPointers[index] = unmanagedPointer;
            }
            Marshal.Copy(byteArrayPointers, 0, rootPointer, numberOfStrings);
            return rootPointer;
        }

        public void SetupEventListening()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            // 设置唤醒回调
            var callback = new MpvEventCallback(HandleMpvEvent);
            _mpvSetWakeupCallback(_mpvHandle, callback, IntPtr.Zero);

            // 观察一些常用属性
            ObserveProperty("time-pos");
            ObserveProperty("pause");
            ObserveProperty("duration");
        }

        private void ObserveProperty(string propertyName)
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            byte[] nameBytes = MpvUtilsExtensions.GetUtf8Bytes(propertyName);
            _mpvObserveProperty(_mpvHandle, 0, nameBytes, MpvFormatString);
        }

        private void HandleMpvEvent(IntPtr userData)
        {
            IntPtr eventPtr = _mpvWaitEvent(_mpvHandle, 0);
            if (eventPtr != IntPtr.Zero)
            {
                MpvEvent evt = Marshal.PtrToStructure<MpvEvent>(eventPtr);

                switch (evt.event_id)
                {
                    case 1: // MPV_EVENT_PROPERTY_CHANGE
                        Debug.WriteLine("Property changed event");
                        break;

                    case MpvEventError: // 错误事件
                        if (evt.data != IntPtr.Zero)
                        {
                            string errorMsg = Marshal.PtrToStringAnsi(evt.data);
                            Debug.WriteLine($"MPV Error: {errorMsg} (code: {evt.error})");
                        }
                        else
                        {
                            Debug.WriteLine($"MPV Error occurred (code: {evt.error})");
                        }

                        break;

                    case MpvEventLogMessage: // 处理日志消息事件
                        if (evt.data != IntPtr.Zero)
                        {
                            MpvEventLogMessage logMsg = Marshal.PtrToStructure<MpvEventLogMessage>(evt.data);

                            string prefix = Marshal.PtrToStringAnsi(logMsg.prefix);
                            string level = Marshal.PtrToStringAnsi(logMsg.level);
                            string text = Marshal.PtrToStringAnsi(logMsg.text);

                            Console.WriteLine($"[MPV Log] [{level}] {prefix}: {text}");
                        }

                        break;
                    default:
                        Debug.WriteLine($"Unknown MPV event type: {evt.event_id}");
                        break;
                }
            }
        }

        public void StartEventLoop()
        {
            Task.Run(() =>
            {
                while (_mpvHandle != IntPtr.Zero)
                {
                    IntPtr eventPtr = _mpvWaitEvent(_mpvHandle, 10);
                    if (eventPtr != IntPtr.Zero)
                    {
                        try
                        {
                            MpvEvent evt = Marshal.PtrToStructure<MpvEvent>(eventPtr);
                            string eventInfo = ParseMpvEvent(evt);
                            Debug.WriteLine($"[MPV Event] {evt.event_id}: {eventInfo}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MPV Event Error] {ex.Message}");
                        }
                    }
                }
            });
        }

        private string ParseMpvEvent(MpvEvent evt)
        {
            try
            {
                switch ((MpvEventId)evt.event_id)
                {
                    case MpvEventId.Shutdown:
                        return "MPV is shutting down";

                    case MpvEventId.LogMessage:
                        if (evt.data != IntPtr.Zero)
                        {
                            var log = Marshal.PtrToStructure<MpvEventLogMessage>(evt.data);
                            return $"[{Marshal.PtrToStringUTF8(log.level)}] {Marshal.PtrToStringUTF8(log.prefix)}: {Marshal.PtrToStringUTF8(log.text)}";
                        }
                        break;

                    case MpvEventId.PropertyChange:
                        if (evt.data != IntPtr.Zero)
                        {
                            var prop = Marshal.PtrToStructure<MpvEventProperty>(evt.data);
                            string propName = Marshal.PtrToStringUTF8(prop.name);
                            string value = prop.format == 1 ? Marshal.PtrToStringUTF8(prop.data) : $"[binary data, format:{prop.format}]";
                            return $"Property changed: {propName} = {value}";
                        }
                        break;

                    case MpvEventId.EndFile:
                        if (evt.data != IntPtr.Zero)
                        {
                            var endFile = Marshal.PtrToStructure<MpvEventEndFile>(evt.data);
                            return $"Playback ended (reason: {GetEndFileReason(endFile.reason)}, error: {endFile.error})";
                        }
                        break;

                    case MpvEventId.StartFile:
                        return "Playback starting";

                    case MpvEventId.FileLoaded:
                        FileLoaded?.Invoke(this,EventArgs.Empty);
                        return "File loaded";

                    case MpvEventId.VideoReconfig:
                        return "Video reconfigured";

                    case MpvEventId.AudioReconfig:
                        return "Audio reconfigured";

                    case MpvEventId.Seek:
                        return "Seek initiated";

                    case MpvEventId.PlaybackRestart:
                        return "Playback restarted";

                    case MpvEventId.ClientMessage:
                        return "Received client message";

                    case MpvEventId.QueueOverflow:
                        return "Event queue overflow";

                    case MpvEventId.Hook:
                        return "Hook triggered";

                    default:
                        return $"Unknown event: {evt.event_id}";
                }
            }
            catch (Exception ex)
            {
                return $"Error parsing event: {ex.Message}";
            }
            return "Empty event data";
        }

        private string GetEndFileReason(int reason)
        {
            switch (reason)
            {
                case 0: return "EOF/UNKNOWN";
                case 1: return "RESTARTED";
                case 2: return "ABORTED";
                case 3: return "QUIT";
                case 4: return "ERROR";
                case 5: return "REDIRECT";
                default: return $"UNKNOWN({reason})";
            }
        }


        public bool IsPaused()
        {
            if (_mpvHandle == IntPtr.Zero)
                return true;

            var lpBuffer = IntPtr.Zero;
            MpvGetPropertyString(_mpvHandle, MpvUtilsExtensions.GetUtf8Bytes("pause"), MpvFormatString, ref lpBuffer);
            var isPaused = Marshal.PtrToStringAnsi(lpBuffer) == "yes";
            _mpvFree(lpBuffer);
            return isPaused;
        }

        public void Pause()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = MpvUtilsExtensions.GetUtf8Bytes("yes");
            MpvSetProperty(_mpvHandle, MpvUtilsExtensions.GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public void Play()
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            var bytes = MpvUtilsExtensions.GetUtf8Bytes("no");
            MpvSetProperty(_mpvHandle, MpvUtilsExtensions.GetUtf8Bytes("pause"), MpvFormatString, ref bytes);
        }

        public void SetTime(double value)
        {
            if (_mpvHandle == IntPtr.Zero)
                return;

            DoMpvCommand("seek", value.ToString(CultureInfo.InvariantCulture), "absolute");
        }

        public void SetPropertyString(string key, string value)
        {
            var bytes = MpvUtilsExtensions.GetUtf8Bytes(value);
            MpvSetProperty(_mpvHandle,
                MpvUtilsExtensions.GetUtf8Bytes(key),
                MpvFormatString,
                ref bytes);
        }

        public void Initialize()
        {
            if (_mpvHandle != IntPtr.Zero)
                _mpvTerminateDestroy(_mpvHandle);
            LoadMpvDynamic();

            if (_libMpvDll == IntPtr.Zero)
                throw new Exception("Failed to load mpv library");

            _mpvHandle = _mpvCreate();
            if (_mpvHandle == IntPtr.Zero)
                throw new Exception("Failed to create mpv instance");

            if (_mpvInitialize(_mpvHandle) < 0)
                throw new Exception("Failed to initialize mpv");

            _mpvInitialize.Invoke(_mpvHandle);
            ObserveProperty("time-pos");

            // 设置事件监听
            //SetupEventListening();
            StartEventLoop();
        }

        public void Dispose()
        {
            if (_mpvHandle != IntPtr.Zero)
            {
                _mpvTerminateDestroy(_mpvHandle);
                _mpvHandle = IntPtr.Zero;
            }
            if (_libMpvDll != IntPtr.Zero)
            {
                FreeLibrary(_libMpvDll);
                _libMpvDll = IntPtr.Zero;
            }
        }

        ~MpvClient()
        {
            Dispose();
        }
    }
}
