using BiliLite.ViewModels;
using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
using BiliLite.ViewModels.Live;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.User.SendDynamic;
using BiliLite.ViewModels.UserDynamic;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class ViewModelExtensions
    {
        public static IServiceCollection AddViewModels(this IServiceCollection services)
        {
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<DownloadPageViewModel>();
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
            return services;
        }
    }
}
