using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using PropertyChanged;

namespace BiliLite.ViewModels.User
{
    public class FavoriteItemViewModel : BaseViewModel
    {
        public string Cover { get; set; }

        public int Attr { get; set; }

        public string Intro { get; set; }

        public string Fid { get; set; }

        public string Id { get; set; }

        [JsonProperty("like_state")]
        public int LikeState { get; set; }

        public string Mid { get; set; }

        public string Title { get; set; }

        public int Type { get; set; }

        [JsonProperty("media_count")]
        public int MediaCount { get; set; }

        [JsonProperty("fav_state")]
        public int FavState { get; set; }

        [DependsOn(nameof(Attr))]
        public bool Privacy =>
            //attr单数为私密，双数为公开
            Attr % 2 != 0;

        [AlsoNotifyFor(nameof(FavState))]
        public bool IsFav
        {
            get => FavState == 1;
            set => FavState = value ? 1 : 0;
        }
    }
}