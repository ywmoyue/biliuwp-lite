using System;
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

namespace BiliLite.Services
{
    public class SettingsImportExportService
    {
        private const string SettingsExportKey = "4bmsD9chgoP1jdohv2+OV9Pv2403r4IfJU18ixpFWQA=";
        private const string SettingsExportIV = "ABxrLAWa7MrKg6w1xxtZmw==";

        private AesCryptoServiceProvider aesProvider;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;

        public SettingsImportExportService()
        {
            aesProvider = new AesCryptoServiceProvider();
            aesProvider.Key = Convert.FromBase64String(SettingsExportKey);
            aesProvider.IV = Convert.FromBase64String(SettingsExportIV);
            aesProvider.Mode = CipherMode.CBC;
            aesProvider.Padding = PaddingMode.PKCS7;

            encryptor = aesProvider.CreateEncryptor(aesProvider.Key, aesProvider.IV);
            decryptor = aesProvider.CreateDecryptor(aesProvider.Key, aesProvider.IV);
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
            byte[] encrypted;
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using var swEncrypt = new StreamWriter(csEncrypt);
            swEncrypt.Write(plainText);
            encrypted = msEncrypt.ToArray();

            return encrypted;
        }

        public string DecryptFromBinary(byte[] cipherTextBinary)
        {
            string plaintext = null;
            using var msDecrypt = new MemoryStream(cipherTextBinary);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);
            plaintext = srDecrypt.ReadToEnd();

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

        public async Task ExportSettings()
        {
            var folder = await new FolderPicker().PickSingleFolderAsync();
            if (folder == null) return;
            var file = await folder.CreateFileAsync($"{DateTime.Now.ToString("yyyy-M-d-HH_mm_ss")}.bililiteSettings");

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
        }

        public async Task ExportSettingsWithAccount()
        {
            var folder = await new FolderPicker().PickSingleFolderAsync();
            if (folder == null) return;
            var file = await folder.CreateFileAsync($"{DateTime.Now.ToString("yyyy-M-d-HH_mm_ss")}.bililiteSettings");

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
        }

        public async Task ImportSettings()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".bililiteSettings");
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return;
            using var openFile = await file.OpenAsync(FileAccessMode.Read);
            using var stream = openFile.AsStreamForRead();
            var bin = new byte[stream.Length];

            await stream.ReadAsync(bin, 0, bin.Length);

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
    }
}
