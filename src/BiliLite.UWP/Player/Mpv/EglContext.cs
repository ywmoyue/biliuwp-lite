using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.Mpv
{
    public class EglContext
    {
        // EGL相关常量
        private const int EGL_DEFAULT_DISPLAY = 0;
        private const int EGL_NO_CONTEXT = 0;
        private const int EGL_NO_DISPLAY = 0;
        private const int EGL_NO_SURFACE = 0;
        private const int EGL_OPENGL_ES2_BIT = 0x0004;
        private const int EGL_RED_SIZE = 0x3024;
        private const int EGL_GREEN_SIZE = 0x3025;
        private const int EGL_BLUE_SIZE = 0x3026;
        private const int EGL_ALPHA_SIZE = 0x3027;
        private const int EGL_DEPTH_SIZE = 0x3025;
        private const int EGL_STENCIL_SIZE = 0x3026;
        private const int EGL_SURFACE_TYPE = 0x3033;
        private const int EGL_RENDERABLE_TYPE = 0x3040;
        private const int EGL_NONE = 0x3038;
        private const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
        private const int EGL_WIDTH = 0x3057;
        private const int EGL_HEIGHT = 0x3056;
        private const int EGL_WINDOW_BIT = 0x0004;
        // OpenGL ES常量
        private const int GL_FRAMEBUFFER_BINDING = 0x8CA6;

        // EGL函数声明
        [DllImport("libEGL.dll")]
        private static extern IntPtr eglGetDisplay(IntPtr display_id);

        [DllImport("libEGL.dll", CallingConvention = CallingConvention.Winapi)]
        public static extern IntPtr eglGetProcAddress(string procname);

        [DllImport("libEGL.dll")]
        private static extern bool eglInitialize(IntPtr display, out int major, out int minor);

        [DllImport("libEGL.dll")]
        private static extern bool eglChooseConfig(IntPtr display, int[] attrib_list,
            IntPtr[] configs, int config_size, out int num_config);

        [DllImport("libEGL.dll")]
        private static extern IntPtr eglCreateContext(IntPtr display, IntPtr config,
            IntPtr share_context, int[] attrib_list);

        [DllImport("libEGL.dll")]
        private static extern IntPtr eglCreateWindowSurface(IntPtr display, IntPtr config,
            IntPtr win, int[] attrib_list);

        [DllImport("libEGL.dll")]
        private static extern bool eglMakeCurrent(IntPtr display, IntPtr draw, IntPtr read, IntPtr context);

        [DllImport("libEGL.dll")]
        private static extern bool eglSwapBuffers(IntPtr display, IntPtr surface);

        [DllImport("libEGL.dll")]
        private static extern int eglGetError();

        // OpenGL ES函数声明
        private delegate void glGetIntegervDelegate(int pname, out int data);
        private glGetIntegervDelegate glGetIntegerv;

        // EGL变量
        private IntPtr eglDisplay = IntPtr.Zero;
        private IntPtr eglContext = IntPtr.Zero;
        private IntPtr eglSurface = IntPtr.Zero;
        private IntPtr eglConfig = IntPtr.Zero;

        public bool InitializeEGL(SwapChainPanel panel)
        {
            try
            {
                int eglError0 = eglGetError();
                // 1. 获取EGL显示
                eglDisplay = eglGetDisplay(IntPtr.Zero);
                if (eglDisplay == IntPtr.Zero)
                    throw new Exception("Failed to get EGL display");

                int eglError2 = eglGetError();
                // 2. 初始化EGL
                if (!eglInitialize(eglDisplay, out int major, out int minor))
                    throw new Exception("Failed to initialize EGL");

                int eglError1 = eglGetError();
                // 3. 选择配置
                int[] configAttribs = {
            EGL_RED_SIZE, 8,
            EGL_GREEN_SIZE, 8,
            EGL_BLUE_SIZE, 8,
            EGL_ALPHA_SIZE, 8,
            EGL_DEPTH_SIZE, 24,
            EGL_STENCIL_SIZE, 8,
            EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
            EGL_NONE
        };

                IntPtr[] configs = new IntPtr[1];
                if (!eglChooseConfig(eglDisplay, configAttribs, configs, 1, out int numConfig))
                {
                    int eglError = eglGetError();
                    throw new Exception($"eglChooseConfig failed with error: 0x{eglError:X}");
                }

                if (numConfig == 0)
                {
                    // 尝试更宽松的配置要求
                    int[] fallbackAttribs = {
                        EGL_RED_SIZE, 5,
                        EGL_GREEN_SIZE, 6,
                        EGL_BLUE_SIZE, 5,
                        EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
                        EGL_NONE
                    };

                    if (!eglChooseConfig(eglDisplay, fallbackAttribs, configs, 1, out numConfig) || numConfig == 0)
                    {
                        int eglError = eglGetError();
                        throw new Exception("No matching EGL config available (even with fallback settings)");
                    }
                }

                eglConfig = configs[0];

                // 4. 创建上下文
                int[] contextAttribs = {
            EGL_CONTEXT_CLIENT_VERSION, 2,
            EGL_NONE
        };

                eglContext = eglCreateContext(eglDisplay, eglConfig, IntPtr.Zero, contextAttribs);
                if (eglContext == IntPtr.Zero)
                    throw new Exception("Failed to create EGL context");

                // 5. 创建表面 (使用SwapChainPanel)
                var panelNative = Marshal.GetIUnknownForObject(panel);
                int[] surfaceAttribs = {
            EGL_WIDTH, (int)panel.ActualWidth,
            EGL_HEIGHT, (int)panel.ActualHeight,
            EGL_NONE
        };

                eglSurface = eglCreateWindowSurface(eglDisplay, eglConfig, panelNative, surfaceAttribs);
                Marshal.Release(panelNative);

                if (eglSurface == IntPtr.Zero)
                    throw new Exception("Failed to create EGL surface");

                // 6. 设置当前上下文
                if (!eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
                    throw new Exception("Failed to make EGL context current");

                // 7. 获取GL函数指针
                glGetIntegerv = (glGetIntegervDelegate)Marshal.GetDelegateForFunctionPointer(
                    eglGetProcAddress("glGetIntegerv"), typeof(glGetIntegervDelegate));

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"EGL初始化失败: {ex.Message}");
                CleanupEGL();
                return false;
            }
        }
        public void CleanupEGL()
        {
            // 清理EGL资源
            if (eglDisplay != IntPtr.Zero)
            {
                eglMakeCurrent(eglDisplay, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                if (eglContext != IntPtr.Zero)
                {
                    // eglDestroyContext(eglDisplay, eglContext);
                    eglContext = IntPtr.Zero;
                }
                if (eglSurface != IntPtr.Zero)
                {
                    // eglDestroySurface(eglDisplay, eglSurface);
                    eglSurface = IntPtr.Zero;
                }
                // eglTerminate(eglDisplay);
                eglDisplay = IntPtr.Zero;
            }
        }

        public int GetCurrentFbo()
        {
            if (glGetIntegerv == null)
                throw new InvalidOperationException("OpenGL ES not initialized");

            int currentFbo;
            glGetIntegerv(GL_FRAMEBUFFER_BINDING, out currentFbo);
            return currentFbo;
        }

        public void SwapBuffers()
        {
            if (eglDisplay == IntPtr.Zero || eglSurface == IntPtr.Zero)
                throw new InvalidOperationException("EGL not initialized");

            if (!eglSwapBuffers(eglDisplay, eglSurface))
            {
                int error = eglGetError();
                throw new Exception($"eglSwapBuffers failed with error: 0x{error:X}");
            }
        }
    }
}
