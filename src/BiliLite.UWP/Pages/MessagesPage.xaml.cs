using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Services.Biz;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels;
using BiliLite.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MessagesPage : BasePage, IUpdatePivotLayout
    {
        private readonly MessagesService m_messagesService;
        private readonly MessagesViewModel m_viewModel;
        private readonly EmoteViewModel m_emoteViewModel;

        public MessagesPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<MessagesViewModel>();
            m_emoteViewModel = App.ServiceProvider.GetService<EmoteViewModel>();
            m_messagesService = App.ServiceProvider.GetRequiredService<MessagesService>();
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (m_viewModel.ChatContexts == null)
            {
                await m_messagesService.GetChatContexts(m_viewModel);
            }

            if (m_viewModel.ChatMessages != null) return;
            if (m_viewModel.ChatContexts != null && m_viewModel.ChatContexts.Any())
            {
                ChatContextListView.SelectedIndex = 0;
            }
        }

        private async void ChatContextListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatContextListView.SelectedValue is not ChatContextViewModel chatContext) return;
            await m_messagesService.GetChatMessages(m_viewModel, chatContext);
            ScrollToLatestMessage();
#if !DEBUG
            await m_messagesService.SetReaded(m_viewModel, chatContext);
#endif
        }

        private void ScrollToLatestMessage()
        {
            if (m_viewModel.ChatMessages.Count <= 0) return;
            var lastItem = m_viewModel.ChatMessages.Last();
            ChatMessageListView.ScrollIntoView(lastItem);
        }

        private async void LoadMore_OnClick(object sender, RoutedEventArgs e)
        {
            await m_messagesService.GetChatContexts(m_viewModel, true);
        }

        private async void LoadMoreMessages_OnClick(object sender, RoutedEventArgs e)
        {
            var currentMsg = m_viewModel.ChatMessages.First();
            await m_messagesService.GetChatMessages(m_viewModel, m_viewModel.SelectedChatContext, true);
            ChatMessageListView.ScrollIntoView(currentMsg);
        }

        private async void RefreshContexts_OnClick(object sender, RoutedEventArgs e)
        {
            var currentContextId = m_viewModel.SelectedChatContext.ChatContextId;
            await m_messagesService.GetChatContexts(m_viewModel);
            var context = m_viewModel.ChatContexts.FirstOrDefault(x => x.ChatContextId == currentContextId);
            if (context != null)
            {
                var index = m_viewModel.ChatContexts.IndexOf(context);
                ChatContextListView.SelectedIndex = index;
            }
            else
            {
                ChatContextListView.SelectedIndex = 0;
            }
        }

        private async void RefreshMessages_OnClick(object sender, RoutedEventArgs e)
        {
            await m_messagesService.GetChatMessages(m_viewModel, m_viewModel.SelectedChatContext);
            ScrollToLatestMessage();
        }

        private async void OpenWeb_OnClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://message.bilibili.com/"));
        }

        private async void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            await m_messagesService.SendTextMsg(m_viewModel, m_viewModel.SelectedChatContext,
                m_viewModel.ChatMessageInput);
            ScrollToLatestMessage();
        }

        private async void BtnOpenFace_Click(object sender, RoutedEventArgs e)
        {
            FaceFlyout.ShowAt(sender as Button);
            if (m_emoteViewModel.Packages == null || m_emoteViewModel.Packages.Count == 0)
            {
                await m_emoteViewModel.GetEmote(EmoteBusiness.reply);
            }
        }

        private async void BtnSendImage_Click(object sender, RoutedEventArgs e)
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".png");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".gif");
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return;
            using var openFile = await file.OpenAsync(FileAccessMode.Read);
            using var stream = openFile.AsStreamForRead();
            var bin = new byte[stream.Length];

            await stream.ReadAsync(bin, 0, bin.Length);
            var fileInfo = new UploadFileInfo()
            {
                Data = bin,
                FileName = file.Name,
            };
            await m_messagesService.SendImageMsg(m_viewModel, m_viewModel.SelectedChatContext, fileInfo);
            ScrollToLatestMessage();
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            m_viewModel.ChatMessageInput += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
