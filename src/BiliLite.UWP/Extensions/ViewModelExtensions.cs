using BiliLite.ViewModels.Comment;
using BiliLite.ViewModels.Download;
using BiliLite.ViewModels.Home;
using BiliLite.ViewModels.Live;
using BiliLite.ViewModels.User;
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
            return services;
        }
    }
}
