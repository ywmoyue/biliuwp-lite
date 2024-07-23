using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.Models.Common.Player
{
    public class PlaySpeedMenuItem
    {
        public PlaySpeedMenuItem(){}

        public PlaySpeedMenuItem(double value)
        {
            Value = value;
        }

        public string Content => Value + "x";

        public double Value { get; set; }
    }
}
