using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Common.Settings;
using BiliLite.Modules;
using BiliLite.ViewModels.UserDynamic;

namespace BiliLite.Services
{
    public class ContentFilterService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public List<FilterRule> RecommendFilterRules { get; set; }

        public List<FilterRule> SearchFilterRules { get; set; }

        public List<FilterRule> DynamicFilterRules { get; set; }

        public ContentFilterService()
        {
            LoadFilterRules();
        }

        private void LoadFilterRules()
        {
            RecommendFilterRules =
                SettingService.GetValue(SettingConstants.Filter.RECOMMEND_FILTER_RULE, new List<FilterRule>());
            SearchFilterRules =
                SettingService.GetValue(SettingConstants.Filter.SEARCH_FILTER_RULE, new List<FilterRule>());
            DynamicFilterRules =
                SettingService.GetValue(SettingConstants.Filter.DYNAMIC_FILTER_RULE, new List<FilterRule>());
        }

        private void UpdateFilterRule(FilterRule target, FilterRule source)
        {
            target.ContentType = source.ContentType;
            target.FilterType = source.FilterType;
            target.Rule = source.Rule;
            target.Enable = source.Enable;
        }

        public void AddRecommendFilterRule(FilterRule filterRule)
        {
            RecommendFilterRules.Add(filterRule);
            SettingService.SetValue(SettingConstants.Filter.RECOMMEND_FILTER_RULE, RecommendFilterRules);
        }

        public void AddSearchFilterRule(FilterRule filterRule)
        {
            SearchFilterRules.Add(filterRule);
            SettingService.SetValue(SettingConstants.Filter.SEARCH_FILTER_RULE, SearchFilterRules);
        }

        public void AddDynamicFilterRule(FilterRule filterRule)
        {
            DynamicFilterRules.Add(filterRule);
            SettingService.SetValue(SettingConstants.Filter.DYNAMIC_FILTER_RULE, DynamicFilterRules);
        }

        public void DeleteFilterRule(FilterRule filterRule)
        {
            var recommendFilterRule = RecommendFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (recommendFilterRule != null)
            {
                RecommendFilterRules.Remove(recommendFilterRule);
                SettingService.SetValue(SettingConstants.Filter.RECOMMEND_FILTER_RULE, RecommendFilterRules);
                return;
            }
            var searchFilterRule = SearchFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (searchFilterRule != null)
            {
                SearchFilterRules.Remove(searchFilterRule);
                SettingService.SetValue(SettingConstants.Filter.SEARCH_FILTER_RULE, SearchFilterRules);
                return;
            }
            var dynamicFilterRule = DynamicFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (dynamicFilterRule != null)
            {
                DynamicFilterRules.Remove(dynamicFilterRule);
                SettingService.SetValue(SettingConstants.Filter.DYNAMIC_FILTER_RULE, DynamicFilterRules);
                return;
            }
        }

        public void UpdateFilterRule(FilterRule filterRule)
        {
            var recommendFilterRule = RecommendFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (recommendFilterRule != null)
            {
                UpdateFilterRule(recommendFilterRule,filterRule);
                SettingService.SetValue(SettingConstants.Filter.RECOMMEND_FILTER_RULE, RecommendFilterRules);
                return;
            }
            var searchFilterRule = SearchFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (searchFilterRule != null)
            {
                UpdateFilterRule(searchFilterRule, filterRule);
                SettingService.SetValue(SettingConstants.Filter.SEARCH_FILTER_RULE, SearchFilterRules);
                return;
            }
            var dynamicFilterRule = DynamicFilterRules.FirstOrDefault(x => x.Id == filterRule.Id);
            if (dynamicFilterRule != null)
            {
                UpdateFilterRule(dynamicFilterRule, filterRule);
                SettingService.SetValue(SettingConstants.Filter.DYNAMIC_FILTER_RULE, DynamicFilterRules);
                return;
            }
        }

        public List<RecommendItemModel> FilterRecommendItems(List<RecommendItemModel> recommendItems)
        {
            if (RecommendFilterRules.Count == 0)
            {
                return recommendItems;
            }
            var query = recommendItems.AsEnumerable();
            query = RecommendFilterRules.Where(rule => rule.Enable)
                .Aggregate(query, (current, rule) => rule.ContentType switch
                {
                    FilterContentType.Title => rule.FilterType switch
                    {
                        FilterType.Word => current.Where(x => !x.Title.Contains(rule.Rule)),
                        FilterType.Regular => current.Where(x => !new Regex(rule.Rule).IsMatch(x.Title)),
                        _ => current
                    },
                    FilterContentType.User => rule.FilterType switch
                    {
                        FilterType.Word => current.Where(x => !x.Args.UpName.Contains(rule.Rule)),
                        FilterType.Regular => current.Where(x => !new Regex(rule.Rule).IsMatch(x.Args.UpName)),
                        _ => current
                    },
                    _ => current
                });

            var result = query.ToList();

            _logger.Debug($"source recommendItems count:{recommendItems.Count};after filter:{result.Count}");
            //if (recommendItems.Count - result.Count > 0)
            //{
            //    Notify.ShowMessageToast($"过滤:{recommendItems.Count - result.Count}");
            //}

            return result;
        }

        public List<SearchVideoItem> FilterSearchItems(List<SearchVideoItem> searchItems)
        {
            if (SearchFilterRules.Count == 0)
            {
                return searchItems;
            }
            var query = searchItems.AsEnumerable();
            query = SearchFilterRules.Where(rule => rule.Enable)
                .Aggregate(query, (current, rule) => rule.ContentType switch
                {
                    FilterContentType.Title => rule.FilterType switch
                    {
                        FilterType.Word => current.Where(x => !x.title.Contains(rule.Rule)),
                        FilterType.Regular => current.Where(x => !new Regex(rule.Rule).IsMatch(x.title)),
                        _ => current
                    },
                    FilterContentType.User => rule.FilterType switch
                    {
                        FilterType.Word => current.Where(x => !x.author.Contains(rule.Rule)),
                        FilterType.Regular => current.Where(x => !new Regex(rule.Rule).IsMatch(x.author)),
                        _ => current
                    },
                    _ => current
                });

            var result = query.ToList();

            _logger.Debug($"source searchItems count:{searchItems.Count};after filter:{result.Count}");
            //if (searchItems.Count - result.Count > 0)
            //{
            //    Notify.ShowMessageToast($"过滤:{searchItems.Count - result.Count}");
            //}

            return result;
        }

        public List<DynamicV2ItemViewModel> FilterDynamicItems(List<DynamicV2ItemViewModel> dynItems)
        {
            if (DynamicFilterRules.Count == 0)
            {
                return dynItems;
            }
            var query = dynItems.AsEnumerable();
            query = DynamicFilterRules.Where(rule => rule.Enable)
                .Aggregate(query, (current, rule) => rule.ContentType switch
                {
                    FilterContentType.User => rule.FilterType switch
                    {
                        FilterType.Word => current.Where(x => !(x.Author != null && x.Author.Author.Name.Contains(rule.Rule))),
                        FilterType.Regular => current.Where(x => !(x.Author != null && new Regex(rule.Rule).IsMatch(x.Author.Author.Name))),
                        _ => current
                    },
                    _ => current
                });

            var result = query.ToList();

            _logger.Debug($"source dynItems count:{dynItems.Count};after filter:{result.Count}");
            //if (dynItems.Count - result.Count > 0)
            //{
            //    Notify.ShowMessageToast($"过滤:{dynItems.Count - result.Count}");
            //}

            return result;
        }
    } 
}
