using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Responses;

namespace BiliLite.Services.Biz
{
    public class SearchService
    {
        /// <summary>
        /// 搜索历史最大保存条数
        /// </summary>
        private const int MAX_SEARCH_HISTORY_COUNT = 20;

        private readonly SettingSqlService m_settingSqlService;

        public SearchService(SettingSqlService settingSqlService)
        {
            m_settingSqlService = settingSqlService;
        }

        public int PivotIndexCache { get; set; }

        public int ComboIndexCache { get; set; }

        public async Task<List<string>> GetSearchSuggestContents(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            var suggestSearchResult = await new SearchAPI().SearchSuggest(text).Request();
            if (!suggestSearchResult.status) return null;
            var suggestSearch = await suggestSearchResult.GetResult<SearchSuggestResponse>();
            if (suggestSearch == null) return null;
            if (suggestSearch.result == null) return null;
            var suggestSearchContent = suggestSearch.result.Tag.Select(x => x.Value);
            return suggestSearchContent.ToList();
        }

        /// <summary>
        /// 获取搜索历史（最新在最前）
        /// </summary>
        public List<string> GetSearchHistory()
        {
            var history = m_settingSqlService.GetValue<List<string>>(SettingConstants.Search.SEARCH_HISTORY, null);
            return history ?? new List<string>();
        }

        /// <summary>
        /// 添加搜索历史，已存在时移动到最前，超出条数时丢弃最旧记录
        /// </summary>
        public List<string> AddSearchHistory(string keyword)
        {
            keyword = keyword?.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                return GetSearchHistory();
            }

            var history = GetSearchHistory();
            history.RemoveAll(x => string.Equals(x, keyword, StringComparison.OrdinalIgnoreCase));
            history.Insert(0, keyword);
            if (history.Count > MAX_SEARCH_HISTORY_COUNT)
            {
                history = history.Take(MAX_SEARCH_HISTORY_COUNT).ToList();
            }

            m_settingSqlService.SetValue(SettingConstants.Search.SEARCH_HISTORY, history);
            return history;
        }

        /// <summary>
        /// 删除单条搜索历史
        /// </summary>
        public List<string> RemoveSearchHistory(string keyword)
        {
            var history = GetSearchHistory();
            history.RemoveAll(x => string.Equals(x, keyword, StringComparison.Ordinal));
            m_settingSqlService.SetValue(SettingConstants.Search.SEARCH_HISTORY, history);
            return history;
        }

        /// <summary>
        /// 清空搜索历史
        /// </summary>
        public void ClearSearchHistory()
        {
            m_settingSqlService.SetValue(SettingConstants.Search.SEARCH_HISTORY, new List<string>());
        }
    }
}
