using BiliLite.Models;
using Microsoft.Toolkit.Uwp.Notifications;

namespace BiliLite.Services.Notification.Template
{
    internal class TileTemplate
    {
        public static TileContent LiveTile(TileModel tileModel) => new TileContent()
        {
            // 创建静态磁贴对象并返回
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
                                Text = tileModel.Description,
                                HintMaxLines = 3,
                                HintWrap = true,
                            },
                            new AdaptiveText()
                            {
                                Text = tileModel.Name,
                                HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                HintAlign = AdaptiveTextAlign.Center,
                            },
                        },
                        PeekImage = new TilePeekImage()
                        {
                            Source = tileModel.Url,
                            HintOverlay = 30,
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
                                Text = tileModel.Description,
                                HintStyle = AdaptiveTextStyle.Body,
                                HintMaxLines = 2,
                                HintWrap = true,
                            },
                            new AdaptiveText()
                            {
                                Text = tileModel.Name,
                                HintAlign = AdaptiveTextAlign.Center,
                            },
                        },
                        BackgroundImage = new TileBackgroundImage()
                        {
                            Source = tileModel.Url,
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
                                Text = tileModel.Description,
                                HintStyle = AdaptiveTextStyle.Subtitle,
                                HintMaxLines = 5,
                                HintWrap = true,
                            },
                            new AdaptiveText() {},
                            new AdaptiveText()
                            {
                                Text = tileModel.Name,
                                HintStyle = AdaptiveTextStyle.Base,
                                HintAlign = AdaptiveTextAlign.Center,
                            },
                        },
                        BackgroundImage = new TileBackgroundImage()
                        {
                            Source = tileModel.Url,
                        },
                    }
                }
            }
        };
    }
}
