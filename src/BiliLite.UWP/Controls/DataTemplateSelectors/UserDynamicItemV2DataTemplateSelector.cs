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
                };
        }

        public DataTemplate ArticleTemplate { get; set; }

        public DataTemplate ForwardTemplate { get; set; }

        public DataTemplate Draw1x1Template { get; set; }

        public DataTemplate Draw2x2Template { get; set; }

        public DataTemplate Draw3x3Template { get; set; }

        public DataTemplate OtherTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var model = item as DynamicV2ItemViewModel;
            var success = _dynamicTypeTemplateSelectFuncs.TryGetValue(model.CardType, out var selectFunc);
            return success ? selectFunc(this, model) : OtherTemplate;
        }
    }
}
