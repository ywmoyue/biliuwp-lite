using System;
using System.Linq;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Databases;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;

namespace BiliLite.Services
{
    public class SettingService
    {
        private static LocalObjectStorageHelper storageHelper = new LocalObjectStorageHelper();

        public static T GetValue<T>(string key, T _default)
        {
            if (storageHelper.KeyExists(key))
            {
                return storageHelper.Read<T>(key);
            }
            else
            {
                return _default;
            }
        }

        public static void SetValue<T>(string key, T value)
        {
            storageHelper.Save<T>(key, value);
        }

        public static bool HasValue(string key)
        {
            return storageHelper.KeyExists(key);
        }

        public class UI
        {
            private static bool? _loadOriginalImage = null;

            public static bool? LoadOriginalImage
            {
                get
                {
                    if (_loadOriginalImage == null)
                    {
                        _loadOriginalImage = GetValue(SettingConstants.UI.ORTGINAL_IMAGE, false);
                    }
                    return _loadOriginalImage.Value;
                }
                set => _loadOriginalImage = value;
            }
        }

        public class Account
        {
            private static ApiKeyInfo _loginAppKey;

            public static MyProfileModel Profile => storageHelper.Read<MyProfileModel>(SettingConstants.Account.USER_PROFILE);

            public static bool Logined => storageHelper.KeyExists(SettingConstants.Account.ACCESS_KEY) && !string.IsNullOrEmpty(storageHelper.Read<string>(SettingConstants.Account.ACCESS_KEY, null));

            public static string AccessKey => GetValue(SettingConstants.Account.ACCESS_KEY, "");

            public static long UserID => GetValue<long>(SettingConstants.Account.USER_ID, 0);

            public static ApiKeyInfo GetLoginAppKeySecret()
            {
                if (_loginAppKey != null) return _loginAppKey;
                var keySecretPair = GetValue(SettingConstants.Account.LOGIN_APP_KEY_SECRET, "");
                if (string.IsNullOrEmpty(keySecretPair))
                {
                    return SettingConstants.Account.DefaultLoginAppKeySecret;
                }

                var keySecret = keySecretPair.Split(':');

                var key = keySecret[0];
                var secret = keySecret[1];
                var userAgent = "";
                var mobiApp = "";
                switch (key)
                {
                    case Constants.IOS_APP_KEY:
                        mobiApp = Constants.IOS_MOBI_APP;
                        userAgent = Constants.IOS_USER_AGENT;
                        break;
                    case Constants.ANDROID_APP_KEY:
                        mobiApp = Constants.ANDROID_MOBI_APP;
                        userAgent = Constants.ANDROID_USER_AGENT;
                        break;
                }

                var appKey = new ApiKeyInfo(keySecret[0], keySecret[1], mobiApp, userAgent);
                _loginAppKey = appKey;
                return appKey;
            }

            public static void SetLoginAppKeySecret(ApiKeyInfo appKeySecret)
            {
                var keySecretPair = $"{appKeySecret.Appkey}:{appKeySecret.Secret}";
                SetValue(SettingConstants.Account.LOGIN_APP_KEY_SECRET, keySecretPair);
                _loginAppKey = appKeySecret;
            }
        }
    }

    public class SettingSqlService
    {
        private static bool _supportSql = true;
        private readonly BiliLiteDbContext m_biliLiteDbContext;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private const string TEST_SUPPORT = "TEST_SUPPORT_000";

        public SettingSqlService(BiliLiteDbContext biliLiteDbContext)
        {
            m_biliLiteDbContext = biliLiteDbContext;
            CheckSupportSql();
        }

        private void CheckSupportSql()
        {
            try
            {
                // 测试数据库是否支持
                var testSupport = m_biliLiteDbContext.SettingItems.Find(TEST_SUPPORT);
                if (testSupport == null)
                {
                    testSupport = new SettingItem()
                    {
                        Key = TEST_SUPPORT,
                        Value = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ""
                    };
                    m_biliLiteDbContext.SettingItems.Add(testSupport);
                }
                else
                {
                    testSupport.Value = DateTimeOffset.Now.ToUnixTimeMilliseconds() + "";
                }

                m_biliLiteDbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _supportSql = false;
                _logger.Error("检查到sql存储不支持", ex);
            }
        }

        public T GetValue<T>(string key, T _default)
        {
            if (!_supportSql)
            {
                return _default;
            }

            var settingItem = m_biliLiteDbContext.SettingItems.Find(key);
            if (settingItem == null) return _default;

            var result = JsonConvert.DeserializeObject<T>(settingItem.Value);

            return result;
        }

        public void SetValue<T>(string key, T value)
        {
            if (!_supportSql)
            {
                return;
            }
            var settingItem = m_biliLiteDbContext.SettingItems.Find(key);
            if (settingItem == null)
            {
                m_biliLiteDbContext.SettingItems.Add(new SettingItem()
                {
                    Key = key,
                    Value = JsonConvert.SerializeObject(value),
                });
            }
            else
            {
                settingItem.Value = JsonConvert.SerializeObject(value);
            }

            m_biliLiteDbContext.SaveChanges();
        }

        public bool HasValue(string key)
        {
            if (!_supportSql)
            {
                return false;
            }
            return m_biliLiteDbContext.SettingItems.Any(x => x.Key == key);
        }
    }
}
