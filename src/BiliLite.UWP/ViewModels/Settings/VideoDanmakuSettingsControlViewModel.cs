using System;
using System.Collections.Generic;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Settings
{
    public class VideoDanmakuSettingsControlViewModel : BaseViewModel
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly PlayerAPI m_playerAPI;

        public VideoDanmakuSettingsControlViewModel()
        {
            m_playerAPI = new PlayerAPI();
            LoadShieldSetting();
        }

        /// <summary>
        /// 弹幕屏蔽关键词列表
        /// </summary>
        public ObservableCollection<string> ShieldWords { get; set; }
        public ObservableCollection<string> ShieldUsers { get; set; }
        public ObservableCollection<string> ShieldRegulars { get; set; }

        public void LoadShieldSetting()
        {
            ShieldWords = SettingService.GetValue<ObservableCollection<string>>(SettingConstants.VideoDanmaku.SHIELD_WORD, new ObservableCollection<string>() { });

            //正则关键词
            ShieldRegulars = SettingService.GetValue<ObservableCollection<string>>(SettingConstants.VideoDanmaku.SHIELD_REGULAR, new ObservableCollection<string>() { });

            //用户
            ShieldUsers = SettingService.GetValue<ObservableCollection<string>>(SettingConstants.VideoDanmaku.SHIELD_USER, new ObservableCollection<string>() { });
        }
        public async Task ImportDanmuFilter()
        {
            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".json");
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return;
            using var stream = await file.OpenReadAsync();
            var text = await stream.ReadTextAsync(Encoding.UTF8);
            var filterList = JsonConvert.DeserializeObject<List<DanmuFilterItem>>(text);

            ImportDanmakuFilterCore(filterList);
        }

        public async Task SyncDanmuFilter()
        {
            try
            {
                var result = await m_playerAPI.GetDanmuFilterWords().Request();
                if (!result.status)
                {
                    Notify.ShowMessageToast(result.message);
                    return;
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() == 0)
                {
                    var items = JsonConvert.DeserializeObject<List<DanmuFilterItem>>(obj["data"]["rule"].ToString());

                    ImportDanmakuFilterCore(items);
                }
                else
                {
                    Notify.ShowMessageToast(obj["message"].ToString());
                }
            }
            catch (Exception ex)
            {
                _logger.Log("读取弹幕屏蔽词失败", LogType.Error, ex);
            }
        }

        public async Task<bool> AddDanmuFilterItem(string word, int type)
        {
            try
            {
                var result = await m_playerAPI.AddDanmuFilterWord(word: word, type: type).Request();
                if (!result.status)
                {
                    return false;
                }
                var obj = result.GetJObject();
                return obj["code"].ToInt32() == 0;
            }
            catch (Exception ex)
            {
                _logger.Log("添加弹幕屏蔽词失败", LogType.Error, ex);
                return false;
            }
        }

        private void ImportDanmakuFilterCore(List<DanmuFilterItem> filterList)
        {
            {
                var words = filterList.Where(x => x.Type == 0).Select(x => x.Filter).ToList();
                var ls = ShieldWords.Union(words);
                ShieldWords.Clear();
                foreach (var item in ls)
                {
                    ShieldWords.Add(item);
                }
                SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, ShieldWords);
            }
            {
                var users = filterList.Where(x => x.Type == 1).Select(x => x.Filter).ToList();
                var ls = ShieldRegulars.Union(users);
                ShieldRegulars.Clear();
                foreach (var item in ls)
                {
                    if (!item.IsValidRegex())
                    {
                        _logger.Warn("非法正则表达式: " + item);
                        continue;
                    }
                    ShieldRegulars.Add(item);
                }
                SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_REGULAR, ShieldRegulars);
            }
            {
                var users = filterList.Where(x => x.Type == 2).Select(x => x.Filter).ToList();
                var ls = ShieldUsers.Union(users);
                ShieldUsers.Clear();
                foreach (var item in ls)
                {
                    ShieldUsers.Add(item);
                }
                SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_USER, ShieldUsers);
            }
        }
    }
}
