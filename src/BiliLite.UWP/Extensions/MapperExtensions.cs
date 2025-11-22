using System;
using System.Collections.Generic;
using System.Linq;
using Atelier39;
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
using BiliLite.Models.Common.Msg;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Common.Settings;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.Detail;
using BiliLite.Models.Databases;
using BiliLite.Models.Download;
using BiliLite.Models.Dynamic;
using BiliLite.Models.Functions;
using BiliLite.Modules.User.UserDetail;
using BiliLite.Services;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
using BiliLite.ViewModels.Messages;
using BiliLite.ViewModels.Plugins;
using BiliLite.ViewModels.Season;
using BiliLite.ViewModels.Settings;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.UserDynamic;
using BiliLite.ViewModels.Video;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using NSDanmaku.Model;
using DanmakuMode = Atelier39.DanmakuMode;
using DynamicType = Bilibili.App.Dynamic.V2.DynamicType;

namespace BiliLite.Extensions
{
    public static class MapperExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static IServiceCollection AddMapper(this IServiceCollection services)
        {
            // 配置类型映射
            TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);

            // 双向映射
            TypeAdapterConfig<WebSocketPlugin, WebSocketPluginViewModel>.NewConfig()
                .TwoWays();
            TypeAdapterConfig<DownloadedItemDTO, DownloadedItem>.NewConfig()
                .TwoWays();
            TypeAdapterConfig<DownloadedSubItemDTO, DownloadedSubItem>.NewConfig()
                .TwoWays();
            TypeAdapterConfig<FilterRule, FilterRuleViewModel>.NewConfig()
                .TwoWays();

            // 单向映射
            TypeAdapterConfig<DownloadItem, DownloadItemViewModel>.NewConfig();
            TypeAdapterConfig<DownloadEpisodeItem, DownloadEpisodeItemViewModel>.NewConfig();
            TypeAdapterConfig<DataCommentModel, DataCommentViewModel>.NewConfig();
            TypeAdapterConfig<CommentContentModel, CommentContentViewModel>.NewConfig();
            TypeAdapterConfig<VideoDetailModel, VideoDetailViewModel>.NewConfig();
            TypeAdapterConfig<VideoDetailStaffModel, VideoDetailStaffViewModel>.NewConfig();
            TypeAdapterConfig<VideoDetailStatModel, VideoDetailStatViewModel>.NewConfig();
            TypeAdapterConfig<VideoDetailRelatesModel, VideoDetailRelatesViewModel>.NewConfig();
            TypeAdapterConfig<VideoDetailReqUserModel, VideoDetailReqUserViewModel>.NewConfig();
            TypeAdapterConfig<SeasonDetailUserStatusModel, SeasonDetailUserStatusViewModel>.NewConfig();
            TypeAdapterConfig<SeasonDetailModel, SeasonDetailViewModel>.NewConfig();
            TypeAdapterConfig<AnimeFallModel, AnimeFallViewModel>.NewConfig();
            TypeAdapterConfig<HomeNavItem, HomeNavItemViewModel>.NewConfig();
            TypeAdapterConfig<FollowTlistItemModel, UserRelationFollowingTagViewModel>.NewConfig();
            TypeAdapterConfig<VideoListSection, VideoListSectionViewModel>.NewConfig();
            TypeAdapterConfig<VideoPlaylistItem, VideoListItem>.NewConfig();
            TypeAdapterConfig<FavoriteItemModel, FavoriteItemViewModel>.NewConfig();
            TypeAdapterConfig<CinemaHomeModel, CinemaHomeViewModel>.NewConfig();
            TypeAdapterConfig<CinemaHomeFallModel, CinemaHomeFallViewModel>.NewConfig();
            TypeAdapterConfig<SeasonShortReviewItemModel, SeasonShortReviewItemViewModel>.NewConfig();
            TypeAdapterConfig<SeasonShortReviewItemStatModel, SeasonShortReviewItemStatViewModel>.NewConfig();
            TypeAdapterConfig<BiliDanmakuItem, DanmakuModel>.NewConfig();
            TypeAdapterConfig<NewEP, UserDynamicSeasonNewEpInfo>.NewConfig();
            TypeAdapterConfig<FollowListItem, UserDynamicSeasonInfo>.NewConfig();
            TypeAdapterConfig<BaseShortcutFunction, ShortcutFunctionModel>.NewConfig();
            TypeAdapterConfig<ShortcutFunctionViewModel, ShortcutFunctionModel>.NewConfig();
            TypeAdapterConfig<DanmakuItem, DanmakuSimpleItem>.NewConfig();
            TypeAdapterConfig<DanmakuModel, DanmakuSimpleItem>.NewConfig();

