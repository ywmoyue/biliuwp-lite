﻿using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Requests.Api.User;
using System;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class EditFavFolderDialog : ContentDialog
    {
        readonly FavoriteApi favoriteApi;
        readonly string id;
        public EditFavFolderDialog(string id, string title, string desc, bool isOpen)
        {
            this.InitializeComponent();
            favoriteApi = new FavoriteApi();
            this.id = id;
            txtTitle.Text = title;
            txtDesc.Text = desc;
            checkPrivacy.IsChecked = isOpen;
        }
        public bool Success { get; set; } = false;
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(txtTitle.Text))
            {
                NotificationShowExtensions.ShowMessageToast("请输入收藏夹名称");
                return;
            }
            try
            {
                IsPrimaryButtonEnabled = false;
                var result = await favoriteApi.EditFavorite(txtTitle.Text, txtDesc.Text, checkPrivacy.IsChecked.Value, id).Request();
                if (result.status)
                {
                    var data = await result.GetData<object>();
                    if (data.success)
                    {
                        NotificationShowExtensions.ShowMessageToast("修改成功");
                        Success = true;
                        this.Hide();
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(data.message);
                    }

                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(result.message);
                }


            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            finally
            {
                IsPrimaryButtonEnabled = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

    }
}
