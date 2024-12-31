using BiliLite.Models.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace BiliLite.Extensions
{
    public static class RegisterServiceExtensions
    {
        public static void AddAttributeService(this IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in types)
            {
                if (type.GetCustomAttributes(typeof(RegisterSingletonServiceAttribute), false).Any())
                {
                    services.AddSingleton(type);
                }

                if (type.GetCustomAttributes(typeof(RegisterTransientServiceAttribute), false).Any())
                {
                    services.AddTransient(type);
                }
            }
        }
    }
}
