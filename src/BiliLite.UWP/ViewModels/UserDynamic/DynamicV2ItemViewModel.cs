using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Dynamic;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.UserDynamic
{
    public class DynamicV2ItemViewModel:BaseViewModel
    {
        private ModuleDynamic m_dynamic;
        private readonly IMapper m_mapper;

        public DynamicV2ItemViewModel()
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
        }

        public string CardType { get; set; }

        public string ItemType { get; set; }

        public ModuleAuthor Author { get; set; }

        public ModuleAuthorForward AuthorForward { get; set; }

        public ModuleDynamic Dynamic
        {
            get
            {
                return m_dynamic;
            }
            set
            {
                m_dynamic = value;
                if (value.DynForward != null)
                {
                    Item = m_mapper.Map<DynamicV2ItemViewModel>(value.DynForward.Item);
                }
            }
        }

        public ModuleDesc Desc { get; set; }

        public DynamicV2ItemViewModel Item { get; set; }

        public List<DynamicV2ItemViewModel> Items =>
            new List<DynamicV2ItemViewModel>()
            {
                Item
            };

        public ModuleStat Stat { get; set; }

        public Extend Extend { get; set; }

        [DependsOn(nameof(Content))]
        public bool ShowContent => Desc != null;

        [DependsOn(nameof(Desc))]
        public RichTextBlock Content
        {
            get
            {
                try
                {
                    if (Desc != null)
                    {
                        return
                            Desc.Text.UserDynamicStringToRichText(
                                Extend.DynIdStr,
                                new List<DynamicCardDisplayEmojiInfoItemModel>(),
                                JObject.FromObject(new { }));

                    }

                    return new RichTextBlock();
                }
                catch (Exception ex)
                {
                    return new RichTextBlock();
                }
            }
        }

        public int CoverWidth => 160;

        [DependsOn(nameof(AuthorForward),nameof(CardType),nameof(ItemType))]
        public bool IsReport => AuthorForward != null;

        [DoNotNotify]
        public string Verify { get; set; } = Constants.App.TRANSPARENT_IMAGE;

        [DoNotNotify]
        public string Pendant { get; set; } = Constants.App.TRANSPARENT_IMAGE;
    }
}