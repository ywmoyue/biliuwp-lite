using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Video;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Video;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class VideoListView : UserControl
    {
        private readonly VideoListViewModel m_viewModel;
        private readonly IMapper m_mapper;
        private readonly MediaListService m_mediaListService;
        private object m_flyoutContextElement;

        public VideoListView(VideoListViewModel viewModel, IMapper mapper, MediaListService mediaListService)
        {
            m_viewModel = viewModel;
            m_mapper = mapper;
            m_mediaListService = mediaListService;
            InitializeComponent();
        }

        public event EventHandler<VideoListItem> OnSelectionChanged;

        public void LoadData(List<VideoListSection> sections)
        {
            if (m_viewModel.Sections == null)
            {
                m_viewModel.Sections = m_mapper.Map<ObservableCollection<VideoListSectionViewModel>>(sections);
                m_viewModel.LastSelectedItem = CurrentItem();
            }
            else
            {
                var existSectionIdList = m_viewModel.Sections.Select(x => x.Id).ToList();
                var mapSections =
                    m_mapper.Map<List<VideoListSectionViewModel>>(sections.Where(x =>
                        x.Id == 0 || !existSectionIdList.Contains(x.Id)));
                m_viewModel.Sections.AddRange(mapSections);
            }
        }

        public void Next(string id)
        {
            var next = false;
            foreach (var section in m_viewModel.Sections)
            {
                foreach (var item in section.Items)
                {
                    if (next)
                    {
                        section.SelectedItem = item;
                        return;
                    }
                    if (item.Id == id)
                    {
                        next = true;
                    }
                }
            }
            NotificationShowExtensions.ShowMessageToast("播放完毕");
        }

        public bool IsLast(string id)
        {
            return m_viewModel.Sections.LastOrDefault()?.Items.LastOrDefault()?.Id == id;
        }

        public VideoListItem CurrentItem()
        {
            return (m_viewModel.Sections.Where(section => section.SelectedItem != null)
                .Select(section => section.SelectedItem)).FirstOrDefault();
        }

        private void ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListView { SelectedItem: VideoListItem item })) return;
            if (item == m_viewModel.LastSelectedItem) return;
            foreach (var section in m_viewModel.Sections.Where(
                         x => x.SelectedItem != item && x.SelectedItem != null))
            {
                section.SelectedItem = null;
            }
            ScrollToItem(item);

            m_viewModel.LastSelectedItem = item;

            OnSelectionChanged?.Invoke(this, item);
        }

        private void ScrollToItem(VideoListItem item)
        {
            var offset = GetItemOffsetHeight(item);
            VideoListScrollViewer.ChangeView(null, offset, null);
        }

        private double GetItemOffsetHeight(VideoListItem item)
        {
            var offset = 0d;
            var expanders = this.FindChildrenByType<Microsoft.UI.Xaml.Controls.Expander>().ToList();
            if (!expanders.Any()) return 0;
            var expanderHeaderHeight = (expanders.First().Header as FrameworkElement).ActualHeight;
            var videoListItemGridHeight = 87 + 12;
            foreach (var section in m_viewModel.Sections)
            {
                if (section.SelectedItem != item && !section.Selected)
                {
                    offset += expanderHeaderHeight;
                }
                else if (section.SelectedItem != item && section.Selected)
                {
                    offset += expanderHeaderHeight;

                    offset += (section.Items.Count * videoListItemGridHeight);
                }
                else if (section.SelectedItem == item)
                {
                    section.Selected = true;
                    var expander = expanders.FirstOrDefault(x => x.DataContext == section);
                    if (expander == null) return 0;
                    expander.IsExpanded = true;

                    foreach (var videoItem in section.Items)
                    {
                        if (videoItem == item)
                        {
                            return offset;
                        }

                        offset += videoListItemGridHeight;
                    }
                }
            }

            return offset;
        }

        private void UIElement_OnContextRequested(UIElement sender, ContextRequestedEventArgs args)
        {
            m_flyoutContextElement = sender;
            sender.ContextFlyout.ShowAt(sender, new FlyoutShowOptions());
        }

        private void CloseList_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Control { DataContext: VideoListSectionViewModel section })
            {
                m_viewModel.Sections.Remove(section);
            }
        }

        private async void BtnLoadMore_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is HyperlinkButton btnLoadMore))
            {
                return;
            }

            if (!(btnLoadMore.DataContext is VideoListSectionViewModel section)) return;
            await m_mediaListService.LoadMoreMediaList(section);
        }

        private void SectionListView_Loaded(object sender, RoutedEventArgs e) => ScrollToItem(CurrentItem());

        private void OnScrollToCurrentBtnTapped(object sender, TappedRoutedEventArgs e) => ScrollToItem(CurrentItem());

        private void OnUpToTopBtnTapped(object sender, TappedRoutedEventArgs e) => VideoListScrollViewer.ChangeView(null, 0, null);

        private void OnDownToBottomBtnTapped(object sender, TappedRoutedEventArgs e) => VideoListScrollViewer.ChangeView(null, SectionListView.ActualHeight, null);
    }
}
