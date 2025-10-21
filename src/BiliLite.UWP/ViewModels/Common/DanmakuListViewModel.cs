using System.Collections.Generic;
using BiliLite.Models.Common.Danmaku;
using PropertyChanged;

namespace BiliLite.ViewModels.Common;

public class DanmakuListViewModel : BaseViewModel
{
    private const string RECENT_DANMU_TITLE = "前后10秒内弹幕";
    private const string ALL_DANMU_TITLE = "弹幕池中全部弹幕";

    public List<DanmakuSimpleItem> Danmakus { get; set; }

    [DependsOn(nameof(ShowAllDanmu))]
    public string Title => ShowAllDanmu ? ALL_DANMU_TITLE : RECENT_DANMU_TITLE;

    public bool ShowAllDanmu { get; set; }
}