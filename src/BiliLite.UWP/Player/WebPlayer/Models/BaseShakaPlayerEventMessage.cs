namespace BiliLite.Player.WebPlayer.Models;

public class BaseShakaPlayerEventMessage
{
    public string Event { get; set; }

    public object Data { get; set; }
}

public static class ShakaPlayerEventLists
{
    public const string POSITION_CHANGED = "positionChanged";

    public const string LOADED = "loaded";

    public const string ENDED = "ended";

    public const string VOLUME_CHANGED = "volumeChanged";

    public const string STATS = "stats";
}