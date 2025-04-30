using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Player.ShakaPlayer.Models
{
    public class BaseShakaPlayerEventMessage
    {
        public string Event { get; set; }

        public object Data { get; set; }
    }

    public static class ShakaPlayerEventLists
    {
        public const string POSITION_CHANGED = "positionChanged";

        public const string LOADED = "loaded";
    }
}