            // 复杂映射配置
            TypeAdapterConfig<CommentItem, HotReply>.NewConfig()
                .Map(dest => dest.UserName, src => src.Member.Uname)
                .Map(dest => dest.Message, src => src.Content.Message)
                .Map(dest => dest.Emote, src => src.Content.Emote);

            TypeAdapterConfig<CommentItem, CommentViewModel>.NewConfig()
                .Map(dest => dest.HotReplies, src => src.Replies);

            TypeAdapterConfig<MediaListItem, VideoListItem>.NewConfig()
                .Map(dest => dest.Author, src => src.Upper.Name)
                .Map(dest => dest.Duration, src => TimeSpan.FromSeconds(src.Duration));

            var danmakuModeConvertDic = new Dictionary<DanmakuLocation, DanmakuMode>()
    {
        { DanmakuLocation.Scroll, DanmakuMode.Rolling },
        { DanmakuLocation.Top, DanmakuMode.Top },
        { DanmakuLocation.Bottom, DanmakuMode.Bottom },
        { DanmakuLocation.Position, DanmakuMode.Unknown },
        { DanmakuLocation.Other, DanmakuMode.Unknown },
    };

            TypeAdapterConfig<BiliDanmakuItem, DanmakuItem>.NewConfig()
                .Map(dest => dest.BaseFontSize, src => src.Size)
                .Map(dest => dest.TextColor, src => src.Color)
                .Map(dest => dest.StartMs, src => src.Time)
                .Map(dest => dest.Mode, src => danmakuModeConvertDic.GetValueOrDefault(src.Location));

            TypeAdapterConfig<DownloadSaveEpisodeInfo, DownloadedSubItem>.NewConfig()
                .Map(dest => dest.Paths, src => new List<string>())
                .Map(dest => dest.Title, src => src.EpisodeTitle)
                .Map(dest => dest.SubtitlePath, src => new List<DownloadSubtitleInfo>());

            TypeAdapterConfig<Arc, SubmitVideoItemModel>.NewConfig()
                .Map(dest => dest.Play, src => src.Archive.Stat.View)
                .Map(dest => dest.Pic, src => src.Archive.Pic)
                .Map(dest => dest.Title, src => src.Archive.Title.Replace("<em class=\"keyword\">", "").Replace("</em>", ""))
                .Map(dest => dest.Length, src => src.Archive.Duration.ProgressToTime())
                .Map(dest => dest.Aid, src => src.Archive.Aid)
                .Map(dest => dest.Created, src => src.Archive.Ctime)
                .Map(dest => dest.VideoReview, src => src.Archive.Stat.Danmaku)
                .Map(dest => dest.RedirectUrl, src => src.Archive.RedirectUrl);

            TypeAdapterConfig<SubmitVideoCursorItem, SubmitVideoItemModel>.NewConfig()
                .Map(dest => dest.Pic, src => src.Cover)
                .Map(dest => dest.Length, src => src.Duration.ProgressToTime())
                .Map(dest => dest.Aid, src => src.Aid)
                .Map(dest => dest.Created, src => src.CTime)
                .Map(dest => dest.VideoReview, src => src.Danmaku);

            TypeAdapterConfig<DynamicCardDescModel, UserDynamicItemDisplayViewModel>.NewConfig()
                .Map(dest => dest.CommentCount, src => src.comment)
                .Map(dest => dest.Datetime, src => TimeExtensions.TimestampToDatetime(src.timestamp).ToString())
                .Map(dest => dest.DynamicID, src => src.dynamic_id)
                .Map(dest => dest.LikeCount, src => src.like)
                .Map(dest => dest.Mid, src => src.uid)
                .Map(dest => dest.ShareCount, src => src.repost)
                .Map(dest => dest.Time, src => src.timestamp.HandelTimestamp())
                .Map(dest => dest.IntType, src => src.type)
                .Map(dest => dest.ReplyID, src => src.rid_str)
                .Map(dest => dest.ReplyType, src => src.r_type)
                .Map(dest => dest.Type, src => DynamicParseExtensions.ParseType(src.type))
                .Map(dest => dest.IsSelf, src => src.uid == SettingService.Account.UserID)
                .Map(dest => dest.Liked, src => src.is_liked == 1);

