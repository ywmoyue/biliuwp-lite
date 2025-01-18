using System;
using Windows.UI;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using Windows.UI.Xaml.Media;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.ViewModels.Messages;

public class ReplyMeMessageViewModel : BaseViewModel
{
    [DoNotNotify]
    public string Id { get; set; }

    [DoNotNotify]
    public string UserId { get; set; }

    [DoNotNotify]
    public string UserFace { get; set; }

    [DoNotNotify]
    public string UserName { get; set; }

    // 回复我的消息内容
    [DoNotNotify]
    public string Content { get; set; }

    [DoNotNotify]
    public string Title { get; set; }

    // 我的消息内容
    [DoNotNotify]
    public string ReferenceContent { get; set; }

    [DoNotNotify]
    public bool ShowReferenceContent => !string.IsNullOrEmpty(ReferenceContent);

    [DoNotNotify]
    public string Url { get; set; }

    [DoNotNotify]
    public DateTimeOffset Time { get; set; }

    [DoNotNotify]
    public string TimeStr => Time.ToString("yyyy年M月d日 HH:mm");

    public bool HasLike { get; set; }

    [DependsOn(nameof(HasLike))]
    public Brush LikeColor => !HasLike ? new SolidColorBrush(Colors.Gray) :
        new SolidColorBrush((Color)App.ServiceProvider.GetRequiredService<ThemeService>().ThemeResource["SystemAccentColor"]);

}