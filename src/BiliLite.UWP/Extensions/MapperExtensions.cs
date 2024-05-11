using System.Collections.Generic;
using System.Linq;
using Atelier39;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using Bilibili.App.Interface.V1;
using Bilibili.Tv.Interfaces.Dm.V1;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Common.Dynamic;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.User.UserDetails;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Download;
using BiliLite.Models.Dynamic;
using BiliLite.Modules.User.UserDetail;
using BiliLite.Services;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
using BiliLite.ViewModels.Season;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.UserDynamic;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;
using NSDanmaku.Model;
using DanmakuMode = Atelier39.DanmakuMode;
using DynamicType = Bilibili.App.Dynamic.V2.DynamicType;

namespace BiliLite.Extensions
{
    public static class MapperExtensions
    {
        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            var mapper = new Mapper(new MapperConfiguration(expression =>
            {
                expression.CreateMap<DownloadItem, DownloadItemViewModel>();
                expression.CreateMap<DownloadEpisodeItem, DownloadEpisodeItemViewModel>();
                expression.CreateMap<CommentItem, HotReply>()
                    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Member.Uname))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Content.Message))
                    .ForMember(dest => dest.Emote, opt => opt.MapFrom(src => src.Content.Emote));
                expression.CreateMap<CommentItem, CommentViewModel>()
                    .ForMember(dest => dest.HotReplies, opt => opt.MapFrom(src => src.Replies));
                expression.CreateMap<DataCommentModel, DataCommentViewModel>();
                expression.CreateMap<CommentContentModel, CommentContentViewModel>();
                expression.CreateMap<VideoDetailModel, VideoDetailViewModel>();
                expression.CreateMap<VideoDetailStaffModel, VideoDetailStaffViewModel>();
                expression.CreateMap<VideoDetailStatModel, VideoDetailStatViewModel>();
                expression.CreateMap<VideoDetailRelatesModel, VideoDetailRelatesViewModel>();
                expression.CreateMap<VideoDetailReqUserModel, VideoDetailReqUserViewModel>();
                expression.CreateMap<SeasonDetailUserStatusModel, SeasonDetailUserStatusViewModel>();
                expression.CreateMap<SeasonDetailModel, SeasonDetailViewModel>();
                expression.CreateMap<AnimeFallModel, AnimeFallViewModel>();
                expression.CreateMap<HomeNavItem, HomeNavItemViewModel>();
                expression.CreateMap<UserCenterInfoModel, UserCenterInfoViewModel>();
                expression.CreateMap<FollowTlistItemModel, UserRelationFollowingTagViewModel>();
                expression.CreateMap<VideoListSection, VideoListSectionViewModel>();
                expression.CreateMap<VideoPlaylistItem,VideoListItem>();

                var danmakuModeConvertDic = new Dictionary<DanmakuLocation, DanmakuMode>()
                {
                    { DanmakuLocation.Scroll, DanmakuMode.Rolling },
                    { DanmakuLocation.Top, DanmakuMode.Top },
                    { DanmakuLocation.Bottom, DanmakuMode.Bottom },
                    { DanmakuLocation.Position, DanmakuMode.Unknown },
                    { DanmakuLocation.Other, DanmakuMode.Unknown },
                };
                expression.CreateMap<BiliDanmakuItem, DanmakuModel>();
                expression.CreateMap<BiliDanmakuItem, DanmakuItem>()
                    .ForMember(dest => dest.BaseFontSize, opt => opt.MapFrom(src => src.Size))
                    .ForMember(dest => dest.TextColor, opt => opt.MapFrom(src => src.Color))
                    .ForMember(dest => dest.StartMs, opt => opt.MapFrom(src => src.Time))
                    .ForMember(dest => dest.Mode, opt => opt.MapFrom(src => danmakuModeConvertDic.GetValueOrDefault(src.Location)));

