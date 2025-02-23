using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.ViewModels;
using BiliLite.ViewModels.Comment;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using static BiliLite.Models.Requests.Api.CommentApi;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class SendCommentDialog : ContentDialog
    {
        readonly CommentApi commentApi;
        readonly EmoteViewModel emoteVM;
        readonly string oid;
        readonly CommentType commentType;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly SendCommentDialogViewModel m_viewModel;

        public SendCommentDialog(string oid, CommentType commentType)
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<SendCommentDialogViewModel>();
            emoteVM = App.ServiceProvider.GetService<EmoteViewModel>();
            this.InitializeComponent();
            commentApi = new CommentApi();
            this.oid = oid;
            this.commentType = commentType;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(txt_Comment.Text.Trim()))
            {
                NotificationShowExtensions.ShowMessageToast("检查下你的输入哦");
                return;
            }
            try
            {
                IsPrimaryButtonEnabled = false;
                var text = txt_Comment.Text;
                var result = await commentApi.AddComment(oid, commentType, text, m_viewModel.Pictures.ToList()).Request();
                var data = await result.GetData<object>();
                if (data.code == 0)
                {
                    NotificationShowExtensions.ShowMessageToast("发表评论成功");
                    this.Hide();

                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(data.message.ToString());
                }

            }
            catch (Exception)
            {
                IsPrimaryButtonEnabled = true;
                NotificationShowExtensions.ShowMessageToast("发送评论失败");
                // throw;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void btnOpenFace_Click(object sender, RoutedEventArgs e)
        {
            FaceFlyout.ShowAt(sender as Button);
            if (emoteVM.Packages == null || emoteVM.Packages.Count == 0)
            {
                await emoteVM.GetEmote(EmoteBusiness.reply);
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            txt_Comment.Text += (e.ClickedItem as EmotePackageItemModel).text.ToString();
        }

        private async void BtnUploadDraw_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var filePicker = new FileOpenPicker();
                filePicker.FileTypeFilter.Add(".jpg");
                filePicker.FileTypeFilter.Add(".png");
                filePicker.FileTypeFilter.Add(".jpeg");
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

                var api = commentApi.UploadDraw(fileInfo);
                var result = await api.Request();
                if (!result.status)
                    throw new CustomizedErrorException(result.message);
                var uploadDrawResult = await result.GetData<DynamicPicture>();
                if (!uploadDrawResult.success)
                    throw new CustomizedErrorException(uploadDrawResult.message);
                m_viewModel.AddPicture(uploadDrawResult.data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
        }

        private void BtnRemovePicture_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: DynamicPicture picture })
            {
                m_viewModel.RemovePicture(picture);
            }
        }
    }
}
