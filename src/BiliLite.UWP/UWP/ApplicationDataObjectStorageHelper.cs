using BiliLite.Services;
using Newtonsoft.Json;
using System;

namespace BiliLite.UWP
{
    public class ApplicationDataObjectStorageHelper : IObjectStorageHelper
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public void Clear()
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();
        }

        public bool KeyExists(string key)
        {
            return Windows.Storage.ApplicationData.Current.LocalSettings.Values.ContainsKey(key);
        }

        public T Read<T>(string key)
        {
            try
            {
                var value = Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] as string;
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception ex)
            {
                _logger.Error(key + ":" + ex.Message, ex);
                throw;
            }
        }

        public void Save<T>(string key, T value)
        {
            try
            {
                if (value == null)
                {
                    Remove(key);
                }
                var jsonString = JsonConvert.SerializeObject(value);
                Windows.Storage.ApplicationData.Current.LocalSettings.Values[key] = jsonString;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save value for key '{key}': {ex.Message}", ex);
            }
        }

        public void Remove(string key)
        {
            Windows.Storage.ApplicationData.Current.LocalSettings.Values.Remove(key);
        }
    }
}
