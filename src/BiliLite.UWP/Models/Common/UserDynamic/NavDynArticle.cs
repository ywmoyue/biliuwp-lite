using Bilibili.App.Dynamic.V2;
using BiliLite.ViewModels.UserDynamic;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace BiliLite.Models.Common.UserDynamic
{
    public interface ICustomArticle
    {
        public string Title { get; }

        public string JumpUrl { get; }

        public string Cover { get; }
    }

    public class NavDynArticle : ICustomArticle
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

    public class ModuleDynArticle : ICustomArticle
    {
        public ModuleDynArticleBasic Basic { get; set; }

        [JsonProperty("id_str")]
        public string IdStr { get; set; }

        public ModuleDynArticleModules Modules { get; set; }

        public string Title
        {
            get
            {
                return Modules.ModuleDynamic?.Major?.Opus?.Title;
            }
        }

        public string JumpUrl
        {
            get
            {
                var url = Modules.ModuleDynamic?.Major?.Opus?.JumpUrl;
                if(url != null && url.StartsWith("//"))
                {
                    url = "https:" + url;
                }
                return url;
            }
        }

        public string Cover
        {
            get
            {
                return Modules.ModuleDynamic?.Major?.Opus?.Pics?.FirstOrDefault()?.Url;
            }
        }

        public DynamicV2ItemViewModel ToDynamicItem(IUserDynamicCommands parent)
        {
            var item = new DynamicV2ItemViewModel()
            {
                CardType = Constants.DynamicTypes.CUSTOM_ARTICLE,
                CustomArticle = this,
                Author = new ModuleAuthor()
                {
                    Mid = Modules.ModuleAuthor.Mid,
                    Author = new UserInfo()
                    {
                        Face = Modules.ModuleAuthor.Face,
                        Official = Modules.ModuleAuthor.OfficialVerify,
                        Name = Modules.ModuleAuthor.Name,
                        Vip = new VipInfo()
                        {
                            Status = Modules.ModuleAuthor.Vip.Status
                        },
                    },
                    PtimeLabelText = Modules.ModuleAuthor.PubTime,
                },
                Extend = new Extend()
                {
                    DynIdStr = IdStr
                },
                Parent = parent,
            };
            return item;
        }
    }

    public class ModuleDynArticleBasic
    {
        [JsonProperty("comment_id_str")]
        public string CommentIdStr { get; set; }

        [JsonProperty("comment_type")]
        public string CommentType { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }

        [JsonProperty("rid_str")]
        public string RidStr { get; set; }
    }

    public class ModuleDynArticleModules
    {
        [JsonProperty("module_author")]
        public ModuleDynArticleModuleAuthor ModuleAuthor { get; set; }

        [JsonProperty("module_dynamic")]
        public ModuleDynArticleModuleDynamic ModuleDynamic { get; set; }

        public class ModuleDynArticleModuleDynamic
        {
            public ModuleDynModuleDynamicMajor Major { get; set; }

            public class ModuleDynModuleDynamicMajor
            {
                public ModuleDynModuleDynamicOpus Opus { get; set; }

                public class ModuleDynModuleDynamicOpus
                {
                    public string Title { get; set; }

                    [JsonProperty("jump_url")]
                    public string JumpUrl { get; set; }

                    public List<ModuleDynModuleDynamicPic> Pics { get; set; }

                    public class ModuleDynModuleDynamicPic
                    {
                        public string Url { get; set; }
                    }
                }
            }
        }

        public class ModuleDynArticleModuleAuthor
        {
            public string Face { get; set; }

            public string Name { get; set; }

            public long Mid { get; set; }

            [JsonProperty("pub_action")]
            public string PubAction { get; set; }

            [JsonProperty("pub_time")]
            public string PubTime { get; set; }

            [JsonProperty("official_verify")]
            public OfficialVerify OfficialVerify { get; set; }

            public NavDynArticleAuthorVip Vip { get; set; }
        }
    }
}
