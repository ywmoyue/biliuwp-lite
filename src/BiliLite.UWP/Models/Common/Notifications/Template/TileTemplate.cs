using Microsoft.Toolkit.Uwp.Notifications;

namespace BiliLite.Models.Common.Notifications.Template
{
    public class TileTemplate
    {
        private static string GetPeekImageSource(NotificationTile tile) =>
            !string.IsNullOrEmpty(tile.AvatarUrl) ? tile.AvatarUrl : tile.Url;

        public static TileContent LiveTile(NotificationTile notificationTile) => new TileContent()
        {
            // 创建动态磁贴对象并返回：翻转前显示UP主头像，翻转后显示视频封面及标题
            Visual = new TileVisual()
            {
                Branding = TileBranding.NameAndLogo,
                TileMedium = new TileBinding()
                {
                    Content = new TileBindingContentAdaptive()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = notificationTile.Name,
                                HintMaxLines = 3,
                                HintWrap = true,
                            },
                        },
                        PeekImage = new TilePeekImage()
                        {
                            Source = GetPeekImageSource(notificationTile),
                            HintOverlay = 20,
                            HintCrop = TilePeekImageCrop.Circle,
                        },
                        BackgroundImage = new TileBackgroundImage()
                        {
                            Source = notificationTile.Url,
                            HintOverlay = 40,
                        },
                    }
                },
                TileWide = new TileBinding()
                {
                    Content = new TileBindingContentAdaptive()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = notificationTile.Name,
                                HintStyle = AdaptiveTextStyle.Body,
                                HintMaxLines = 2,
                                HintWrap = true,
                            },
                            new AdaptiveText()
                            {
                                Text = notificationTile.Description,
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Left,
                            },
                        },
                        PeekImage = new TilePeekImage()
                        {
                            Source = GetPeekImageSource(notificationTile),
                            HintOverlay = 20,
                            HintCrop = TilePeekImageCrop.Circle,
                        },
                        BackgroundImage = new TileBackgroundImage()
                        {
                            Source = notificationTile.Url,
                            HintOverlay = 50,
                        },
                    }
                },
                TileLarge = new TileBinding()
                {
                    Content = new TileBindingContentAdaptive()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = notificationTile.Name,
                                HintStyle = AdaptiveTextStyle.Subtitle,
                                HintMaxLines = 3,
                                HintWrap = true,
                            },
                            new AdaptiveText() {},
                            new AdaptiveText()
                            {
                                Text = notificationTile.Description,
                                HintStyle = AdaptiveTextStyle.Base,
                                HintAlign = AdaptiveTextAlign.Left,
                            },
                        },
                        PeekImage = new TilePeekImage()
                        {
                            Source = GetPeekImageSource(notificationTile),
                            HintOverlay = 20,
                            HintCrop = TilePeekImageCrop.Circle,
                        },
                        BackgroundImage = new TileBackgroundImage()
                        {
                            Source = notificationTile.Url,
                            HintOverlay = 50,
                        },
                    }
                }
            }
        };
    }
}
