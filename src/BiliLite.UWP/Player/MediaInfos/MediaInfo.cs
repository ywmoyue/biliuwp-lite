using System.Text;
using System;
using BiliLite.Extensions;

namespace BiliLite.Player.MediaInfos
{
    public class MediaInfo
    {
        public string VideoCodec { get; set; }

        public string AudioCodec { get; set; }

        [MediaInfoDoNotToString]
        public float VideoHeight { get; set; }

        [MediaInfoDoNotToString]
        public float VideoWidth { get; set; }

        public string Resolution => $"{VideoWidth} * {VideoHeight}";

        private double _VideoBitRate { get; set; }
        public string VideoBitRate
        {
            get => (_VideoBitRate / 1000).ToString() + " Kbps";
            set => _VideoBitRate = value.ToInt32();
        }

        private double _AudioBitRate { get; set; }
        public string AudioBitRate
        {
            get => (_AudioBitRate / 1000).ToString() + " Kbps";
            set => _AudioBitRate = value.ToInt32();
        }

        [MediaInfoDoNotToString]
        public string DecoderEngine { get; set; }

        [MediaInfoDoNotToString]
        public double Speed { get; set; }

        [MediaInfoDoNotToString]
        public int DroppedFrames { get; set; }

        [MediaInfoDoNotToString]
        public int PacketsLost { get; set; }

        public double Fps { get; set; }

        public string PlayerType { get; set; }

        public string Url { get; set; }

        public override string ToString()
        {
            var properties = this.GetType().GetProperties();
            var strBuilder = new StringBuilder();
            foreach (var property in properties)
            {
                var hasAttribute = Attribute.IsDefined(property, typeof(MediaInfoDoNotToStringAttribute));
                if (!hasAttribute && property.GetValue(this) != null)
                {
                    strBuilder.Append(property.Name + ": " + property.GetValue(this) + "\r\n");
                }
            }
            return strBuilder.ToString().TrimEnd(',', ' ');
        }
    }
}