            TypeAdapterConfig<FollowListItem, UserDynamicItemDisplayViewModel>.NewConfig()
                .Map(dest => dest.Type, src => UserDynamicDisplayType.SeasonV2)
                .Map(dest => dest.ContentDisplayInfo, src => new UserDynamicSeasonDisplayInfo()
                {
                    Url = src.Url,
                    Cover = src.Cover,
                    SubTitle = src.SubTitle,
                    Title = src.Title,
                });

            TypeAdapterConfig<DynamicItem, DynamicV2ItemViewModel>.NewConfig()
                .Map(dest => dest.Author, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthor).ModuleAuthor)
                .Map(dest => dest.AuthorForward, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleAuthorForward).ModuleAuthorForward)
                .Map(dest => dest.Dynamic, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDynamic).ModuleDynamic)
                .Map(dest => dest.Desc, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleDesc).ModuleDesc)
                .Map(dest => dest.OpusSummary, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleOpusSummary).ModuleOpusSummary)
                .Map(dest => dest.Stat, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleStat).ModuleStat)
                .Map(dest => dest.BottomStat, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleBottom).ModuleButtom.ModuleStat)
                .Map(dest => dest.Fold, src => src.Modules.FirstOrDefault(x => x.ModuleType == DynModuleType.ModuleFold).ModuleFold)
                .Map(dest => dest.SourceJson, src => src.ToString());

            TypeAdapterConfig<BaseShortcutFunction, ShortcutFunctionViewModel>.NewConfig()
                .Map(dest => dest.IsPressAction, src => src.ReleaseFunction != null);

            TypeAdapterConfig<BiliMessageSession, ChatContextViewModel>.NewConfig()
                .Map(dest => dest.Title, src => src.AccountInfo == null ? src.GroupName : src.AccountInfo.Name)
                .Map(dest => dest.Cover, src => src.AccountInfo == null ? src.GroupCover : src.AccountInfo.PicUrl)
                .Map(dest => dest.ChatContextId, src => src.TalkerId)
                .Map(dest => dest.HasNotify, src => src.BizMsgUnreadCount > 0)
                .Map(dest => dest.UnreadMsgCount, src => src.UnreadCount)
                .Map(dest => dest.LastMsg, src => src.LastMsg)
                .Map(dest => dest.Type, src => src.SessionType)
                .Map(dest => dest.Time, src => src.SessionTs)
                .Map(dest => dest.FromUserId, src => src.LastMsg.SenderUid);

            TypeAdapterConfig<BiliSessionPrivateMessage, ChatMessage>.NewConfig()
                .Map(dest => dest.UserId, src => src.SenderUid)
                .Map(dest => dest.ChatMessageId, src => src.MsgSeqno)
                .Map(dest => dest.Time, src => DateTimeOffset.FromUnixTimeSeconds(src.Timestamp))
                .Map(dest => dest.ContentStr, src => src.Content);

            TypeAdapterConfig<BiliReplyMeData, ReplyMeMessageViewModel>.NewConfig()
                .Map(dest => dest.UserId, src => src.User.Mid)
                .Map(dest => dest.UserFace, src => src.User.Avatar)
                .Map(dest => dest.UserName, src => src.User.Nickname)
                .Map(dest => dest.Title, src => src.Item.Title)
                .Map(dest => dest.Content, src => src.Item.SourceContent)
                .Map(dest => dest.ReferenceContent, src => src.Item.TargetReplyContent)
                .Map(dest => dest.HasLike, src => src.Item.LikeState != 0)
                .Map(dest => dest.Time, src => DateTimeOffset.FromUnixTimeSeconds(src.ReplyTime))
                .Map(dest => dest.Url, src => src.Item.NativeUri);

            TypeAdapterConfig<DanmakuItem, DanmakuSimpleItem>.NewConfig()
                .Map(dest => dest.ProgressMs, src => src.StartMs)
                .Map(dest => dest.Id, src => src.Id.ToString())
                .Map(dest => dest.Content, src => src.Text);

            TypeAdapterConfig<DanmakuModel, DanmakuSimpleItem>.NewConfig()
                .Map(dest => dest.ProgressMs, src => src.time * 1000)
                .Map(dest => dest.Id, src => src.rowID)
                .Map(dest => dest.MidHash, src => src.sendID)
                .Map(dest => dest.Content, src => src.text);

            // 验证配置
            TypeAdapterConfig.GlobalSettings.Compile();

            // 注册 Mapster 的 IMapper 服务
            services.AddSingleton<IMapper>(new MapsterMapper.Mapper());

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
