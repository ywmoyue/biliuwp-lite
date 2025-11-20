using BiliLite.Models.Common;
using BiliLite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomlyn;
using Tomlyn.Model;
using Windows.Storage;

namespace BiliLite.UWP
{
    internal class TomlObjectStorageHelper : IObjectStorageHelper
    {
        private TomlTable _data;
        private readonly object _lock = new object();
        private static readonly string _filePath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.toml");
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public TomlObjectStorageHelper()
        {
            LoadData();
        }

        private void LoadData()
        {
            lock (_lock)
            {
                try
                {
                    if (File.Exists(_filePath))
                    {
                        var tomlContent = File.ReadAllText(_filePath);
                        _data = Toml.ToModel(tomlContent);
                    }
                    else
                    {
                        _data = new TomlTable();
                    }
                }
                catch
                {
                    _data = new TomlTable();
                }
            }
        }

        private void SaveData()
        {
            lock (_lock)
            {
                try
                {
                    var tomlContent = Toml.FromModel(_data);
                    File.WriteAllText(_filePath, tomlContent);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save data: {ex.Message}", ex);
                }
            }
        }

        public bool KeyExists(string key)
        {
            lock (_lock)
            {
                return _data.ContainsKey(key);
            }
        }

        public T Read<T>(string key)
        {
            lock (_lock)
            {
                try
                {
                    var value = _data[key];

                    if (typeof(T) == typeof(string))
                    {
                        return (T)(object)value.ToString();
                    }
                    else if (typeof(T).IsValueType || typeof(T) == typeof(string))
                    {
                        try
                        {
                            return (T)Convert.ChangeType(value, typeof(T));
                        }
                        catch
                        {
                            return JsonConvert.DeserializeObject<T>(value.ToString());
                        }
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject<T>(value.ToString());
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(key + ":" + ex.Message, ex);
                    throw;
                }
            }
        }

        public void Save<T>(string key, T value)
        {
            lock (_lock)
            {
                try
                {
                    if (value == null)
                    {
                        _data.Remove(key);
                    }
                    else if (value is string || value.GetType().IsValueType)
                    {
                        _data[key] = value;
                    }
                    else
                    {
                        var jsonString = JsonConvert.SerializeObject(value);
                        _data[key] = jsonString;
                    }

                    SaveData();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to save value for key '{key}': {ex.Message}", ex);
                }
            }
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                if (_data.ContainsKey(key))
                {
                    _data.Remove(key);
                    SaveData();
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _data.Clear();
                SaveData();
            }
        }
    }

}
