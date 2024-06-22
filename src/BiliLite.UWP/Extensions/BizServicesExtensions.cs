using BiliLite.Services.Biz;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class BizServicesExtensions
    {
        public static IServiceCollection AddBizServices(this IServiceCollection services)
        {
            services.AddTransient<MediaListService>();
            return services;
        }
    }
}
