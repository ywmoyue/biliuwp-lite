using System;
using System.IO;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using Windows.Storage;
using BiliLite.Extensions;
using Microsoft.UI.Xaml.Media.Imaging;
using CommunityToolkit.WinUI.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Other
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MarkdownViewerPage : Page
    {
        private string m_mdFileForderPath;

        public MarkdownViewerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!(e.Parameter is MarkdownViewerPagerParameter parameter)) return;
            if (parameter.Type == MarkdownViewerPagerParameterType.Content)
            {
                MdBlock.Text = parameter.Value;
            }
            else
            {
                m_mdFileForderPath = Path.GetDirectoryName(parameter.Value);
                MdBlock.Text = await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri(parameter.Value)));
            }
        }

        private async void MdBlock_OnLinkClicked(object sender, LinkClickedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(e.Uri);
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
