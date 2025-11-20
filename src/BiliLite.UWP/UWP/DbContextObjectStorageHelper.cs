using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.UWP
{
    class DbContextObjectStorageHelper : IObjectStorageHelper
    {
        private SettingSqlService m_settingSqlService;

        public DbContextObjectStorageHelper()
        {
            m_settingSqlService = App.ServiceProvider.GetRequiredService<SettingSqlService>();
        }

        public void Clear()
        {
            
        }

        public bool KeyExists(string key)
        {
            return m_settingSqlService.HasValue(key);
        }

        public T Read<T>(string key)
        {
            return m_settingSqlService.GetValue<T>(key, default);
        }

        public T Read<T>(string key, T defaultValue)
        {
            return m_settingSqlService.GetValue<T>(key, defaultValue);
        }

        public void Remove(string key)
        {
            
        }

        public void Save<T>(string key, T value)
        {
            m_settingSqlService.SetValue<T>(key, value);
        }
    }
}
