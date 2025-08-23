using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    public static partial class RegisterServiceExtensions
    {
        public static void AddAttributeService(this IServiceCollection services, int displayMode)
        {
            services.AddAutoRegisteredServices(displayMode);
        }
    }
}
