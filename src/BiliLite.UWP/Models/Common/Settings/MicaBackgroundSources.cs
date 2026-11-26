using System.Collections.Generic;

namespace BiliLite.Models.Common.Settings;

/// <summary>
/// 窗口背景材质。
/// WinUI3 下云母/亚克力只能作为窗口级 SystemBackdrop 应用，无法像 UWP 的 MicaForUWP 那样
/// 作为笔刷放进主题字典，因此这里只区分"要对窗口应用哪种材质"。
/// 云母依赖 Windows 11；亚克力在 Windows 10 1809 及以上可用。
/// 实际是否生效由 MicaController/DesktopAcrylicController.IsSupported() 在运行时判断。
/// </summary>
public enum SystemBackdropSource
{
    /// <summary>
    /// 亚克力
    /// </summary>
    Acrylic = 1,

    /// <summary>
    /// 云母（默认，与历史默认值 2 保持一致）
    /// </summary>
    Mica = 2,
}

public class MicaBackgroundSources
{
    public List<KeyValueOption<SystemBackdropSource>> Options =
    [
        new KeyValueOption<SystemBackdropSource>("亚克力", SystemBackdropSource.Acrylic),
        new KeyValueOption<SystemBackdropSource>("云母", SystemBackdropSource.Mica)
    ];
}
