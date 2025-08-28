using System.Linq;
using BiliLite.Models.Attributes;
using BiliLite.ViewModels;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
using BiliLite.ViewModels.Live;
using BiliLite.ViewModels.Rank;
using BiliLite.ViewModels.Search;
using BiliLite.ViewModels.Settings;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.User.SendDynamic;
using BiliLite.ViewModels.UserDynamic;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BiliLite.Extensions
{
    public static partial class ViewModelExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services, int displayMode)
        {
            services.AddSingleton<HomeViewModel>();
            services.AddTransient<DownloadDialogViewModel>();
            services.AddTransient<CommentControlViewModel>();
            services.AddTransient<UserSubmitVideoViewModel>();
            services.AddTransient<UserSubmitCollectionViewModel>();
            services.AddTransient<RecommendPageViewModel>();
            services.AddTransient<DynamicPageViewModel>();
            services.AddTransient<AnimePageViewModel>();
            services.AddTransient<LiveDetailPageViewModel>();
            services.AddTransient<HistoryViewModel>();
            services.AddTransient<UserDetailViewModel>();
            services.AddTransient<UserFollowingTagsFlyoutViewModel>();
            services.AddTransient<UserAttentionButtonViewModel>();
            services.AddTransient<UserDynamicSpaceViewModel>();
            services.AddTransient<SendDynamicViewModel>();
            services.AddTransient<SendDynamicV2ViewModel>();
            services.AddTransient<EmoteViewModel>();
            services.AddTransient<AtViewModel>();
            services.AddTransient<TopicViewModel>();
            services.AddTransient<UserDynamicAllViewModel>();
            services.AddTransient<PlayerToastViewModel>();
            services.AddTransient<VideoListViewModel>();
            services.AddTransient<CollectedPageViewModel>();
            services.AddTransient<RankViewModel>();
            services.AddTransient<EditPlaySpeedMenuViewModel>();
            services.AddTransient<SendCommentDialogViewModel>();
            services.AddTransient<FilterSettingsViewModel>();
            services.AddTransient<LiveSettingsControlViewModel>();
            services.AddTransient<PlaySettingsControlViewModel>();
            services.AddTransient<VideoDanmakuSettingsControlViewModel>();
            services.AddTransient<ShortcutKeySettingsControlViewModel>();
            services.AddTransient<DevSettingsControlViewModel>();
            services.AddTransient<MainPageViewModel>();
            services.AddTransient<SearchPageViewModel>();

            // 存在CI编译问题，暂时仅X64架构启用该方法
#if X64
            services.AddAutoRegisteredViewModels(displayMode);
#else
            services.AddAttributeViewModel(displayMode);
#endif

            return services;
        }

        private static void AddAttributeViewModel(this IServiceCollection services, int displayMode)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in types)
            {
                if (type.GetCustomAttributes(typeof(RegisterSingletonViewModelAttribute), false).Any())
                {
                    if (displayMode == 2)
                    {
                        services.AddTransient(type);
                    }
                    else
                    {
                        services.AddSingleton(type);
                    }
                }

                if (type.GetCustomAttributes(typeof(RegisterTransientViewModelAttribute), false).Any())
                {
                    services.AddTransient(type);
                }
            }
        }
    }
}
