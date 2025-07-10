using System.Collections.Generic;
using MicaForUWP.Media;

namespace BiliLite.Models.Common.Settings;

public class MicaBackgroundSources
{
    public List<KeyValueOption<BackgroundSource>> Options =
    [
        new KeyValueOption<BackgroundSource>("亚克力", BackgroundSource.HostBackdrop),
        new KeyValueOption<BackgroundSource>("云母", BackgroundSource.WallpaperBackdrop)
    ];
}