namespace BiliLite.Player.WebPlayer.Models;

public class WebPlayerStatsUpdatedData
{
    public int Height { get; set; }

    public int Width { get; set; }

    public int? DecodedFrames { get; set; }

    public int? DroppedFrames { get; set; }

    public double? BpsAudio { get; set; }

    public double? BpsVideo { get; set; }

    public string Speed { get; set; }

    public string AudioCodec { get; set; }

    public string VideoCodec { get; set; }
}