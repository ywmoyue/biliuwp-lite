using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class SessionEmoteInfo
{
    public string Text { get; set; }

    public string Uri { get; set; }

    public int Size { get; set; }

    [JsonProperty("gif_url")]
    public string GifUrl { get; set; }
}