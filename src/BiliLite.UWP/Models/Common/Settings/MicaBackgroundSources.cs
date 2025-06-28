using System.Collections.Generic;
using MicaForUWP.Media;

namespace BiliLite.Models.Common.Settings;

public class MicaBackgroundSources
{
    public List<KeyValueOption<BackgroundSource>> Options =
    [
        new KeyValueOption<BackgroundSource>("系统内容", BackgroundSource.HostBackdrop),
        new KeyValueOption<BackgroundSource>("壁纸内容", BackgroundSource.WallpaperBackdrop)
    ];
}