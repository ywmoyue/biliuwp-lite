using BiliLite.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.ViewModels.Season;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class SendReviewDialog : ContentDialog
    {
        private readonly SeasonReviewViewModel m_viewModel;

        public SendReviewDialog(int mediaId)
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<SeasonReviewViewModel>();
            m_viewModel.MediaID = mediaId;
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(txtBoxContent.Text.Trim()))
            {
                Notify.ShowMessageToast("内容不能为空");
                return;
            }
            int score = (int)rating.Value * 2;
            var result = await m_viewModel.SendShortReview(txtBoxContent.Text, checkShare.IsChecked.Value, score);
            if (result)
            {
                this.Hide();
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private void RatingControl_ValueChanged(Microsoft.UI.Xaml.Controls.RatingControl sender, object args)
        {
            if (rating.Value >= 5)
            {
                txtTips.Visibility = Visibility.Visible;
            }
            else
            {
                txtTips.Visibility = Visibility.Collapsed;
            }
            rating.Caption = (rating.Value * 2).ToString("0");
        }
    }
}
