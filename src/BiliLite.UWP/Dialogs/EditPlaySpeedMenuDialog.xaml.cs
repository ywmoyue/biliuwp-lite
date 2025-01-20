using BiliLite.Models.Common.Player;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class EditPlaySpeedMenuDialog : ContentDialog
    {
        public double DialogHeight => Window.Current.Bounds.Height * 0.7;

        private readonly EditPlaySpeedMenuViewModel m_viewModel;
        private readonly PlaySpeedMenuService m_playSpeedMenuService;

        public EditPlaySpeedMenuDialog(EditPlaySpeedMenuViewModel viewModel, PlaySpeedMenuService playSpeedMenuService)
        {
            m_viewModel = viewModel;
            m_playSpeedMenuService = playSpeedMenuService;
            m_viewModel.PlaySpeedMenuItems = new ObservableCollection<PlaySpeedMenuItem>(m_playSpeedMenuService.MenuItems);
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            m_playSpeedMenuService.SetMenuItems(m_viewModel.PlaySpeedMenuItems.ToList());
        }

        private void BtnRemovePlaySpeed_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is Button btn)) return;
            if (!(btn.DataContext is PlaySpeedMenuItem menuItem)) return;

            m_viewModel.PlaySpeedMenuItems.Remove(menuItem);
        }

        private void BtnAddPlaySpeed_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            m_viewModel.PlaySpeedMenuItems.Add(new PlaySpeedMenuItem(m_viewModel.AddPlaySpeedValue));
            m_viewModel.PlaySpeedMenuItems =
                new ObservableCollection<PlaySpeedMenuItem>(m_viewModel.PlaySpeedMenuItems.OrderByDescending(x => x.Value));
        }
    }
}
