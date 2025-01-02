using System;

namespace BiliLite.Pages
{
    public interface IMainPage
    {
        public object CurrentPage { get; }

        public event EventHandler MainPageLoaded;
    }
}
