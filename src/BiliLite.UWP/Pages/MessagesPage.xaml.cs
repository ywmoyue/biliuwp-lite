using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Msg;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;

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
            var chatContexts = await m_messagesService.GetChatContexts(m_viewModel);
            m_viewModel.ChatContexts = new ObservableCollection<ChatContextViewModel>(chatContexts);
        }

        private async void ChatContextListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChatContextListView.SelectedValue is not ChatContextViewModel chatContext) return;
            var chatMessages = await m_messagesService.GetChatMessages(chatContext);
            m_viewModel.ChatMessages = new ObservableCollection<ChatMessage>(chatMessages);
            ScrollToLatestMessage();
        }

        private void ScrollToLatestMessage()
        {
            if (m_viewModel.ChatMessages.Count <= 0) return;
            var lastItem = m_viewModel.ChatMessages.Last();
            ChatMessageListView.ScrollIntoView(lastItem);
        }

        private void LoadMore_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
