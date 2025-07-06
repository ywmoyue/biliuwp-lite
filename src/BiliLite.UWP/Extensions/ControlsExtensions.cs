using BiliLite.Controls;
using BiliLite.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using BiliLite.Pages;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Extensions
{
    public static class ControlsExtensions
    {
        public static IServiceCollection AddControls(this IServiceCollection services)
        {
            var mode = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
            if (mode == 0)
            {
                services.AddSingleton<IMainPage, MainPage>();
            }
            else
            {
                services.AddSingleton<IMainPage, NoTabMainPage>();
            }
            services.AddTransient<SendDynamicDialog>();
            services.AddTransient<SendDynamicV2Dialog>();
            services.AddTransient<EditPlaySpeedMenuDialog>();
            services.AddTransient<PlayerToast>();
            services.AddTransient<VideoListView>();
            return services;
        }

        public static T FindFirstChildByType<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }

                var subChild = FindFirstChildByType<T>(child);
                if (subChild != null) return subChild;
            }

            return null;
        }

        public static IEnumerable<T> FindChildrenByType<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield break;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    yield return typedChild;
                }

                foreach (var descendant in FindChildrenByType<T>(child))
                {
                    yield return descendant;
                }
            }
        }

        /// <summary>
        /// 在可视化树中根据x:Name查找元素
        /// </summary>
        /// <param name="parent">起始父元素</param>
        /// <param name="name">要查找的元素的x:Name</param>
        /// <returns>找到的元素，如果未找到则返回null</returns>
        public static FrameworkElement FindChildByElementName(this DependencyObject parent, string name)
        {
            if (parent == null || string.IsNullOrEmpty(name)) return null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                // 检查是否是FrameworkElement且有匹配的Name
                if (child is FrameworkElement frameworkElement && frameworkElement.Name.ToString() == name)
                {
                    return frameworkElement;
                }

                // 递归查找子元素
                var foundChild = FindChildByElementName(child, name);
                if (foundChild != null) return foundChild;
            }

            return null;
        }

        public static bool CheckFocusTextBoxNow()
        {
            var elent = FocusManager.GetFocusedElement();
            return elent is TextBox || elent is AutoSuggestBox;
        }
    }
}
