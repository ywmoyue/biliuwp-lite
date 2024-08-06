using BiliLite.ViewModels.UserDynamic;

namespace BiliLite.Models.Common.UserDynamic
{
    public class UserDynamicSeasonInfo
    {
        public string SeasonId { get; set; }

        public string Url { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        public UserDynamicSeasonNewEpInfo NewEp { get; set; }

        public DynamicV2ItemViewModel ToDynamicItem(IUserDynamicCommands parent)
        {
            var item = new DynamicV2ItemViewModel()
            {
                CardType = Constants.DynamicTypes.CUSTOM_SEASON,
                Season = this,
                Parent = parent,
            };
            return item;
        }
    }

    public class UserDynamicSeasonNewEpInfo
    {
        public string IndexShow { get; set; }

        public string Cover { get; set; }
    }
}
