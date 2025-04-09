using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.Player.Mpv
{
    public class MpvConstants
    {
        public static string MpvPath = Path.Combine(ApplicationData.Current.LocalFolder.Path,
            "libmpv-2.dll");
    }
}
