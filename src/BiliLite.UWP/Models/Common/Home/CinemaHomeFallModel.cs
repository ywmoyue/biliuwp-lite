using System.Collections.Generic;

namespace BiliLite.Models.Common.Home
{
    public class CinemaHomeFallModel
    {
        public int Wid { get; set; }

        public string Title { get; set; }

        public bool ShowMore { get; set; } = true;

        public List<CinemaHomeFallItemModel> Items { get; set; }
    }
}