using BiliLite.Models.Common.Comment;
using BiliLite.Models.Requests.Api;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class CommentDialog : UserControl
    {
        private Popup popup;
        public CommentDialog()
        {
            this.InitializeComponent();
            popup = new Popup();
            this.Width = Window.Current.Bounds.Width;
            this.Height = Window.Current.Bounds.Height;
            popup.Child = this;
            this.Loaded += CommentDialog_Loaded;
            this.Unloaded += CommentDialog_Unloaded;
        }

        private void CommentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }
        public void Show(string oid, int commentMode, CommentApi.CommentSort commentSort)
        {
            this.popup.IsOpen = true;
            comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = commentMode,
                CommentSort = commentSort,
                Oid = oid,
                IsDialog = true
            }, true);
        }
        public async void Close()
        {
            await RootBorder.Fade(value: 0, duration: 200, delay: 0, easingType: EasingType.Default, easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseOut).StartAsync();
            this.popup.IsOpen = false;
        }
        private async void CommentDialog_Loaded(object sender, RoutedEventArgs e)
        {


            Window.Current.SizeChanged += Current_SizeChanged;

            await RootBorder.Fade(value: 1, duration: 200, delay: 0, easingType: EasingType.Default, easingMode: Windows.UI.Xaml.Media.Animation.EasingMode.EaseIn).StartAsync();


        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.Width = e.Size.Width;
            this.Height = e.Size.Height;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.Close();
        }

        private void RootBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
