using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.System;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MessagesPage : BasePage
    {
        private readonly MessagesService m_messagesService;
        private readonly MessagesViewModel m_viewModel;

        public MessagesPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<MessagesViewModel>();
            m_messagesService = App.ServiceProvider.GetRequiredService<MessagesService>();
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await m_messagesService.GetChatContexts(m_viewModel);
            if (m_viewModel.ChatContexts.Any())
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
    }
}
