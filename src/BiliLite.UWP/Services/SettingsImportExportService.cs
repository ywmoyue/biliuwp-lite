﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using Newtonsoft.Json;
using Tomlyn;
using Tomlyn.Model;
using BiliLite.Extensions;
using System.Security.Cryptography;
using Windows.Storage;
using ArtisanCode.SimpleAesEncryption;

namespace BiliLite.Services
{
    public class SettingsImportExportService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private const string SETTINGS_EXPORT_KEY = "4bmsD9chgoP1jdohv2+OV9Pv2403r4IfJU18ixpFWQA=";
        private const string SETTINGS_EXPORT_IV = "ABxrLAWa7MrKg6w1xxtZmw==";
        private readonly RijndaelMessageEncryptor m_encryptor;
        private readonly RijndaelMessageDecryptor m_decryptor;

        public SettingsImportExportService()
        {
            var config = new SimpleAesEncryptionConfiguration()
            {
                CipherMode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
                EncryptionKey = new EncryptionKeyConfigurationElement(256, SETTINGS_EXPORT_KEY),
            };
            m_encryptor = new RijndaelMessageEncryptor(config);
            m_decryptor = new RijndaelMessageDecryptor(config);
        }

        private void ExportSettingsCore(TomlTable model, Type settingsType)
        {
            var fields = settingsType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(false);
                var keyAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(SettingKeyAttribute));
                if (!(keyAttribute is SettingKeyAttribute settingKeyAttribute)) continue;
                var key = field.GetRawConstantValue().ToString();

                if (!SettingService.HasValue(key)) continue;

                object value = null;
                if (settingKeyAttribute.Type == typeof(string))
                {
                    value = SettingService.GetValue<string>(key, "");
                }
                else if (settingKeyAttribute.Type == typeof(int))
                {
                    value = SettingService.GetValue<int>(key, 0);
                }
                else if (settingKeyAttribute.Type == typeof(long))
                {
                    value = SettingService.GetValue<long>(key, 0);
                }
                else if (settingKeyAttribute.Type == typeof(double))
                {
                    value = SettingService.GetValue<double>(key, 0);
                }
                else if (settingKeyAttribute.Type == typeof(object))
                {
                    value = SettingService.GetValue<object>(key, null);
                    value = JsonConvert.SerializeObject(value);
                }
                else if (settingKeyAttribute.Type == typeof(bool))
                {
                    value = SettingService.GetValue<bool>(key, false);
                }

