using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Databases;
using BiliLite.Pages;
using Newtonsoft.Json;

namespace BiliLite.Services
{
    [RegisterSingletonService]
    public class PageSaveService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BiliLiteDbContext m_biliLiteDbContext;
        private readonly IMainPage m_mainPage;

        public PageSaveService(BiliLiteDbContext biliLiteDbContext, IMainPage mainPage)
        {
            m_biliLiteDbContext = biliLiteDbContext;
            m_mainPage = mainPage;
        }

        public void HandleStartApp()
        {
            try
            {
                var mode = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
                if (mode != 0)
                {
                    return;
                }

                if (!SettingService.GetValue(SettingConstants.UI.ENABLE_OPEN_LAST_PAGE,
                        SettingConstants.UI.DEFAULT_ENABLE_OPEN_LAST_PAGE))
                {
                    return;
                }
                var pages = m_biliLiteDbContext.PageSavedItems.ToList();
                if (!pages.Any()) return;
                m_mainPage.MainPageLoaded += async (_, _) =>
                {
                    // 等待部分资源加载，否则会白屏
                    await Task.Delay(500);
                    foreach (var page in pages)
                    {
                        m_biliLiteDbContext.PageSavedItems.Remove(page);
                        MessageCenter.NavigateToPage(null, new NavigationInfo()
                        {
                            icon = JsonConvert.DeserializeObject<Symbol>(page.Icon),
                            page = JsonConvert.DeserializeObject<Type>(page.Type),
                            parameters = JsonConvert.DeserializeObject<object>(page.Parameters),
                            title = page.Title,
                        });
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error("打开上次的标签页失败", ex);
            }
        }

        public string AddPage(string title, Type type, object parameters, Symbol icon)
        {
            try
            {
                var page = new PageSavedDTO()
                {
                    Parameters = JsonConvert.SerializeObject(parameters),
                    Type = JsonConvert.SerializeObject(type),
                    Title = title,
                    Icon = JsonConvert.SerializeObject(icon),
                };
                m_biliLiteDbContext.PageSavedItems.Add(page);
                m_biliLiteDbContext.SaveChanges();
                return page.Id;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return "";
        }

        public void UpdatePage(string id, string title, Type type, object parameters, Symbol icon)
        {
            try
            {
                var page = m_biliLiteDbContext.PageSavedItems.Find(id);
                if (page == null) return;
                page.Title = title;
                page.Type = JsonConvert.SerializeObject(type);
                page.Parameters = JsonConvert.SerializeObject(parameters);
                page.Icon = JsonConvert.SerializeObject(icon);
                m_biliLiteDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public void RemovePage(string id)
        {
            try
            {
                var page = m_biliLiteDbContext.PageSavedItems.Find(id);
                if (page == null) return;
                m_biliLiteDbContext.PageSavedItems.Remove(page);
                m_biliLiteDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public void SaveMainPageTabIndex(int index)
        {

        }
    }
}