                expression.CreateMap<DownloadSaveEpisodeInfo, DownloadedSubItem>()
                    .ForMember(dest => dest.Paths, opt => opt.MapFrom(src => new List<string>()))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.EpisodeTitle))
                    .ForMember(dest => dest.SubtitlePath, opt => opt.MapFrom(src => new List<DownloadSubtitleInfo>()));

                expression.CreateMap<Arc, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Play, opt => opt.MapFrom(src => src.Archive.Stat.View))
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Archive.Pic))
                    .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Archive.Title.Replace("<em class=\"keyword\">", "").Replace("</em>", "")))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Archive.Duration.ProgressToTime()))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Archive.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.Archive.Ctime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Archive.Stat.Danmaku))
                    .ForMember(dest => dest.RedirectUrl, opt => opt.MapFrom(src => src.Archive.RedirectUrl));

                expression.CreateMap<SubmitVideoCursorItem, SubmitVideoItemModel>()
                    .ForMember(dest => dest.Pic, opt => opt.MapFrom(src => src.Cover))
                    .ForMember(dest => dest.Length, opt => opt.MapFrom(src => src.Duration.ProgressToTime()))
                    .ForMember(dest => dest.Aid, opt => opt.MapFrom(src => src.Aid))
                    .ForMember(dest => dest.Created, opt => opt.MapFrom(src => src.CTime))
                    .ForMember(dest => dest.VideoReview, opt => opt.MapFrom(src => src.Danmaku));

                expression.CreateMap<DynamicCardDescModel, UserDynamicItemDisplayViewModel>()
                    .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.comment))
                    .ForMember(dest => dest.Datetime, opt => opt.MapFrom(src => TimeExtensions.TimestampToDatetime(src.timestamp).ToString()))
                    .ForMember(dest => dest.DynamicID, opt => opt.MapFrom(src => src.dynamic_id))
                    .ForMember(dest => dest.LikeCount, opt => opt.MapFrom(src => src.like))
                    .ForMember(dest => dest.Mid, opt => opt.MapFrom(src => src.uid))
                    .ForMember(dest => dest.ShareCount, opt => opt.MapFrom(src => src.repost))
                    .ForMember(dest => dest.Time, opt => opt.MapFrom(src => src.timestamp.HandelTimestamp()))
                    .ForMember(dest => dest.IntType, opt => opt.MapFrom(src => src.type))
                    .ForMember(dest => dest.ReplyID, opt => opt.MapFrom(src => src.rid_str))
                    .ForMember(dest => dest.ReplyType, opt => opt.MapFrom(src => src.r_type))
                    .ForMember(dest => dest.Type, opt => opt.MapFrom(src => DynamicParseExtensions.ParseType(src.type)))
                    .ForMember(dest => dest.IsSelf, opt => opt.MapFrom(src => src.uid == SettingService.Account.UserID))
                    .ForMember(dest => dest.Liked, opt => opt.MapFrom(src => src.is_liked == 1));

                expression.CreateMap<FollowListItem, UserDynamicItemDisplayViewModel>()
                    .ForMember(dest => dest.Type, opt => opt.MapFrom(src => UserDynamicDisplayType.SeasonV2))
                    .ForMember(dest => dest.ContentDisplayInfo, opt => opt.MapFrom(src =>
                        new UserDynamicSeasonDisplayInfo()
                        {
                            Url = src.Url,
                            Cover = src.Cover,
                            SubTitle = src.SubTitle,
                            Title = src.Title,
                        }));

                expression.CreateMap<DynamicItem, DynamicV2ItemViewModel>()
                    .ForMember(dest => dest.Author,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthor).ModuleAuthor))
                    .ForMember(dest => dest.AuthorForward,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthorForward).ModuleAuthorForward))
                    .ForMember(dest => dest.Dynamic,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDynamic).ModuleDynamic))
                    .ForMember(dest => dest.Desc,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDesc).ModuleDesc))
                    .ForMember(dest => dest.OpusSummary,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleOpusSummary).ModuleOpusSummary))
                    .ForMember(dest => dest.Stat,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleStat).ModuleStat))
                    .ForMember(dest => dest.Fold,
                        opt => opt.MapFrom(src =>
                            src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleFold).ModuleFold))
                    .ForMember(dest => dest.SourceJson,
                        opt => opt.MapFrom(src =>
                            src.ToString()));

                expression.CreateMap<NewEP, UserDynamicSeasonNewEpInfo>();
                expression.CreateMap<FollowListItem, UserDynamicSeasonInfo>();
            }));

            services.AddSingleton<IMapper>(mapper);
            return services;
        }

        public static List<DanmakuItem> MapToDanmakuItems(this IEnumerable<DanmakuElem> elems)
        {
            var danmakuItems = new List<DanmakuItem>();
            foreach (var danmakuElem in elems)
            {
                var danmakuItem = new DanmakuItem()
                {
                    Id = (ulong)danmakuElem.Id,
                    Text = danmakuElem.Content,
                    StartMs = (uint)danmakuElem.Progress,
                    BaseFontSize = danmakuElem.Fontsize,
                    Mode = (DanmakuMode)danmakuElem.Mode,
                    TextColor = danmakuElem.Color.ParseColor(),
                    Weight = danmakuElem.Weight,
                    MidHash = danmakuElem.MidHash
                };
                danmakuItem.ParseAdvanceDanmaku();
                danmakuItems.Add(danmakuItem);
            }
            return danmakuItems.OrderBy(x => x.StartMs).ToList();
        }

        public static List<DynamicItemModel> MapToDynamicItemModels(this IEnumerable<DynamicItem> dynamicItems)
        {
            var dynamicItemModels = new List<DynamicItemModel>();
            foreach (var src in dynamicItems)
            {
                var type = 0;
                switch (src.CardType)
                {
                    case DynamicType.Av:
                        type = 8;
                        break;
                    case DynamicType.Pgc:
                        type = 512;
                        break;
                    case DynamicType.UgcSeason:
                        type = 4310;
                        break;
                }

                var moduleAuthor = src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthor);
                var moduleDynamic = src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDynamic);


                // 处理特殊情况：类型为番剧但是数据为普通视频
                if (moduleDynamic != null && type == 512 && moduleDynamic.ModuleDynamic.DynArchive != null &&
                    moduleDynamic.ModuleDynamic.DynPgc == null)
                {
                    type = 8;
                }

                var dynDesc = new DynamicDescModel()
                {
                    Type = type,
                    DynamicId = src.Extend.DynIdStr,
                    TimeText = moduleAuthor.ModuleAuthor.PtimeLabelText
                };
                var dynItemModel = new DynamicItemModel()
                {
                    Desc = dynDesc,
                };

                switch (type)
                {
                    case 8:
                        {
                            var dynVideoStat = new DynamicVideoCardStatModel()
                            {
                                View = moduleDynamic.ModuleDynamic.DynArchive.View
                            };
                            var dynOwner = new DynamicVideoCardOwnerModel()
                            {
                                Face = src.Extend.OrigFace,
                                Name = src.Extend.OrigName,
                            };
                            var dynVideo = new DynamicVideoCardModel()
                            {
                                Aid = moduleDynamic.ModuleDynamic
                                    .DynArchive.Avid.ToString(),
                                Duration = moduleDynamic.ModuleDynamic.DynArchive.Duration,
                                Pic = src.Extend.OrigImgUrl,
                                Title = moduleDynamic.ModuleDynamic.DynArchive.Title,
                                Stat = dynVideoStat,
                                Owner = dynOwner,
                                SeasonId = moduleDynamic.ModuleDynamic.DynArchive.PgcSeasonId,
                                ViewCountText = moduleDynamic.ModuleDynamic.DynArchive.CoverLeftText2,
                                DanmakuCountText = moduleDynamic.ModuleDynamic.DynArchive.CoverLeftText3,
                            };
                            if (string.IsNullOrEmpty(dynVideo.Title))
                            {
                                dynVideo.Title = src.Extend.OrigDesc.FirstOrDefault()?.Text;
                            }
                            dynItemModel.Video = dynVideo;
                            break;
                        }
                    case 512:
                        {
                            var seasonInfo = new DynamicSeasonCardApiSeasonInfoModel()
                            {
                                Title = src.Extend.OrigName,
                                SeasonId = moduleDynamic.ModuleDynamic.DynPgc.SeasonId,
                            };
                            var dynSeason = new DynamicSeasonCardModel()
                            {
                                Aid = moduleDynamic.ModuleDynamic.DynPgc.Aid.ToString(),
                                Cover = moduleDynamic.ModuleDynamic.DynPgc.Cover,
                                Season = seasonInfo,
                                NewDesc = src.Extend.OrigDesc.FirstOrDefault()?.Text,
                            };
                            dynItemModel.Season = dynSeason;
                            break;
                        }
                    case 4310:
                    {
                        var dynUgcSeason = new DynamicUgcSeasonCardModel()
                        {
                            Aid = moduleDynamic.ModuleDynamic
                                .DynUgcSeason.Avid.ToString(),
                            Duration = moduleDynamic.ModuleDynamic.DynUgcSeason.Duration,
                            Pic = moduleDynamic.ModuleDynamic
                                .DynUgcSeason.Cover,
                            Title = moduleDynamic.ModuleDynamic.DynUgcSeason.Title,
                        };

                        dynItemModel.UgcSeason = dynUgcSeason;
                        break;
                    }
                }

                dynamicItemModels.Add(dynItemModel);
            }

            return dynamicItemModels;
        }
    }
}
