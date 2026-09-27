using System;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using Windows.Storage;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Other
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MarkdownViewerPage : Page
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private string m_mdFileForderPath;

        public MarkdownViewerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var parameter = GetParameter(e.Parameter);
            if (parameter == null) return;
            if (parameter.Type == MarkdownViewerPagerParameterType.Content)
            {
                MdBlock.Text = parameter.Value;
            }
            else
            {
                try
                {
                    m_mdFileForderPath = Path.GetDirectoryName(parameter.Value);
                    MdBlock.Text = await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri(parameter.Value)));
                }
                catch (Exception ex)
                {
                    NotificationShowExtensions.ShowMessageToast("帮助页加载失败");
                    logger.Log("加载Markdown文件失败", LogType.Fatal, ex);
                }
            }
        }

        /// <summary>
        /// 恢复上次打开的标签页时，参数是 JsonConvert.DeserializeObject&lt;object&gt; 得到的 JObject，
        /// 直接按 MarkdownViewerPagerParameter 判断会失败导致页面空白，这里转回具体类型
        /// </summary>
        private static MarkdownViewerPagerParameter GetParameter(object value)
        {
            if (value is MarkdownViewerPagerParameter parameter) return parameter;

            if (value == null) return null;

            try
            {
                return JsonConvert.DeserializeObject<MarkdownViewerPagerParameter>(JsonConvert.SerializeObject(value));
            }
            catch
            {
                return null;
            }
        }

        private async void MdBlock_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (MarkdownLinkExtensions.TryHandleCustomLink(e.Link)) return;

            if (Uri.TryCreate(e.Link, UriKind.Absolute, out var uri))
            {
                await Windows.System.Launcher.LaunchUriAsync(uri);
            }
        }

        //private void MdBlock_OnImageResolving(object sender, ImageResolvingEventArgs e)
        //{
        //    if (e.Url.IsUrl(UriKind.Absolute))
        //    {
        //        e.Image = new BitmapImage(new Uri(e.Url));
        //    }
        //    else if (e.Url.IsUrl(UriKind.Relative) && m_mdFileForderPath != null)
        //    {
        //        e.Image = new BitmapImage(new Uri(Path.Combine(m_mdFileForderPath, e.Url)));
        //    }

        //    e.Handled = true;
        //}
    }
}
