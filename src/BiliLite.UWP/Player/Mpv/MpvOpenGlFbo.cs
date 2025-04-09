using System.Runtime.InteropServices;

namespace BiliLite.Player.Mpv;

/// <summary>
/// 用于 MPV_RENDER_PARAM_OPENGL_FBO 的结构体
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct MpvOpenGlFbo
{
    /// <summary>
    /// 帧缓冲对象名称
    /// </summary>
    /// <remarks>
    /// 必须是由 glGenFramebuffers() 生成的有效 FBO（已完成且可颜色渲染），或 0。
    /// 如果值为 0，则表示 OpenGL 默认帧缓冲区。
    /// </remarks>
    public int fbo;

    /// <summary>
    /// 帧缓冲区的宽度
    /// </summary>
    public int w;

    /// <summary>
    /// 帧缓冲区的高度
    /// </summary>
    public int h;

    /// <summary>
    /// 底层纹理的内部格式（如 GL_RGBA8），如果未知则为 0
    /// </summary>
    /// <remarks>
    /// 如果是默认帧缓冲区，可以是等效格式。
    /// </remarks>
    public int internal_format;
}