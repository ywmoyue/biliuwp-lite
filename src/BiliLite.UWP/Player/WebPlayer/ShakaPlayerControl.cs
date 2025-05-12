using BiliLite.Models.Common.Player;

namespace BiliLite.Player.WebPlayer;

public class ShakaPlayerControl : BaseWebPlayer
{
    public override string PlayerView { get; } = "ShakaPlayerView";

    public override RealPlayerType Type { get; } = RealPlayerType.ShakaPlayer;
}