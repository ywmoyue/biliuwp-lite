using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using AutoMapper;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Settings;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class FilterSettingsControl : UserControl
    {
        private readonly ContentFilterService m_contentFilterService;
        private readonly FilterSettingsViewModel m_viewModel;
        private readonly IMapper m_mapper;

        public FilterSettingsControl()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            m_viewModel = App.ServiceProvider.GetRequiredService<FilterSettingsViewModel>();
            DataContext = m_viewModel;
            m_contentFilterService = App.ServiceProvider.GetRequiredService<ContentFilterService>();
            m_viewModel.RecommendFilterRules =
                new ObservableCollection<FilterRuleViewModel>(
                    m_mapper.Map<List<FilterRuleViewModel>>(m_contentFilterService.RecommendFilterRules)
                    );
            m_viewModel.SearchFilterRules =
                new ObservableCollection<FilterRuleViewModel>(
                    m_mapper.Map<List<FilterRuleViewModel>>(m_contentFilterService.SearchFilterRules)
                );
            m_viewModel.DynamicFilterRules =
                new ObservableCollection<FilterRuleViewModel>(
                    m_mapper.Map<List<FilterRuleViewModel>>(m_contentFilterService.DynamicFilterRules)
                );
            this.InitializeComponent();
        }

        private FilterRule CreateNewFilterRule(FilterRuleType filterRuleType)
        {
            var rule = new FilterRule()
            {
                Id = Guid.NewGuid().ToString(),
                Rule = "新规则",
                FilterRuleType = filterRuleType,
                ContentType = FilterContentType.Title,
                FilterType = FilterType.Word,
                Enable = true,
            };
            return rule;
        }

        private void BtnAddRecommendFilterRule_OnClick(object sender, RoutedEventArgs e)
        {
            var rule = CreateNewFilterRule(FilterRuleType.Recommend);
            m_contentFilterService.AddRecommendFilterRule(rule);
            var ruleViewModel = m_mapper.Map<FilterRuleViewModel>(rule);
            m_viewModel.RecommendFilterRules.Add(ruleViewModel);
        }

        private void UpdateFilterRule(FilterRuleViewModel filterRuleViewModel)
        {
            var rule = m_mapper.Map<FilterRule>(filterRuleViewModel);
            m_contentFilterService.UpdateFilterRule(rule);
        }

        private async void OnFilterRuleChanged<T>(object sender, T e)
        {
            if (sender is FrameworkElement { DataContext: FilterRuleViewModel filterRuleViewModel })
            {
                // 等待数据源更新
                await Task.Delay(50);
                UpdateFilterRule(filterRuleViewModel);
            }
        }

        private void BtnDeleteFilterRule_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement { DataContext: FilterRuleViewModel filterRuleViewModel })
            {
                var rule = m_mapper.Map<FilterRule>(filterRuleViewModel);
                m_contentFilterService.DeleteFilterRule(rule);

                m_viewModel.RecommendFilterRules.Remove(filterRuleViewModel);
                m_viewModel.SearchFilterRules.Remove(filterRuleViewModel);
                m_viewModel.DynamicFilterRules.Remove(filterRuleViewModel);
            }
        }

        private void BtnAddSearchFilterRule_OnClick(object sender, RoutedEventArgs e)
        {
            var rule = CreateNewFilterRule(FilterRuleType.Search);
            m_contentFilterService.AddSearchFilterRule(rule);
            var ruleViewModel = m_mapper.Map<FilterRuleViewModel>(rule);
            m_viewModel.SearchFilterRules.Add(ruleViewModel);
        }

        private void BtnAddDynamicFilterRule_OnClick(object sender, RoutedEventArgs e)
        {
            var rule = CreateNewFilterRule(FilterRuleType.Dynamic);
            m_contentFilterService.AddDynamicFilterRule(rule);
            var ruleViewModel = m_mapper.Map<FilterRuleViewModel>(rule);
            m_viewModel.DynamicFilterRules.Add(ruleViewModel);
        }
    }
}
