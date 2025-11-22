using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using MapsterMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PropertyChanged;

namespace BiliLite.ViewModels.UserDynamic
{
    public class DynamicV2ItemViewModel : BaseViewModel
    {
        private ModuleDynamic m_dynamic;
        private ModuleStat m_stat;
        private readonly IMapper m_mapper;

        public DynamicV2ItemViewModel()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
        }

        [DoNotNotify]
        public string ManuscriptTitle
        {
            get
            {
                // Dynamic.DynArchive.Title
                if (Dynamic is { DynArchive: { } })
                {
                    return Dynamic.DynArchive.Title;
                }
                // Extend.OrigDesc[0].Text
                if (Extend is { OrigDesc: { } } && Extend.OrigDesc.Count > 0)
                {
                    return Extend.OrigDesc[0].Text;
                }
                // LiveInfo.PlayInfo.Title
                if (LiveInfo is { PlayInfo: { } })
                {
                    return LiveInfo.PlayInfo.Title;
                }
                // Dynamic.DynCourBatchUp.Title
                if (Dynamic is { DynCourBatchUp: { } })
                {
                    return Dynamic.DynCourBatchUp.Title;
                }
                // Season.Title
                if (Season is { })
                {
                    return Season.Title;
                }
                // CustomArticle.Title
                if (CustomArticle is { })
                {
                    return CustomArticle.Title;
                }

                return "";
            }
        }

        public IUserDynamicCommands Parent { get; set; }

        public string CardType { get; set; }

        public string ItemType { get; set; }

        public ModuleAuthor Author { get; set; }

        public bool AuthorIsVip
        {
            get
            {
                if (Author?.Author.Vip == null) return false;
                return Author.Author.Vip.Status == 1;
            }
        }

        public ModuleAuthorForward AuthorForward { get; set; }

        public UserDynamicSeasonInfo Season { get; set; }

        public NavDynArticle CustomArticle { get; set; }

        public string SourceJson { get; set; }

        public ModuleDynamic Dynamic
        {
            get
            {
                return m_dynamic;
            }
            set
            {
                m_dynamic = value;
                if (value.DynForward != null)
                {
                    Item = m_mapper.Map<DynamicV2ItemViewModel>(value.DynForward.Item);
                }

                if (value.DynLiveRcmd != null)
                {
                    LiveInfo = JsonConvert.DeserializeObject<DynLiveInfo>(value.DynLiveRcmd.Content);
                }

                if (value.DynCommonLive != null)
                {
                    LiveInfo = new DynLiveInfo()
                    {
                        PlayInfo = new DynLivePlayInfo()
                        {
                            Cover = value.DynCommonLive.Cover,
                            Title = value.DynCommonLive.Title,
                            AreaName = value.DynCommonLive.CoverLabel,
                            WatchedShow = new DynLiveWatchedShow()
                            {
                                TextLarge = value.DynCommonLive.CoverLabel2,
                            }
                        }
                    };
                }
            }
        }

        public DynLiveInfo LiveInfo { get; set; }

        public ModuleDesc Desc { get; set; }

        public ModuleFold Fold { get; set; }

        public ModuleOpusSummary OpusSummary { get; set; }

        [DependsOn(nameof(Item))]
        public bool ForwardMissed => Item == null;

        public DynamicV2ItemViewModel Item { get; set; }

        public List<DynamicV2ItemViewModel> Items
        {
            get
            {
                if (Item == null) return null;
                Item.Parent = Parent;
                return new List<DynamicV2ItemViewModel>()
                {
                    Item
                };
            }
        }

        public ModuleStat Stat
        {
            get => m_stat;
            set
            {
                if (value == null) return;
                m_stat = value;
                Liked = value.LikeInfo.IsLike;
                LikeCount = value.Like;
            }
        }

        public ModuleStat BottomStat
        {
            get => m_stat;
            set
            {
                if (value == null) return;
                m_stat = value;
                Liked = value.LikeInfo.IsLike;
                LikeCount = value.Like;
            }
        }

        public bool Liked { get; set; }

        public long LikeCount { get; set; }

        public Extend Extend { get; set; }

        [DependsOn(nameof(Content))] public bool ShowContent => Desc != null || OpusSummary != null;

