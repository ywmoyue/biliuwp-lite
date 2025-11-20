using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Databases;
using BiliLite.Pages;
using Microsoft.EntityFrameworkCore;
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

        public async Task HandleStartApp()
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

                var pages = await m_biliLiteDbContext.PageSavedItems.ToListAsync();

                var limitCount = SettingService.GetValue(SettingConstants.UI.OPEN_LAST_PAGE_LIMIT_COUNT,
                    SettingConstants.UI.DEFAULT_OPEN_LAST_PAGE_LIMIT_COUNT);

                if (limitCount > 0)
                {
                    pages = pages.TakeLast(limitCount).ToList();
                }

                if (!pages.Any()) return;

                m_mainPage.MainPageLoaded += async (_, _) =>
                {
                    // 等待部分资源加载，否则会白屏
                    await Task.Delay(500);
                    await using var transaction = await m_biliLiteDbContext.Database.BeginTransactionAsync();
                    try
                    {
                        foreach (var page in pages)
                        {
                            MessageCenter.NavigateToPage(null, new NavigationInfo()
                            {
                                icon = JsonConvert.DeserializeObject<Symbol>(page.Icon),
                                page = JsonConvert.DeserializeObject<Type>(page.Type),
                                parameters = JsonConvert.DeserializeObject<object>(page.Parameters),
                                title = page.Title,
                            });
                        }

                        m_biliLiteDbContext.PageSavedItems.RemoveRange(m_biliLiteDbContext.PageSavedItems);

                        await m_biliLiteDbContext.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.Error("打开上次的标签页失败", ex);
            }
        }

        public async Task<string> AddPage(string title, Type type, object parameters, Symbol icon)
        {
            try
            {
                // 检查是否已有事务，如果已有事务则等待事务结束
                {
                    var retryTimes = 0;
                    while (m_biliLiteDbContext.Database.CurrentTransaction != null && retryTimes < 10)
                    {
                        await Task.Delay(100); // 等待100ms再检查
                        retryTimes++;
                    }
                }

                await using var transaction = await m_biliLiteDbContext.Database.BeginTransactionAsync();
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
                    await m_biliLiteDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return page.Id;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }

            return "";
        }

        public async Task UpdatePage(string id, string title, Type type, object parameters, Symbol icon)
        {
            try
            {
                await using var transaction = await m_biliLiteDbContext.Database.BeginTransactionAsync();
                try
                {
                    var page = await m_biliLiteDbContext.PageSavedItems.FindAsync(id);
                    if (page == null) return;
                    page.Title = title;
                    page.Type = JsonConvert.SerializeObject(type);
                    page.Parameters = JsonConvert.SerializeObject(parameters);
                    page.Icon = JsonConvert.SerializeObject(icon);
                    await m_biliLiteDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public async Task RemovePage(string id)
        {
            try
            {
                await using var transaction = await m_biliLiteDbContext.Database.BeginTransactionAsync();
                try
                {
                    var page = await m_biliLiteDbContext.PageSavedItems.FindAsync(id);
                    if (page == null) return;
                    m_biliLiteDbContext.PageSavedItems.Remove(page);
                    await m_biliLiteDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
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
