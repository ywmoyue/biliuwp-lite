using BiliLite.Models.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace BiliLite.Extensions
{
    public static class RegisterServiceExtensions
    {
        public static void AddAttributeService(this IServiceCollection services, int displayMode)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            foreach (var type in types)
            {
                // Handle RegisterSingletonUIServiceAttribute
                var uiServiceAttr = type.GetCustomAttribute<RegisterSingletonUIServiceAttribute>();
                if (uiServiceAttr != null)
                {
                    if (displayMode == 2)
                    {
                        services.AddTransient(type);
                        if (uiServiceAttr.SuperType != null)
                        {
                            services.AddTransient(uiServiceAttr.SuperType, type);
                        }
                    }
                    else
                    {
                        services.AddSingleton(type);
                        if (uiServiceAttr.SuperType != null)
                        {
                            services.AddSingleton(uiServiceAttr.SuperType, type);
                        }
                    }
                }

                // Handle RegisterSingletonServiceAttribute
                var singletonAttr = type.GetCustomAttribute<RegisterSingletonServiceAttribute>();
                if (singletonAttr != null)
                {
                    services.AddSingleton(type);
                    if (singletonAttr.SuperType != null)
                    {
                        services.AddSingleton(singletonAttr.SuperType, type);
                    }
                }

                // Handle RegisterTransientServiceAttribute
                var transientAttr = type.GetCustomAttribute<RegisterTransientServiceAttribute>();
                if (transientAttr != null)
                {
                    services.AddTransient(type);
                    if (transientAttr.SuperType != null)
                    {
                        services.AddTransient(transientAttr.SuperType, type);
                    }
                }
            }

        }
    }
}
