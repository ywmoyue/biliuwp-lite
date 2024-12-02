using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class LiveHomeItemsModel
    {
        [JsonProperty("module_info")]
        public LiveHomeItemsModuleInfoModel ModuleInfo { get; set; }

        public List<LiveHomeItemsItemModel> List { get; set; }
    }
}