using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;

public static class SystemInformation
{
    public static bool IsFirstRun
    {
        get
        {
            
            var result = SettingService.GetValue(SettingConstants.UI.IS_FIRST_RUN, true);
            if (result)
            {
                SettingService.SetValue(SettingConstants.UI.IS_FIRST_RUN, false);
            }
            return result;
        }
    }

    public static class ApplicationVersion
    {
        private static Package Package = Package.Current;

        public static int Major = Package.Id.Version.Major;

        public static int Minor = Package.Id.Version.Minor;

        public static int Build = Package.Id.Version.Build;

        public static int Revision = Package.Id.Version.Revision;
    }
}

