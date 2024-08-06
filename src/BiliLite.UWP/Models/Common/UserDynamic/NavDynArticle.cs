using Bilibili.App.Dynamic.V2;
using BiliLite.ViewModels.UserDynamic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.UserDynamic
{
    public class NavDynArticle
    {
        public string Cover { get; set; }

        [JsonProperty("id_str")]
        public string Id { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }

        [JsonProperty("pub_time")]
        public string PubTime { get; set; }

        public long Rid { get; set; }

        public string Title { get; set; }

        public bool Visible { get; set; }

        public NavDynArticleAuthor Author { get; set; }

        public DynamicV2ItemViewModel ToDynamicItem(IUserDynamicCommands parent)
        {
            var item = new DynamicV2ItemViewModel()
            {
                CardType = Constants.DynamicTypes.CUSTOM_ARTICLE,
                CustomArticle = this,
                Author = new ModuleAuthor()
                {
                    Mid = Author.Mid,
                    Author = new UserInfo()
                    {
                        Face = Author.Face,
                        Official = new OfficialVerify()
                        {
                            Type = Author.Official.Type
                        },
                        Name = Author.Name,
                        Vip = new VipInfo()
                        {
                            Status = Author.Vip.Status
                        },
                    },
                    PtimeLabelText = PubTime,
                },
                Extend = new Extend()
                {
                    DynIdStr = Id
                },
                Parent = parent,
            };
            return item;
        }
    }

    public class NavDynArticleAuthor
    {
        public string Face { get; set; }

        public long Mid { get; set; }

        public string Name { get; set; }

        public NavDynArticleAuthorOfficial Official { get; set; }

        public NavDynArticleAuthorVip Vip { get; set; }
    }

    public class NavDynArticleAuthorOfficial
    {
        public int Role { get; set; }

        public int Type { get; set; }

        public string Title { get; set; }
    }

    public class NavDynArticleAuthorVip
    {
        public int Status { get; set; }
    }
}
