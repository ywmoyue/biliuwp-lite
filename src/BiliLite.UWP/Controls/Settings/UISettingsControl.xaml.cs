using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp.UI;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class UISettingsControl : UserControl
    {
        public UISettingsControl()
        {
            this.InitializeComponent(); 
            LoadUI();
        }
        private void LoadUI()
        {
            //主题
            cbTheme.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            cbTheme.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbTheme.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.THEME, cbTheme.SelectedIndex);
                    Frame rootFrame = Window.Current.Content as Frame;
                    switch (cbTheme.SelectedIndex)
                    {
                        case 1:
                            rootFrame.RequestedTheme = ElementTheme.Light;
                            break;
                        case 2:
                            rootFrame.RequestedTheme = ElementTheme.Dark;
                            break;
                        //case 3:
                        //    // TODO: 切换自定义主题
                        //    rootFrame.Resources = Application.Current.Resources.ThemeDictionaries["Pink"] as ResourceDictionary;
                        //    break;
                        default:
                            rootFrame.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                    App.ExtendAcrylicIntoTitleBar();
                });
            });


            //显示模式
            cbDisplayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
            cbDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_MODE, cbDisplayMode.SelectedIndex);
                    if (cbDisplayMode.SelectedIndex == 2)
                    {
                        Notify.ShowMessageToast("多窗口模式正在开发测试阶段，可能会有一堆问题");
                    }
                    else
                    {
                        Notify.ShowMessageToast("重启生效");
                    }

                });
            });
            //加载原图
            swPictureQuality.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.ORTGINAL_IMAGE, false);
            swPictureQuality.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPictureQuality.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ORTGINAL_IMAGE, swPictureQuality.IsOn);
                    SettingService.UI.LoadOriginalImage = null;
                });
            });
            //缓存页面
            swHomeCache.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true);
            swHomeCache.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swHomeCache.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.CACHE_HOME, swHomeCache.IsOn);

                });
            });

            // 显示推荐页横幅
            SwitchDisplayRecommendBanner.IsOn = SettingService.GetValue(SettingConstants.UI.DISPLAY_RECOMMEND_BANNER, SettingConstants.UI.DEFAULT_DISPLAY_RECOMMEND_BANNER);
            SwitchDisplayRecommendBanner.Loaded += (sender, e) =>
            {
                SwitchDisplayRecommendBanner.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_RECOMMEND_BANNER, SwitchDisplayRecommendBanner.IsOn);

                };
            };

            //右侧详情宽度
            numRightWidth.Value = SettingService.GetValue<double>(SettingConstants.UI.RIGHT_DETAIL_WIDTH, 320);
            numRightWidth.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numRightWidth.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.RIGHT_DETAIL_WIDTH, args.NewValue);
                });
            });

            //右侧详情宽度可调整
            swRightWidthChangeable.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.RIGHT_WIDTH_CHANGEABLE, false);
            swRightWidthChangeable.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swRightWidthChangeable.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.RIGHT_WIDTH_CHANGEABLE, swRightWidthChangeable.IsOn);
                });
            });

            //视频详情页分集列表设计宽度
            NumListEpisodeDesiredWidth.Value = SettingService.GetValue<double>(SettingConstants.UI.VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH, SettingConstants.UI.DEFAULT_VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH);
            NumListEpisodeDesiredWidth.Loaded += (sender, e) =>
            {
                NumListEpisodeDesiredWidth.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH, args.NewValue);
                };
            };

            //动态评论宽度
            NumBoxDynamicCommentWidth.Value = SettingService.GetValue<double>(SettingConstants.UI.DYNAMIC_COMMENT_WIDTH, SettingConstants.UI.DEFAULT_DYNAMIC_COMMENT_WIDTH);
            NumBoxDynamicCommentWidth.Loaded += (sender, e) =>
            {
                NumBoxDynamicCommentWidth.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DYNAMIC_COMMENT_WIDTH, args.NewValue);
                };
            };

            //图片圆角半径
            numImageCornerRadius.Value = SettingService.GetValue<double>(SettingConstants.UI.IMAGE_CORNER_RADIUS, 0);
            ImageCornerRadiusExample.CornerRadius = new CornerRadius(numImageCornerRadius.Value);
            numImageCornerRadius.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numImageCornerRadius.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.IMAGE_CORNER_RADIUS, args.NewValue);
                    ImageCornerRadiusExample.CornerRadius = new CornerRadius(args.NewValue);
                    App.Current.Resources["ImageCornerRadius"] = new CornerRadius(args.NewValue);
                });
            });

            //显示视频封面
            swVideoDetailShowCover.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.SHOW_DETAIL_COVER, true);
            swVideoDetailShowCover.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swVideoDetailShowCover.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.SHOW_DETAIL_COVER, swVideoDetailShowCover.IsOn);
                });
            });

            //新窗口浏览图片
            swPreviewImageNavigateToPage.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, false);
            swPreviewImageNavigateToPage.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPreviewImageNavigateToPage.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, swPreviewImageNavigateToPage.IsOn);
                });
            });

            // 鼠标中键/侧键行为
            cbMouseMiddleAction.SelectedIndex = SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back);
            cbMouseMiddleAction.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbMouseMiddleAction.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, cbMouseMiddleAction.SelectedIndex);
                });
            });

            // 快速收藏
            SwitchQuickDoFav.IsOn = SettingService.GetValue(SettingConstants.UI.QUICK_DO_FAV, SettingConstants.UI.DEFAULT_QUICK_DO_FAV);
            SwitchQuickDoFav.Loaded += (sender, e) =>
            {
                SwitchQuickDoFav.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.QUICK_DO_FAV, SwitchQuickDoFav.IsOn);
                };
            };

            // 默认收藏夹
            DefaultUseFav.Text = SettingService.GetValue(SettingConstants.UI.DEFAULT_USE_FAV, SettingConstants.UI.DEFAULT_USE_FAV_VALUE);
            DefaultUseFav.Loaded += (sender, e) =>
            {
                DefaultUseFav.TextChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DEFAULT_USE_FAV, DefaultUseFav.Text);
                };
            };

            //动态显示
            cbDetailDisplay.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DETAIL_DISPLAY, 0);
            cbDetailDisplay.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDetailDisplay.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DETAIL_DISPLAY, cbDetailDisplay.SelectedIndex);
                });
            });

            // 启用长评论收起
            swEnableCommentShrink.IsOn = SettingService.GetValue(SettingConstants.UI.ENABLE_COMMENT_SHRINK, true);
            swEnableCommentShrink.Loaded += (sender, e) =>
            {
                swEnableCommentShrink.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ENABLE_COMMENT_SHRINK, swEnableCommentShrink.IsOn);
                };
            };

            // 评论收起长度
            numCommentShrinkLength.Value = SettingService.GetValue(SettingConstants.UI.COMMENT_SHRINK_LENGTH, SettingConstants.UI.COMMENT_SHRINK_DEFAULT_LENGTH);
            numCommentShrinkLength.Loaded += (sender, e) =>
            {
                numCommentShrinkLength.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.COMMENT_SHRINK_LENGTH, (int)numCommentShrinkLength.Value);
                };
            };

            // 展示评论热门回复
            swShowHotReplies.IsOn = SettingService.GetValue(SettingConstants.UI.SHOW_HOT_REPLIES, SettingConstants.UI.DEFAULT_SHOW_HOT_REPLIES);
            swShowHotReplies.Loaded += (sender, e) =>
            {
                swShowHotReplies.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.SHOW_HOT_REPLIES, swShowHotReplies.IsOn);
                };
            };

            //动态显示
            cbDynamicDisplayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0);
            cbDynamicDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDynamicDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, cbDynamicDisplayMode.SelectedIndex);
                });
            });

            //推荐显示
            cbRecommendDisplayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 0);
            cbRecommendDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbRecommendDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.RECMEND_DISPLAY_MODE, cbRecommendDisplayMode.SelectedIndex);
                });
            });

            //浏览器打开无法处理的链接
            swOpenUrlWithBrowser.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.OPEN_URL_BROWSER, false);
            swOpenUrlWithBrowser.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swOpenUrlWithBrowser.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.OPEN_URL_BROWSER, swOpenUrlWithBrowser.IsOn);
                });
            });

            //固定标签宽度
            SwitchTabItemFixedWidth.IsOn =
                SettingService.GetValue(SettingConstants.UI.ENABLE_TAB_ITEM_FIXED_WIDTH,
                    SettingConstants.UI.DEFAULT_ENABLE_TAB_ITEM_FIXED_WIDTH);
            SwitchTabItemFixedWidth.Loaded += (sender, e) =>
            {
                SwitchTabItemFixedWidth.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ENABLE_TAB_ITEM_FIXED_WIDTH, SwitchTabItemFixedWidth.IsOn);
                };
            };

            // 固定标签宽度大小
            NumTabItemFixedWidth.Value = SettingService.GetValue(SettingConstants.UI.TAB_ITEM_FIXED_WIDTH, SettingConstants.UI.DEFAULT_TAB_ITEM_FIXED_WIDTH);
            NumTabItemFixedWidth.Loaded += (sender, e) =>
            {
                NumTabItemFixedWidth.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.TAB_ITEM_FIXED_WIDTH, NumTabItemFixedWidth.Value);
                };
            };

            //显示视频底部进度条
            SwShowVideoBottomProgress.IsOn = SettingService.GetValue(SettingConstants.UI.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR, SettingConstants.UI.DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR);
            SwShowVideoBottomProgress.Loaded += (sender, e) =>
            {
                SwShowVideoBottomProgress.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR, SwShowVideoBottomProgress.IsOn);
                };
            };

            var navItems = SettingService.GetValue(SettingConstants.UI.HOEM_ORDER, DefaultHomeNavItems.GetDefaultHomeNavItems());
            gridHomeCustom.ItemsSource = new ObservableCollection<HomeNavItem>(navItems);
            ExceptHomeNavItems();
        }

        private void ExceptHomeNavItems()
        {
            var defaultNavItems = DefaultHomeNavItems.GetDefaultHomeNavItems();
            var hideNavItems = DefaultHomeNavItems.GetDefaultHideHomeNavItems();
            var customNavItem = gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>;

            var customDontHideNavItems = hideNavItems.Where(hideNavItem =>
                customNavItem.Any(x => x.Title == hideNavItem.Title)).ToList();

            foreach (var customDontHideNavItem in customDontHideNavItems)
            {
                hideNavItems.Remove(customDontHideNavItem);
            }

            foreach (var navItem in defaultNavItems)
            {
                if (!customNavItem.Any(x => x.Title == navItem.Title))
                {
                    hideNavItems.Add(navItem);
                }
            }

            gridHomeNavItem.ItemsSource = hideNavItems;
        }
        private void gridHomeCustom_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (!(gridHomeCustom.ItemsSource is ObservableCollection<HomeNavItem> navItems)) return;
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, navItems.ToList());
            Notify.ShowMessageToast("更改成功,重启生效");
        }

        private void gridHomeNavItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as HomeNavItem;
            if (!(gridHomeCustom.ItemsSource is ObservableCollection<HomeNavItem> navItems)) return;
            navItems.Add(item);
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, navItems.ToList());
            ExceptHomeNavItems();
            Notify.ShowMessageToast("更改成功,重启生效");
        }

        private async void btnCleanImageCache_Click(object sender, RoutedEventArgs e)
        {
            await ImageCache.Instance.ClearAsync();
            Notify.ShowMessageToast("已清除图片缓存");
        }

        private void menuRemoveHomeItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as HomeNavItem;
            if (gridHomeCustom.Items.Count == 1)
            {
                Notify.ShowMessageToast("至少要留一个页面");
                return;
            }
            (gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>).Remove(item);
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>);
            ExceptHomeNavItems();
            Notify.ShowMessageToast("更改成功,重启生效");
        }
    }
}
