using System;
using BiliLite.ViewModels.Common;
using FontAwesome5;
using PropertyChanged;

namespace BiliLite.ViewModels.Home
{
    public class HomeNavItemViewModel : BaseViewModel
    {
        [DoNotNotify]
        public string Title { get; set; }

        [DoNotNotify]
        public EFontAwesomeIcon Icon { get; set; }

        [DoNotNotify]
        public Type Page { get; set; }

        [DoNotNotify]
        public object Parameters { get; set; }

        [DoNotNotify]
        public bool NeedLogin { get; set; }

        public bool Show { get; set; }
    }
}