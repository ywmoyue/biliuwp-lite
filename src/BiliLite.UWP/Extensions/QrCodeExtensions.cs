using BiliLite.Services;
using BiliLite.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class QrCodeExtensions
    {
        public static IServiceCollection AddQrCodeService(this IServiceCollection services)
        {
            services.AddTransient<IQrCodeService, QrCoderQrCodeService>();
            return services;
        }
    }
}