        [DoNotNotify]
        public string ContentStr
        {
            get
            {
                if (Extend.OpusSummary != null && Extend.OpusSummary.Summary.Text != null)
                {
                    var text = "";
                    if (Extend.OpusSummary.Title != null)
                    {
                        var title = Extend.OpusSummary.Title.Text.Nodes
                            .Aggregate("", (current, textNode) => current + textNode.RawText);
                        text = title + "\n";
                    }

                    text += Extend.OpusSummary.Summary.Text.Nodes
                        .Aggregate("", (current, textNode) => current + textNode.RawText);

                    return text;
                }

                if (Desc != null)
                {
                    return
                        Desc.Text;
                }

                if (OpusSummary != null)
                {
                    var text = OpusSummary.Summary.Text.Nodes.Aggregate("", (current, textNode) => current + textNode.RawText);

                    return
                        text;
                }

                return "";
            }
        }

        [DependsOn(nameof(Desc), nameof(OpusSummary))]
        public RichTextBlock Content
        {
            get
            {
                try
                {
                    if (Extend.OpusSummary != null && Extend.OpusSummary.Summary.Text != null)
                    {
                        var text = "";
                        if (Extend.OpusSummary.Title != null)
                        {
                            var title = Extend.OpusSummary.Title.Text.Nodes
                                .Aggregate("", (current, textNode) => current + textNode.RawText);
                            text = title + "\n";
                        }

                        text += Extend.OpusSummary.Summary.Text.Nodes
                            .Aggregate("", (current, textNode) => current + textNode.RawText);

                        return
                            text.UserDynamicStringToRichText(
                                Extend.DynIdStr, wordNodes: Extend.OpusSummary?.Summary?.Text?.Nodes?.ToList(),
                                bindingCommands: nameof(Parent));
                    }

                    if (Desc != null)
                    {
                        return
                            Desc.Text.UserDynamicStringToRichText(
                                Extend.DynIdStr, wordNodes: Extend.OpusSummary?.Summary?.Text?.Nodes?.ToList(),
                                bindingCommands: nameof(Parent));
                    }

                    if (OpusSummary != null)
                    {
                        var text = OpusSummary.Summary.Text.Nodes.Aggregate("", (current, textNode) => current + textNode.RawText);

                        return
                            text.UserDynamicStringToRichText(
                                Extend.DynIdStr, wordNodes: Extend.OpusSummary?.Summary?.Text?.Nodes?.ToList(),
                                bindingCommands: nameof(Parent));
                    }

                    return new RichTextBlock();
                }
                catch (Exception ex)
                {
                    return new RichTextBlock();
                }
            }
        }

        public int CoverWidth => 160;

        [DependsOn(nameof(Dynamic))]
        public bool IsShortVideo
        {
            get
            {
                if (Dynamic.DynArchive?.Dimension == null) return false;
                return (Dynamic.DynArchive.Dimension.Height / Dynamic.DynArchive.Dimension.Width) > 1.5;
            }
        }

        [DependsOn(nameof(AuthorForward), nameof(CardType), nameof(ItemType))]
        public bool IsRepost => AuthorForward != null;

        public List<UserDynamicItemDisplayImageInfo> ImageInfos
        {
            get
            {
                var drawItems = this.Dynamic.DynDraw.Items;
                var allImages = drawItems.Select(x => x.Src).ToList();

                return drawItems.Select((t, i) => new UserDynamicItemDisplayImageInfo()
                {
                    AllImages = allImages,
                    Height = t.Height,
                    ImageUrl = t.Src,
                    Width = t.Width,
                    Index = i,
                    ImageCommand = Parent.ImageCommand,
                })
                    .ToList();
            }
        }

        [DoNotNotify]
        public string Verify
        {
            get
            {
                if (Author?.Author?.Official == null)
                    return Constants.App.TRANSPARENT_IMAGE;
                return Author.Author.Official.Type switch
                {
                    1 => Constants.App.VERIFY_OGANIZATION_IMAGE,
                    0 => Constants.App.VERIFY_PERSONAL_IMAGE,
                    -1 => Constants.App.TRANSPARENT_IMAGE,
                    _ => Constants.App.TRANSPARENT_IMAGE
                };
            }
        }

        [DoNotNotify]
        public string Pendant
        {
            get
            {
                if (Author == null) return Constants.App.TRANSPARENT_IMAGE;
                if (Author.Author.Pendant != null && !string.IsNullOrEmpty(Author.Author.Pendant.Image))
                {
                    return Author.Author.Pendant.Image;
                }

                return Constants.App.TRANSPARENT_IMAGE;
            }
        }
    }
}