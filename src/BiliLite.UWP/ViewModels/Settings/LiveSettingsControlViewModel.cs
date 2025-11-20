using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Windows.Storage.Pickers;
using BiliLite.Extensions;
using Newtonsoft.Json.Serialization;
using System.Linq;

namespace BiliLite.ViewModels.Settings
{
    public class LiveSettingsControlViewModel : BaseViewModel
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public LiveSettingsControlViewModel()
        {
            LoadShieldSetting();
        }

        public ObservableCollection<string> LiveShieldWords { get; set; }

        public void LoadShieldSetting()
        {
            LiveShieldWords = SettingService.GetValue<ObservableCollection<string>>(SettingConstants.Live.SHIELD_WORD, new ObservableCollection<string>() { });
        }

        public async Task ImportDanmuFilter()
        {
            var filePicker = FileExtensions.GetFileOpenPicker();
            filePicker.FileTypeFilter.Add(".json");
            var file = await filePicker.PickSingleFileAsync();
            if (file == null) return;
            using var stream = await file.OpenReadAsync();
            var text = await FileIO.ReadTextAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
            var filterList = JsonConvert.DeserializeObject<List<DanmuFilterItem>>(text);
            ImportDanmakuFilterCore(filterList);
        }

        public async Task ExportDanmuFilter()
        {
            var file = await FileExtensions.GetExportFile("Json", ".json", "bilibili.block");
            if (file == null) return;
            await FileIO.WriteTextAsync(file, String.Empty);

            var danmakuFilter = ExportDanmakuFilterCore();
            var text = JsonConvert.SerializeObject(danmakuFilter, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            using var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
            await stream.WriteAsync(text.StrToBuffer());
        }

        private List<DanmuFilterItem> ExportDanmakuFilterCore()
        {
            var filterList = new List<DanmuFilterItem>();

            // 获取屏蔽词
            var shieldWords = SettingService.GetValue<List<string>>(SettingConstants.Live.SHIELD_WORD, null);
            if (shieldWords != null)
            {
                filterList.AddRange(shieldWords.Select(word => new DanmuFilterItem { Type = 0, Filter = word }));
            }

            return filterList;
        }

        private void ImportDanmakuFilterCore(List<DanmuFilterItem> filterList)
        {
            {
                var words = filterList.Where(x => x.Type == 0).Select(x => x.Filter).ToList();
                var ls = LiveShieldWords.Union(words);
                LiveShieldWords.Clear();
                foreach (var item in ls)
                {
                    LiveShieldWords.Add(item);
                }
                SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, LiveShieldWords);
            }
        }
    }
}
