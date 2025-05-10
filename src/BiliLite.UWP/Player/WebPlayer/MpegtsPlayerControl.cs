using BiliLite.Models.Common.Player;

namespace BiliLite.Player.WebPlayer;

public class MpegtsPlayerControl : BaseWebPlayer
{
    public override string PlayerView { get; } = "MpegtsPlayerView";

    public override RealPlayerType Type { get; } = RealPlayerType.Mpegts;
}
