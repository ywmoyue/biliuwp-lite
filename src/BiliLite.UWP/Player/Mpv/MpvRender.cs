using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.Mpv
{
    /// <summary>
    /// MPV渲染器封装类，提供对mpv渲染API的基本封装
    /// </summary>
    public class MpvRender : IDisposable
    {
        private SwapChainPanel _panel;
        private IntPtr _mpvRenderContext;
        private const int MPV_RENDER_PARAM_OPENGL_FBO = 3;
        private const int MPV_RENDER_PARAM_FLIP_Y = 4;
        private readonly MpvClient _mpvClient;
        private readonly EglContext _eglContext = new EglContext();

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false)]
        internal static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvRenderContextCreate(ref IntPtr context, IntPtr mpvHandler, IntPtr parameters);
        private MpvRenderContextCreate _mpvRenderContextCreate;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr OpenGlRenderContextCallback(IntPtr ctx, IntPtr name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MpvRenderContextRender(IntPtr ctx, IntPtr[] parameters);
        private MpvRenderContextRender _mpvRenderContextRender;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MpvRenderContextSetUpdateCallback(IntPtr ctx, MpvRenderUpdateFn callback, IntPtr callback_ctx);
        private MpvRenderContextSetUpdateCallback _mpvRenderContextSetUpdateCallback;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void MpvRenderUpdateFn(IntPtr callback_ctx);

        public MpvRender(MpvClient mpvClient)
        {
            _mpvClient = mpvClient;
        }

        private object GetDllType(Type type, string name)
        {
            IntPtr address = GetProcAddress(_mpvClient.LibMpvDll, name);
            if (address != IntPtr.Zero)
                return Marshal.GetDelegateForFunctionPointer(address, type);
            return null;
        }

        private void LoadMpvDynamic()
        {
            _mpvRenderContextCreate = (MpvRenderContextCreate)GetDllType(typeof(MpvRenderContextCreate), "mpv_render_context_create");
            _mpvRenderContextRender = (MpvRenderContextRender)GetDllType(typeof(MpvRenderContextRender), "mpv_render_context_render");
            _mpvRenderContextSetUpdateCallback = (MpvRenderContextSetUpdateCallback)GetDllType(typeof(MpvRenderContextSetUpdateCallback), "mpv_render_context_set_update_callback");
        }

        private IntPtr GetGLProcAddress(IntPtr ctx, IntPtr name)
        {
            string functionName = Marshal.PtrToStringAnsi(name);

            var address = EglContext.eglGetProcAddress(functionName);
            return address;
        }

        private string GenerateDashManifest(string videoUrl, string audioUrl)
        {
            return $@"﻿<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011"" profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
    <Period start=""PT0S"">
        <AdaptationSet>
            <ContentComponent contentType=""video"" id=""1"" />
            {videoUrl}
        </AdaptationSet>
        <AdaptationSet>
            <ContentComponent contentType=""audio"" id=""2"" />
            {audioUrl}
        </AdaptationSet>
    </Period>
</MPD>";
        }

        public unsafe void Initialize(SwapChainPanel panel, string videoPath, string audioPath)
        {
            if (_mpvClient.MpvHandle == IntPtr.Zero)
                throw new Exception("MpvClient has not initialized");
            _panel = panel;

            if (!_eglContext.InitializeEGL(panel))
                throw new Exception("Failed to initialize EGL");

            LoadMpvDynamic();

            var oglInitParams = new MpvOpenGlInitParams();
            oglInitParams.get_proc_address = (ctx, name) =>
            {
                return GetGLProcAddress(ctx, name);
            };
            oglInitParams.get_proc_address_ctx = IntPtr.Zero;
            oglInitParams.extra_exts = IntPtr.Zero;

            var size = Marshal.SizeOf<MpvOpenGlInitParams>();
            var oglInitParamsBuf = new byte[size];

            fixed (byte* arrPtr = oglInitParamsBuf)
            {
                IntPtr oglInitParamsPtr = new IntPtr(arrPtr);
                Marshal.StructureToPtr(oglInitParams, oglInitParamsPtr, true);

                MpvRenderParam* parameters = stackalloc MpvRenderParam[3];

                parameters[0].type = MpvRenderParamType.ApiType;
                parameters[0].data = Marshal.StringToHGlobalAnsi("opengl");

                parameters[1].type = MpvRenderParamType.InitParams;
                parameters[1].data = oglInitParamsPtr;

                parameters[2].type = MpvRenderParamType.Invalid;
                parameters[2].data = IntPtr.Zero;

                var renderParamSize = Marshal.SizeOf<MpvRenderParam>();

                var paramBuf = new byte[renderParamSize * 3];
                fixed (byte* paramBufPtr = paramBuf)
                {
                    IntPtr param1Ptr = new IntPtr(paramBufPtr);
                    Marshal.StructureToPtr(parameters[0], param1Ptr, true);

                    IntPtr param2Ptr = new IntPtr(paramBufPtr + renderParamSize);
                    Marshal.StructureToPtr(parameters[1], param2Ptr, true);

                    IntPtr param3Ptr = new IntPtr(paramBufPtr + renderParamSize + renderParamSize);
                    Marshal.StructureToPtr(parameters[2], param3Ptr, true);

                    IntPtr context = new IntPtr(0);
                    _mpvRenderContextCreate(ref context, _mpvClient.MpvHandle, param1Ptr);
                    _mpvRenderContext = context;
                }
            }
            _mpvRenderContextSetUpdateCallback(_mpvRenderContext, OnMpvRenderUpdate, IntPtr.Zero);

            // 设置常用选项
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("keep-open"), MpvUtilsExtensions.GetUtf8Bytes("always"));
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("hwdec"), MpvUtilsExtensions.GetUtf8Bytes("auto-safe"));
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("vo"), MpvUtilsExtensions.GetUtf8Bytes("libmpv"));

            // 构建命令参数数组
            string[] command = new string[] {
        "loadfile",
        videoPath,
        "replace",
        $"audio-file={audioPath}"
    };

            // 构建命令参数 - 先加载视频
            _mpvClient.DoMpvCommand("loadfile", videoPath, "replace");

            // 然后附加音频流
            _mpvClient.DoMpvCommand("audio-add", audioPath, "auto");
        }

        public unsafe void Initialize(SwapChainPanel panel, string videoPath)
        {
            if (_mpvClient.MpvHandle == IntPtr.Zero)
                throw new Exception("MpvClient has not initialized");
            _panel = panel;

            if (!_eglContext.InitializeEGL(panel))
                throw new Exception("Failed to initialize EGL");

            LoadMpvDynamic();

            var oglInitParams = new MpvOpenGlInitParams();
            oglInitParams.get_proc_address = (ctx, name) =>
            {
                return GetGLProcAddress(ctx, name);
            };
            oglInitParams.get_proc_address_ctx = IntPtr.Zero;
            oglInitParams.extra_exts = IntPtr.Zero;

            var size = Marshal.SizeOf<MpvOpenGlInitParams>();
            var oglInitParamsBuf = new byte[size];

            fixed (byte* arrPtr = oglInitParamsBuf)
            {
                IntPtr oglInitParamsPtr = new IntPtr(arrPtr);
                Marshal.StructureToPtr(oglInitParams, oglInitParamsPtr, true);

                MpvRenderParam* parameters = stackalloc MpvRenderParam[3];

                parameters[0].type = MpvRenderParamType.ApiType;
                parameters[0].data = Marshal.StringToHGlobalAnsi("opengl");

                parameters[1].type = MpvRenderParamType.InitParams;
                parameters[1].data = oglInitParamsPtr;

                parameters[2].type = MpvRenderParamType.Invalid;
                parameters[2].data = IntPtr.Zero;

                var renderParamSize = Marshal.SizeOf<MpvRenderParam>();

                var paramBuf = new byte[renderParamSize * 3];
                fixed (byte* paramBufPtr = paramBuf)
                {
                    IntPtr param1Ptr = new IntPtr(paramBufPtr);
                    Marshal.StructureToPtr(parameters[0], param1Ptr, true);

                    IntPtr param2Ptr = new IntPtr(paramBufPtr + renderParamSize);
                    Marshal.StructureToPtr(parameters[1], param2Ptr, true);

                    IntPtr param3Ptr = new IntPtr(paramBufPtr + renderParamSize + renderParamSize);
                    Marshal.StructureToPtr(parameters[2], param3Ptr, true);


                    IntPtr context = new IntPtr(0);
                    _mpvRenderContextCreate(ref context, _mpvClient.MpvHandle, param1Ptr);
                    _mpvRenderContext = context;
                }
            }
            _mpvRenderContextSetUpdateCallback(_mpvRenderContext, OnMpvRenderUpdate, IntPtr.Zero);

            // 设置常用选项
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("keep-open"), MpvUtilsExtensions.GetUtf8Bytes("always"));
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("hwdec"), MpvUtilsExtensions.GetUtf8Bytes("auto-safe"));
            _mpvClient.MpvSetOptionString(_mpvClient.MpvHandle, MpvUtilsExtensions.GetUtf8Bytes("vo"), MpvUtilsExtensions.GetUtf8Bytes("libmpv"));

            // 加载视频文件
            _mpvClient.DoMpvCommand("loadfile", videoPath, "replace"); 
            _mpvClient.DoMpvCommand("set", "pause", "yes");
        }


        private void OnMpvRenderUpdate(IntPtr callback_ctx)
        {
            // This gets called when a new frame should be rendered
            RenderFrame();
        }

        public void RenderFrame()
        {
            if (_mpvRenderContext == IntPtr.Zero || _panel == null)
                return;

            // 确保在UI线程执行
            _panel.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    // 获取当前OpenGL上下文和FBO
                    int fbo = _eglContext.GetCurrentFbo(); // 需要实现这个方法来获取当前FBO
                    int width = (int)_panel.ActualWidth;
                    int height = (int)_panel.ActualHeight;

                    if (width <= 0 || height <= 0)
                        return;

                    // 准备FBO参数
                    var fboParams = new MpvOpenGlFbo
                    {
                        fbo = fbo,
                        w = width,
                        h = height,
                        internal_format = 0 // 通常为0，表示默认格式
                    };

                    // 创建渲染参数数组
                    IntPtr[] renderParams = new IntPtr[6];

                    // FBO参数
                    IntPtr fboPtr = Marshal.AllocHGlobal(Marshal.SizeOf(fboParams));
                    Marshal.StructureToPtr(fboParams, fboPtr, false);
                    renderParams[0] = new IntPtr(MPV_RENDER_PARAM_OPENGL_FBO);
                    renderParams[1] = fboPtr;

                    // 翻转Y坐标 (UWP通常需要)
                    int flip_y = 1;
                    IntPtr flipYPtr = Marshal.AllocHGlobal(sizeof(int));
                    Marshal.WriteInt32(flipYPtr, flip_y);
                    renderParams[2] = new IntPtr(MPV_RENDER_PARAM_FLIP_Y);
                    renderParams[3] = flipYPtr;

                    // 结束标记
                    renderParams[4] = IntPtr.Zero;
                    renderParams[5] = IntPtr.Zero;

                    // 执行渲染
                    int result = _mpvRenderContextRender(_mpvRenderContext, renderParams);
                    if (result < 0)
                    {
                        // 处理渲染错误
                        Debug.WriteLine($"mpv渲染失败: {result}");
                    }

                    // 交换缓冲区 (需要在您的OpenGL上下文中实现)
                    _eglContext.SwapBuffers();

                    // 清理资源
                    Marshal.FreeHGlobal(fboPtr);
                    Marshal.FreeHGlobal(flipYPtr);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"渲染帧时出错: {ex.Message}");
                }
            });

        }

        public void Dispose()
        {
            _eglContext.CleanupEGL();
        }

        ~MpvRender()
        {
            Dispose();
        }
    }

}