                model[key] = value;
            }
        }

        private void ImportSettingsCore(TomlTable model, Type settingsType)
        {
            var fields = settingsType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(false);
                var keyAttribute = attributes.FirstOrDefault(x => x.GetType() == typeof(SettingKeyAttribute));
                if (!(keyAttribute is SettingKeyAttribute settingKeyAttribute)) continue;
                var key = field.GetRawConstantValue().ToString();

                if (!model.ContainsKey(key)) continue;

                if (settingKeyAttribute.Type == typeof(object))
                {
                    var value = JsonConvert.DeserializeObject(model[key].ToString());
                    SettingService.SetValue(key, value);
                }
                else if (settingKeyAttribute.Type == typeof(string))
                {
                    var value = model[key].ToString();
                    SettingService.SetValue(key, value);
                }
                else if (settingKeyAttribute.Type == typeof(int))
                {
                    var value = model[key].ToInt32();
                    SettingService.SetValue(key, value);
                }
                else if (settingKeyAttribute.Type == typeof(long))
                {
                    var value = model[key].ToInt64();
                    SettingService.SetValue(key, value);
                }
                else if (settingKeyAttribute.Type == typeof(double))
                {
                    var value = (double)model[key];
                    SettingService.SetValue(key, value);
                }
                else
                {
                    SettingService.SetValue(key, model[key]);
                }
            }
        }

        public byte[] EncryptToBinary(string plainText)
        {
            var cyphertext = m_encryptor.Encrypt(plainText);
            return System.Text.Encoding.UTF8.GetBytes(cyphertext);
        }

        public string DecryptFromBinary(byte[] cipherTextBinary)
        {
            var binText = System.Text.Encoding.UTF8.GetString(cipherTextBinary);
            var plaintext = m_decryptor.Decrypt(binText);
            return plaintext;
        }

        private string Decode(byte[] bin)
        {
            var text = DecryptFromBinary(bin);
            return text;
        }

        private byte[] Encode(string text)
        {
            var bin = EncryptToBinary(text);
            return bin;
        }

        public async Task<bool> ExportSettings()
        {
            var folder = await new FolderPicker().PickSingleFolderAsync();
            if (folder == null) return false;
            var file = await folder.CreateFileAsync($"{DateTime.Now.ToString("yyyy-M-d-HH_mm_ss")}.bililiteSettings");

            try
            {
                var model = Toml.ToModel("");

                ExportSettingsCore(model, typeof(SettingConstants.UI));
                ExportSettingsCore(model, typeof(SettingConstants.VideoDanmaku));
                ExportSettingsCore(model, typeof(SettingConstants.Live));
                ExportSettingsCore(model, typeof(SettingConstants.Player));
                ExportSettingsCore(model, typeof(SettingConstants.Roaming));
                ExportSettingsCore(model, typeof(SettingConstants.Download));
                ExportSettingsCore(model, typeof(SettingConstants.Other));

                var modelString = Toml.FromModel(model);

                var bin = Encode(modelString);

                using var stream = await file.OpenStreamForWriteAsync();
                await stream.WriteAsync(bin, 0, bin.Length);
                await stream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("导出失败", ex);
                Notify.ShowMessageToast("导出失败，已记录错误");
                return false;
            }
        }

        public async Task<bool> ExportSettingsWithAccount()
        {
            var folder = await new FolderPicker().PickSingleFolderAsync();
            if (folder == null) return false;
            var file = await folder.CreateFileAsync($"{DateTime.Now.ToString("yyyy-M-d-HH_mm_ss")}.bililiteSettings");

            try
            {
                var model = Toml.ToModel("");

                ExportSettingsCore(model, typeof(SettingConstants.UI));
                ExportSettingsCore(model, typeof(SettingConstants.Account));
                ExportSettingsCore(model, typeof(SettingConstants.VideoDanmaku));
                ExportSettingsCore(model, typeof(SettingConstants.Live));
                ExportSettingsCore(model, typeof(SettingConstants.Player));
                ExportSettingsCore(model, typeof(SettingConstants.Roaming));
                ExportSettingsCore(model, typeof(SettingConstants.Download));
                ExportSettingsCore(model, typeof(SettingConstants.Other));

                var modelString = Toml.FromModel(model);

                var bin = Encode(modelString);

                using var stream = await file.OpenStreamForWriteAsync();
                await stream.WriteAsync(bin, 0, bin.Length);
                await stream.FlushAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("导出失败", ex);
                Notify.ShowMessageToast("导出失败，已记录错误");
                return false;
            }
        }

        public async Task<bool> ImportSettings()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".bililiteSettings");
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return false;
            using var openFile = await file.OpenAsync(FileAccessMode.Read);
            using var stream = openFile.AsStreamForRead();
            var bin = new byte[stream.Length];

            await stream.ReadAsync(bin, 0, bin.Length);

            try
            {
                var text = Decode(bin);
                var model = Toml.ToModel(text);

                ImportSettingsCore(model, typeof(SettingConstants.UI));
                ImportSettingsCore(model, typeof(SettingConstants.Account));
                ImportSettingsCore(model, typeof(SettingConstants.VideoDanmaku));
                ImportSettingsCore(model, typeof(SettingConstants.Live));
                ImportSettingsCore(model, typeof(SettingConstants.Player));
                ImportSettingsCore(model, typeof(SettingConstants.Roaming));
                ImportSettingsCore(model, typeof(SettingConstants.Download));
                ImportSettingsCore(model, typeof(SettingConstants.Other));
            }
            catch (Exception ex)
            {
                _logger.Error("导入失败", ex);
                Notify.ShowMessageToast("导入失败，已记录错误");
                return false;
            }
            return true;
        }
    }
}
