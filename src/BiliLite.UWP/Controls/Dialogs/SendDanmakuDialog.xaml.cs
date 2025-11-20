using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using System;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class SendDanmakuDialog : ContentDialog
    {
        PlayerAPI playerAPI;
        public SendDanmakuDialog(string _aid, string _cid, double _position)
        {
            this.InitializeComponent();
            playerAPI = new PlayerAPI();
            cid = _cid;
            aid = _aid;
            position = Convert.ToInt32(_position);

        }
        public event EventHandler<SendDanmakuModel> DanmakuSended;


        private string cid, aid;
        private int position;
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Send_text_Comment.Text.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("弹幕内容不能为空!");
                return;
            }
            if (!SettingService.Account.Logined)
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {



                int modeInt = 1;
                if (Send_cb_Mode.SelectedIndex == 2)
                {
                    modeInt = 4;
                }
                if (Send_cb_Mode.SelectedIndex == 1)
                {
                    modeInt = 5;
                }
                var result = await playerAPI.SendDanmu(aid, cid, ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(), Send_text_Comment.Text, position).Request();
                if (!result.status)
                {
                    NotificationShowExtensions.ShowMessageToast("弹幕发送失败" + result.message);
                    return;
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() == 0)
                {
                    if (DanmakuSended != null)
                    {
                        DanmakuSended(this, new SendDanmakuModel()
                        {
                            location = modeInt,
                            color = ((ComboBoxItem)Send_cb_Color.SelectedItem).Tag.ToString(),
                            text = Send_text_Comment.Text
                        });
                    }
                    NotificationShowExtensions.ShowMessageToast("弹幕成功发射");

                    Send_text_Comment.Text = string.Empty;
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast("弹幕发送失败" + obj["message"].ToString());
                }
            }
            catch (Exception ex)
            {

                NotificationShowExtensions.ShowMessageToast("发送弹幕发生错误！\r\n" + ex.HResult);
            }


        }
    }
    public class SendDanmakuModel
    {
        public string text { get; set; }
        public string color { get; set; }
        public int location { get; set; }
    }
}
