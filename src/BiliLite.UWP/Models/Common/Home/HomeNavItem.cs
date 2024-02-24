using System;
using FontAwesome5;

namespace BiliLite.Models.Common.Home
{
    public class HomeNavItem
    {
        public string Title { get; set; }

        public EFontAwesomeIcon Icon { get; set; }

        public Type Page { get; set; }

        public object Parameters { get; set; }

        public bool NeedLogin { get; set; }

        public bool Show { get; set; }
    }
}