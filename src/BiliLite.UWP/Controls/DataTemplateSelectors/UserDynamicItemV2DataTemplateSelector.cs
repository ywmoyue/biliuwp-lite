using BiliLite.Models.Common;
using BiliLite.ViewModels.UserDynamic;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Controls.DataTemplateSelectors
{
    public class UserDynamicItemV2DataTemplateSelector : DataTemplateSelector
    {
        private static readonly Dictionary<string, Func<UserDynamicItemV2DataTemplateSelector, DynamicV2ItemViewModel, DataTemplate>> _dynamicTypeTemplateSelectFuncs;

        static UserDynamicItemV2DataTemplateSelector()
        {
            DataTemplate SelectRowTemplate(UserDynamicItemDataTemplateSelector selector,
                UserDynamicItemDisplayViewModel _) => selector.OneRowTemplate;

            _dynamicTypeTemplateSelectFuncs =
                new Dictionary<string, Func<UserDynamicItemV2DataTemplateSelector,
                    DynamicV2ItemViewModel, DataTemplate>>()
                {
                    { 
                        Constants.DynamicTypes.DRAW, (selector, model) =>
                        {
                            if (model.Dynamic.DynDraw.Items.Count == 1)
                            {
                                return selector.Draw1x1Template;
                            }
                            return model.Dynamic.DynDraw.Items.Count == 4 ? selector.Draw2x2Template : selector.Draw3x3Template;
                        }
                    },
                    { Constants.DynamicTypes.ARTICLE, (selector, model) => selector.ArticleTemplate },
                    { Constants.DynamicTypes.FORWARD, (selector, model) => selector.ForwardTemplate },
                    { Constants.DynamicTypes.AV, (selector, model) => selector.AvTemplate },
                    { Constants.DynamicTypes.PGC, (selector, model) => selector.PgcTemplate },
                    { Constants.DynamicTypes.WORD, (selector, model) => selector.WordTemplate },
                    { Constants.DynamicTypes.MUSIC, (selector, model) => selector.MusicTemplate },
                    { Constants.DynamicTypes.COMMON_SQUARE, (selector, model) => selector.CommonSquareTemplate },
                    { Constants.DynamicTypes.LIVE_RCMD, (selector, model) => selector.LiveRcmdTemplate },
                    { Constants.DynamicTypes.CUSTOM_SEASON, (selector, model) => selector.CustomSeasonTemplate },
                    { Constants.DynamicTypes.CUSTOM_ARTICLE, (selector, model) => selector.CustomArticleTemplate },
                };
        }

        public DataTemplate AvTemplate { get; set; }

        public DataTemplate PgcTemplate { get; set; }

        public DataTemplate ArticleTemplate { get; set; }

        public DataTemplate WordTemplate { get; set; }

        public DataTemplate MusicTemplate { get; set; }

        public DataTemplate ForwardTemplate { get; set; }

        public DataTemplate Draw1x1Template { get; set; }

        public DataTemplate Draw2x2Template { get; set; }

        public DataTemplate Draw3x3Template { get; set; }

        public DataTemplate CommonSquareTemplate { get; set; }

        public DataTemplate LiveRcmdTemplate { get; set; }

        public DataTemplate CustomSeasonTemplate { get; set; }

        public DataTemplate CustomArticleTemplate { get; set; }

        public DataTemplate OtherTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var model = item as DynamicV2ItemViewModel;
            var success = _dynamicTypeTemplateSelectFuncs.TryGetValue(model.CardType, out var selectFunc);
            return success ? selectFunc(this, model) : OtherTemplate;
        }
    }
}
