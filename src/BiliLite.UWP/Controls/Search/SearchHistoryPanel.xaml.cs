using BiliLite.Services.Biz;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BiliLite.Controls.Search
{
    /// <summary>
    /// 搜索历史面板，只负责展示与增删历史，选中关键字时通过 SearchRequested 通知外部
    /// </summary>
    public sealed partial class SearchHistoryPanel : UserControl
    {
        private readonly SearchService m_searchService;
        private readonly ObservableCollection<string> m_history = new ObservableCollection<string>();

        /// <summary>
        /// 用户选中了某条历史关键字
        /// </summary>
        public event EventHandler<string> SearchRequested;

        /// <summary>
        /// 历史已被清空
        /// </summary>
        public event EventHandler HistoryEmptied;

        public SearchHistoryPanel(SearchService searchService)
        {
            m_searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            InitializeComponent();
        }

        /// <summary>
        /// 从存储重新加载历史，返回是否存在历史记录
        /// </summary>
        public bool RefreshHistory()
        {
            LoadHistory(m_searchService.GetSearchHistory());
            return m_history.Count > 0;
        }

        private void LoadHistory(IEnumerable<string> history)
        {
            m_history.Clear();
            if (history == null) return;
            foreach (var keyword in history)
            {
                m_history.Add(keyword);
            }
        }

        private void OnHistoryChanged(List<string> history)
        {
            LoadHistory(history);
            if (m_history.Count == 0)
            {
                HistoryEmptied?.Invoke(this, EventArgs.Empty);
            }
        }

        private void BtnHistoryItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button { Tag: string keyword })) return;
            SearchRequested?.Invoke(this, keyword);
        }

        private void BtnRemoveItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button { Tag: string keyword })) return;

            OnHistoryChanged(m_searchService.RemoveSearchHistory(keyword));
        }

        private void BtnClear_OnClick(object sender, RoutedEventArgs e)
        {
            m_searchService.ClearSearchHistory();
            OnHistoryChanged(m_searchService.GetSearchHistory());
        }
    }
}
