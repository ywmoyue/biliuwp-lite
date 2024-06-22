using Newtonsoft.Json;

namespace BiliLite.Models.Common.Rank
{
    public class RankItemModel
    {
        public int Rank { get; set; }

        public string Aid { get; set; }

        public int Videos { get; set; }

        public int Tid { get; set; }

        public string Tname { get; set; }

        public int Copyright { get; set; }

        public string Pic { get; set; }

        public string Title { get; set; }

        public int Pubdate { get; set; }

        public int Ctime { get; set; }

        public string Desc { get; set; }

        public int State { get; set; }

        public int Duration { get; set; }

        [JsonProperty("mission_id")]
        public int MissionId { get; set; }

        public RankItemOwnerModel Owner { get; set; }

        public RankItemStatModel Stat { get; set; }

        public string Dynamic { get; set; }

        public long Cid { get; set; }

        public string Bvid { get; set; }

        public int Score { get; set; }
    }
}