using BiliLite.Dialogs;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class ControlsExtensions
    {
        public static IServiceCollection AddControls(this IServiceCollection services)
        {
            services.AddTransient<SendDynamicDialog>();
            services.AddTransient<SendDynamicV2Dialog>();
            return services;
        }
    }
}
