using BiliLite.Services;
using BiliLite.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static class QrCodeExtensions
    {
        public static IServiceCollection AddQrCodeService(this IServiceCollection services)
        {
#if ARM64
            services.AddTransient<IQrCodeService, QrCoderQrCodeService>();
#else
            services.AddTransient<IQrCodeService, ZXingQrCodeService>();
#endif
            return services;
        }
    }
}
