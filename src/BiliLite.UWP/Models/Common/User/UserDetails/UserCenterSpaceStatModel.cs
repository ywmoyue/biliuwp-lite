using BiliLite.Extensions;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterSpaceStatModel
    {
        public int Following { get; set; }

        public string Attention => Following > 0 ? " " + Following.ToCountString() : "";

        public int Follower { get; set; }

        public string Fans => Follower > 0 ? " " + Follower.ToCountString() : "";

        public int VideoCount { get; set; }

        public string Video => VideoCount > 0 ? " " + VideoCount.ToCountString() : "";

        public int ArticleCount { get; set; }

        public string Article => ArticleCount > 0 ? " " + ArticleCount.ToCountString() : "";

        public int FavouriteCount { get; set; }

        public string Favourite => FavouriteCount > 0 ? " " + FavouriteCount.ToCountString() : "";

        public int CollectionCount { get; set; }

        public string Collection => CollectionCount > 0 ? " " + CollectionCount.ToCountString() : "";
    }
}