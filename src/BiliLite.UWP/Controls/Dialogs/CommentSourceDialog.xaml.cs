using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class CommentSourceDialog : ContentDialog
    {
        public string CommentSource { get; set; }

        public CommentSourceDialog(string commentSource)
        {
            CommentSource = commentSource;
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
