namespace BiliLite.Models.Common.Player
{
    public class PlaySpeedMenuItem
    {
        public PlaySpeedMenuItem() { }

        public PlaySpeedMenuItem(double value)
        {
            Value = value;
        }

        public string Content => Value + "x";

        public double Value { get; set; }

        public bool IsDeletable => Value != 1;
    }
}
