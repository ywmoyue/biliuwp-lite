//using Microsoft.Toolkit.Uwp.Notifications;

//namespace BiliLite.Models.Common.Notifications.Template
//{
//    public class TileTemplate
//    {
//        public static TileContent LiveTile(NotificationTile notificationTile) => new TileContent()
//        {
//            // 创建静态磁贴对象并返回
//            Visual = new TileVisual()
//            {
//                Branding = TileBranding.NameAndLogo,
//                TileMedium = new TileBinding()
//                {
//                    Content = new TileBindingContentAdaptive()
//                    {
//                        Children =
//                        {
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Description,
//                                HintMaxLines = 3,
//                                HintWrap = true,
//                            },
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Name,
//                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
//                                HintAlign = AdaptiveTextAlign.Center,
//                            },
//                        },
//                        PeekImage = new TilePeekImage()
//                        {
//                            Source = notificationTile.Url,
//                            HintOverlay = 30,
//                        },
//                    }
//                },
//                TileWide = new TileBinding()
//                {
//                    Content = new TileBindingContentAdaptive()
//                    {
//                        Children =
//                        {
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Description,
//                                HintStyle = AdaptiveTextStyle.Body,
//                                HintMaxLines = 2,
//                                HintWrap = true,
//                            },
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Name,
//                                HintAlign = AdaptiveTextAlign.Center,
//                            },
//                        },
//                        BackgroundImage = new TileBackgroundImage()
//                        {
//                            Source = notificationTile.Url,
//                        },
//                    }
//                },
//                TileLarge = new TileBinding()
//                {
//                    Content = new TileBindingContentAdaptive()
//                    {
//                        Children =
//                        {
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Description,
//                                HintStyle = AdaptiveTextStyle.Subtitle,
//                                HintMaxLines = 5,
//                                HintWrap = true,
//                            },
//                            new AdaptiveText() {},
//                            new AdaptiveText()
//                            {
//                                Text = notificationTile.Name,
//                                HintStyle = AdaptiveTextStyle.Base,
//                                HintAlign = AdaptiveTextAlign.Center,
//                            },
//                        },
//                        BackgroundImage = new TileBackgroundImage()
//                        {
//                            Source = notificationTile.Url,
//                        },
//                    }
//                }
//            }
//        };
//    }
//}
