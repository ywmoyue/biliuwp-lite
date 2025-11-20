using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Settings;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class UISettingsControl : UserControl
    {
        private readonly ThemeService m_themeService;
        private readonly UISettingsControlViewModel m_UISettingsControlViewModel;
        private readonly SettingSqlService m_settingSqlService;

        public UISettingsControl()
        {
            m_themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
            m_settingSqlService = App.ServiceProvider.GetService<SettingSqlService>();
            m_UISettingsControlViewModel = App.ServiceProvider.GetRequiredService<UISettingsControlViewModel>();
            InitializeComponent();
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
                    var themeIndex = cbTheme.SelectedIndex;
                    if (themeIndex > 2)
                        m_themeService.SetTheme(ElementTheme.Default);
                    m_themeService.SetTheme((ElementTheme)themeIndex);
                });
            });

            //自带色彩
            gvColor.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.THEME_COLOR, SettingConstants.UI.DEFAULT_THEME_COLOR);
            gvColor.Loaded += (sender, e) =>
            {
                gvColor.SelectionChanged += (obj, args) =>
                {
                    m_UISettingsControlViewModel.ResetIsActived(gvColor.SelectedIndex);

                    if (gvColor.SelectedIndex >= 0)
                    {
                        var selectedItem = gvColor.SelectedItem as ColorItemModel;
                        m_themeService.SetColor(selectedItem.Color);
                    }
                    else
                    {
                        m_themeService.SetColor();
                    }

                    m_settingSqlService.SetValue(SettingConstants.UI.THEME_COLOR_MENU, m_UISettingsControlViewModel.Colors);
                    SettingService.SetValue(SettingConstants.UI.THEME_COLOR, gvColor.SelectedIndex);
                };
            };

            //系统色彩
            btnSysColor.Click += (sender, e) =>
            {
                gvColor.SelectedIndex = -1;
            };

            //自定义色彩
            btnAddColor.Click += (sender, e) =>
            {
                if (m_UISettingsControlViewModel.Colors.Any(item => item.Color == cpAddColor.Color))
                {
                    NotificationShowExtensions.ShowMessageToast("已重复添加");
                    return;
                }

                var color = cpAddColor.Color;
                var hexCode = color.ToString();
                var name = string.IsNullOrEmpty(tbAddColorName.Text) ? tbAddColorName.PlaceholderText : tbAddColorName.Text;
                var isActived = cbSetColor.IsChecked.GetValueOrDefault();
                ColorItemModel colorItemModel = new(isActived, name, hexCode, color);
                m_UISettingsControlViewModel.Colors.Add(colorItemModel);
                if (isActived)
                    gvColor.SelectedIndex = m_UISettingsControlViewModel.Colors.Count - 1;

                m_settingSqlService.SetValue(SettingConstants.UI.THEME_COLOR_MENU, m_UISettingsControlViewModel.Colors);
                NotificationShowExtensions.ShowMessageToast($"已添加：{name} {hexCode}");
            };

            //显示模式
            cbDisplayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
            cbDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_MODE, cbDisplayMode.SelectedIndex);
                    if (cbDisplayMode.SelectedIndex == 2)
                    {
                        NotificationShowExtensions.ShowMessageToast("多窗口模式正在开发测试阶段，可能会有一堆问题");
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast("重启生效");
                    }

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

            // 显示直播页横幅
            SwitchDisplayLiveBanner.IsOn = SettingService.GetValue(SettingConstants.UI.DISPLAY_LIVE_BANNER, true);
            SwitchDisplayLiveBanner.Loaded += (sender, e) =>
            {
                SwitchDisplayLiveBanner.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_LIVE_BANNER, SwitchDisplayLiveBanner.IsOn);

                };
            };

            // 显示直播页推荐直播
            SwitchDisplayLivePageRecommendLive.IsOn = !SettingService.GetValue(SettingConstants.UI.DISPLAY_LIVE_PAGE_RECOMMEND_LIVE, true);
            SwitchDisplayLivePageRecommendLive.Loaded += (sender, e) =>
            {
                SwitchDisplayLivePageRecommendLive.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_LIVE_PAGE_RECOMMEND_LIVE, !SwitchDisplayLivePageRecommendLive.IsOn);
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

            //动态磁贴
            SwitchTile.IsOn = SettingService.GetValue(SettingConstants.UI.ENABLE_NOTIFICATION_TILES, false);
            SwitchTile.Loaded += (sender, e) =>
            {
                SwitchTile.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ENABLE_NOTIFICATION_TILES, SwitchTile.IsOn);
                    if (SwitchTile.IsOn)
                    {
                        RegisterBackgroundTask();
                    }
                    else
                    {
                        // TODO: UnregisterBackgroundTask 
                    }
                };
            };

            //显示视频封面
            swVideoDetailShowCover.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.SHOW_DETAIL_COVER, true);
            swVideoDetailShowCover.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swVideoDetailShowCover.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.SHOW_DETAIL_COVER, swVideoDetailShowCover.IsOn);
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

            // 显示网页链接原文
            SwDisplayLinkSource.IsOn = SettingService.GetValue(SettingConstants.UI.DISPLAY_LINK_SOURCE, SettingConstants.UI.DEFAULT_DISPLAY_LINK_SOURCE);
            SwDisplayLinkSource.Loaded += (sender, e) =>
            {
                SwDisplayLinkSource.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.DISPLAY_LINK_SOURCE, SwDisplayLinkSource.IsOn);
                };
            };

            // 发评反诈
            SwCommAntifraud.IsOn = SettingService.GetValue(SettingConstants.UI.COMM_ANTIFRAUD, SettingConstants.UI.DEFAULT_COMM_ANTIFRAUD);
            SwCommAntifraud.Loaded += (sender, e) =>
            {
                SwCommAntifraud.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.COMM_ANTIFRAUD, SwCommAntifraud.IsOn);
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

            // 标签最小宽度
            NumTabItemMinWidth.Value = SettingService.GetValue(SettingConstants.UI.TAB_ITEM_MIN_WIDTH, SettingConstants.UI.DEFAULT_TAB_ITEM_MIN_WIDTH);
            NumTabItemMinWidth.Loaded += (sender, e) =>
            {
                NumTabItemMinWidth.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.TAB_ITEM_MIN_WIDTH, NumTabItemMinWidth.Value);
                };
            };

            // 标签最大宽度
            NumTabItemMaxWidth.Value = SettingService.GetValue(SettingConstants.UI.TAB_ITEM_MAX_WIDTH, SettingConstants.UI.DEFAULT_TAB_ITEM_MAX_WIDTH);
            NumTabItemMaxWidth.Loaded += (sender, e) =>
            {
                NumTabItemMaxWidth.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.TAB_ITEM_MAX_WIDTH, NumTabItemMaxWidth.Value);
                };
            };

            // 标签高度
            NumTabHeight.Value = SettingService.GetValue(SettingConstants.UI.TAB_HEIGHT, SettingConstants.UI.DEFAULT_TAB_HEIGHT);
            NumTabHeight.Loaded += (sender, e) =>
            {
                NumTabHeight.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.TAB_HEIGHT, NumTabHeight.Value);
                };
            };

            //新窗口浏览图片
            swPreviewImageNavigateToPage.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, false);
            swPreviewImageNavigateToPage.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPreviewImageNavigateToPage.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, swPreviewImageNavigateToPage.IsOn);
                });
            });

            swPreviewImageNavigateToPageFully.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.NEW_FULLY_WINDOW_PREVIEW_IMAGE, true);
            swPreviewImageNavigateToPageFully.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPreviewImageNavigateToPageFully.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.NEW_FULLY_WINDOW_PREVIEW_IMAGE, swPreviewImageNavigateToPageFully.IsOn);
                });
            });

            //CbMicaBackgroundSource.Loaded += (_, _) =>
            //{
            //    //CbMicaBackgroundSource.SelectedIndex =
            //    //    m_UISettingsControlViewModel.MicaBackgroundSources.FindIndex(x =>
            //    //        x.Value == m_UISettingsControlViewModel.MicaBackgroundSource);
            //};

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
            NotificationShowExtensions.ShowMessageToast("更改成功,重启生效");
        }

        private void gridHomeNavItem_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as HomeNavItem;
            if (!(gridHomeCustom.ItemsSource is ObservableCollection<HomeNavItem> navItems)) return;
            navItems.Add(item);
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, navItems.ToList());
            ExceptHomeNavItems();
            NotificationShowExtensions.ShowMessageToast("更改成功,重启生效");
        }

        private void menuRemoveHomeItem_Click(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as HomeNavItem;
            if (gridHomeCustom.Items.Count == 1)
            {
                NotificationShowExtensions.ShowMessageToast("至少要留一个页面");
                return;
            }
            (gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>).Remove(item);
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, gridHomeCustom.ItemsSource as ObservableCollection<HomeNavItem>);
            ExceptHomeNavItems();
            NotificationShowExtensions.ShowMessageToast("更改成功,重启生效");
        }

        private void RegisterBackgroundTask()
        {
            NotificationRegisterExtensions.BackgroundTask("DisposableTileFeedBackgroundTask");
            NotificationRegisterExtensions.BackgroundTask("TileFeedBackgroundTask", new TimeTrigger(15, false));
            //NotificationRegisterExtensions.BackgroundTask("TileFeedBackgroundTask",
            //    "BackgroundTasks.TileFeedBackgroundTask", new TimeTrigger(15, false));
        }

        private void ColorItemMenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement menuFlyoutItem = sender as FrameworkElement;
            var clickedItem = menuFlyoutItem.DataContext;
            switch (menuFlyoutItem.Tag as string)
            {
                case "delete":
                    m_UISettingsControlViewModel.Colors.Remove(clickedItem as ColorItemModel);
                    break;
            }
            m_UISettingsControlViewModel.ResetIsActived(gvColor.SelectedIndex);

            m_settingSqlService.SetValue(SettingConstants.UI.THEME_COLOR_MENU, m_UISettingsControlViewModel.Colors);
        }

        private async void UpdateMicaSettings(object sender, object e)
        {
            //await Task.Delay(50);
            //m_themeService.SetMicaBrushBackgroundSource(m_UISettingsControlViewModel.MicaBackgroundSource,
            //    !m_UISettingsControlViewModel.EnableMicaBackground);
            //SettingService.SetValue(SettingConstants.UI.MICA_BACKGROUND_SOURCE, (int)m_UISettingsControlViewModel.MicaBackgroundSource);
            //SettingService.SetValue(SettingConstants.UI.ENABLE_MICA_BACKGROUND_SOURCE, m_UISettingsControlViewModel.EnableMicaBackground);
        }
    }
}
