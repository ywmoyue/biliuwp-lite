using BiliLite.Extensions;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season;

public class SeasonIndexResultItemOrderModel
{
    public string Follow { get; set; }

    public string Play { get; set; }

    public string Score { get; set; }

    [JsonProperty("pub_date")]
    public long PubDate { get; set; }

    [JsonProperty("pub_real_time")]
    public long PubRealTime { get; set; }

    [JsonProperty("renewal_time")]
    public long RenewalTime { get; set; }

    public string Type { get; set; }

    public string BottomText
    {
        get
        {
            if (Type == "follow")
            {
                return Follow;
            }
            else
            {
                return RenewalTime.HandelTimestamp() + "更新";
            }
        }
    }
    public bool ShowScore => Type == "score";
}