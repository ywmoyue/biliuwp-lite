using System;
using System.Threading;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Databases;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BiliLite.Services
{
    /// <summary>
    /// 通用缓存：客户端不做纯内存缓存，统一落 SQLite（按 Key + 过期时间读写）
    /// </summary>
    [RegisterSingletonService]
    public class CacheService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private readonly BiliLiteDbContext m_biliLiteDbContext;
        private readonly SemaphoreSlim m_semaphore = new SemaphoreSlim(1, 1);

        public CacheService(BiliLiteDbContext biliLiteDbContext)
        {
            m_biliLiteDbContext = biliLiteDbContext;
        }

        /// <summary>
        /// 读取缓存，不存在或已过期返回 null
        /// </summary>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            return await ExecuteAsync(async () =>
            {
                var item = await m_biliLiteDbContext.CacheItems.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Key == key);
                if (item == null)
                {
                    return null;
                }

                if (item.ExpireAt <= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
                {
                    return null;
                }

                return JsonConvert.DeserializeObject<T>(item.Value);
            });
        }

        /// <summary>
        /// 写入缓存，已存在则覆盖
        /// </summary>
        public async Task SetAsync<T>(string key, T value, TimeSpan expireTime) where T : class
        {
            if (value == null)
            {
                return;
            }

            var valueJson = JsonConvert.SerializeObject(value);
            var expireAt = DateTimeOffset.UtcNow.Add(expireTime).ToUnixTimeMilliseconds();

            await ExecuteAsync(async () =>
            {
                var item = await m_biliLiteDbContext.CacheItems.FirstOrDefaultAsync(x => x.Key == key);
                if (item == null)
                {
                    m_biliLiteDbContext.CacheItems.Add(new CacheItem()
                    {
                        Key = key,
                        Value = valueJson,
                        ExpireAt = expireAt,
                    });
                }
                else
                {
                    item.Value = valueJson;
                    item.ExpireAt = expireAt;
                }

                await m_biliLiteDbContext.SaveChangesAsync();
            });
        }

        /// <summary>
        /// 删除缓存
        /// </summary>
        public async Task RemoveAsync(string key)
        {
            await ExecuteAsync(async () =>
            {
                var item = await m_biliLiteDbContext.CacheItems.FirstOrDefaultAsync(x => x.Key == key);
                if (item == null)
                {
                    return;
                }

                m_biliLiteDbContext.CacheItems.Remove(item);
                await m_biliLiteDbContext.SaveChangesAsync();
            });
        }

        private async Task<T> ExecuteAsync<T>(Func<Task<T>> action) where T : class
        {
            await m_semaphore.WaitAsync();
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                _logger.Warn("读取缓存失败", ex);
                return null;
            }
            finally
            {
                m_semaphore.Release();
            }
        }

        private async Task ExecuteAsync(Func<Task> action)
        {
            await m_semaphore.WaitAsync();
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                _logger.Warn("写入缓存失败", ex);
            }
            finally
            {
                m_semaphore.Release();
            }
        }
    }
}
