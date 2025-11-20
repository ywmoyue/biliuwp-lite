using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Models.Requests.Api;
using BiliLite.ViewModels;
using BiliLite.ViewModels.User.SendDynamic;
using BiliLite.ViewModels.UserDynamic;
using System;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using BiliLite.Extensions;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class SendDynamicDialog : ContentDialog
    {
        readonly EmoteViewModel emoteVM;
        readonly AtViewModel atVM;
        readonly SendDynamicViewModel m_viewModel;
        readonly TopicViewModel topicVM;
        public SendDynamicDialog(SendDynamicViewModel sendDynamicViewModel, EmoteViewModel emoteViewModel, AtViewModel atViewModel, TopicViewModel topicVm)
        {
            m_viewModel = sendDynamicViewModel;
            emoteVM = emoteViewModel;
            atVM = atViewModel;
            topicVM = topicVm;
            this.InitializeComponent();
        }

        public void SetRepost(UserDynamicItemDisplayViewModel userDynamicItem)
        {
            m_viewModel.SetRepost(userDynamicItem);
        }

        private void btn_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private async void btnEmoji_Click(object sender, RoutedEventArgs e)
        {
            FaceFlyout.ShowAt(sender as Button);
            if (emoteVM.Packages == null || emoteVM.Packages.Count == 0)
            {
                await emoteVM.GetEmote(EmoteBusiness.dynamic);
            }
        }

        private void gvEmoji_ItemClick(object sender, ItemClickEventArgs e)
        {
            txtContent.Text += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        private void txtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtLength.Text = (233 - txtContent.Text.Length).ToString();
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            atVM.Search(sender.Text);
        }

        private void listAt_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as AtUserModel;
            var location = txtContent.Text.Length;
            var at = "[@" + data.UserName + "]";
            txtContent.Text += at;

            m_viewModel.AddAtItem(new AtDisplayModel()
            {
                Data = data.ID,
                Text = at,
                Location = location,
                Length = at.Length
            });
        }

        private async void btnAt_Click(object sender, RoutedEventArgs e)
        {
            AtFlyout.ShowAt(sender as Button);
            if (atVM.Users.Count == 0 && string.IsNullOrEmpty(atVM.Keyword))
            {
                await atVM.GetUser();
            }
        }

        private async void btnTopic_Click(object sender, RoutedEventArgs e)
        {
            TopicFlyout.ShowAt(sender as Button);
            if (topicVM.Items == null || topicVM.Items.Count == 0)
            {
                await topicVM.GetTopic();
            }
        }

        private void listTopic_ItemClick(object sender, ItemClickEventArgs e)
        {
            txtContent.Text += (e.ClickedItem as RcmdTopicModel).display;
            TopicFlyout.Hide();
        }

        private void TextTopic_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            txtContent.Text += args.QueryText;
            TopicFlyout.Hide();
        }

        private void btn_RemovePic_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.Images.Remove((sender as Button).DataContext as UploadImagesModel);
            m_viewModel.ShowImage = gv_Pics.Items.Count > 0;
        }

        private async void btnImage_Click(object sender, RoutedEventArgs e)
        {
            if (m_viewModel.Images.Count == 9)
            {
                NotificationShowExtensions.ShowMessageToast("只能上传9张图片哦");
                return;
            }
            var picker = FileExtensions.GetFileOpenPicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                m_viewModel.UploadImage(file);
            }
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.Content = txtContent.Text;
            bool result = false;
            btnSend.IsEnabled = false;
            if (m_viewModel.IsRepost)
            {
                result = await m_viewModel.SendRepost();
            }
            else
            {
                result = await m_viewModel.SendDynamic();
            }
            if (result)
            {
                this.Hide();
            }
            else
            {
                btnSend.IsEnabled = true;
            }

        }
    }
}
