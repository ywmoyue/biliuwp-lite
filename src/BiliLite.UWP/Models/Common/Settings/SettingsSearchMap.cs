using System.Collections.Generic;

namespace BiliLite.Models.Common.Settings
{
    public static class SettingsSearchMap
    {
        private static IReadOnlyDictionary<string, string> _map = new Dictionary<string, string>()
        {
            // UISettings
            { "主题", "UISettings:SCTheme" },
            { "更改应用的日夜主题", "UISettings:SCTheme" },
            { "更改应用的边角锐度", "UISettings:SCStyle" },
            { "色彩", "UISettings:rootColor" },
            { "主题背景透明", "UISettings:PageBackgroundMicaBrush" },
            { "选择应用的显示模式", "UISettings:SCDisplayMode" },
            { "固定标签页宽度", "UISettings:SwitchTabItemFixedWidth:SETabPage" },
            { "设置标签页最小宽度", "UISettings:NumTabItemMinWidth:SETabPage" },
            { "设置标签页最大宽度", "UISettings:NumTabItemMaxWidth:SETabPage" },
            { "设置标签页高度", "UISettings:NumTabHeight:SETabPage" },
            { "首页栏目", "UISettings:gridHomeCustom" },
            { "视图布局", "UISettings:SELayout" },
            { "推荐页布局样式", "UISettings:cbRecommendDisplayMode:SELayout" },
            { "动态页布局样式", "UISettings:cbRecommendDisplayMode:SELayout" },
            { "横幅", "UISettings:SEBanner" },
            { "启用推荐页横幅", "UISettings:SEBanner" },
            { "启用直播页横幅", "UISettings:SEBanner" },
            { "图片预览", "UISettings:SEImagePre" },
            { "使用新窗口预览图片", "UISettings:swPreviewImageNavigateToPage:SEImagePre" },
            { "全屏化图片新窗口", "UISettings:swPreviewImageNavigateToPageFully:SEImagePre" },
            { "评论页", "UISettings:SEComment" },
            { "评论页总宽度", "UISettings:NumBoxDynamicCommentWidth:SEComment" },
            { "评论页文字折叠长度", "UISettings:swEnableCommentShrink:SEComment" },
            { "展示评论热门回复", "UISettings:swShowHotReplies:SEComment" },
            { "直播页关闭推荐直播", "UISettings:SELiveStyle" },
            { "视频页", "UISettings:SEVideoStyle" },
            { "视频页启用分隔栏", "UISettings:swRightWidthChangeable:SEVideoStyle" },
            { "视频信息总宽度", "UISettings:numRightWidth:SEVideoStyle" },
            { "分集列表宽度", "UISettings:NumListEpisodeDesiredWidth:SEVideoStyle" },
            { "视频页默认栏目", "UISettings:cbDetailDisplay:SEVideoStyle" },
            { "视频页显示封面", "UISettings:swVideoDetailShowCover:SEVideoStyle" },
            { "快速收藏", "UISettings:SwitchQuickDoFav" },
            { "动态磁贴", "UISettings:SwitchTile" },


            // Play Settings
            { "优先视频编码", "PlaySettings:cbVideoType" },
            { "优先播放器类型", "PlaySettings:ComboBoxUseRealPlayerType" },
            { "FFMpegInteropX 额外播放参数", "PlaySettings:m_viewModel.FFmpegInteropXOptions:SEPlayerType" },
            { "挂载本地文件", "PlaySettings:BtnSelectWebPlayerFile:SEPlayerType" },
            { "启用调试模式", "PlaySettings:SwitchEnableWebPlayerDebugMode:SEPlayerType" },
            { "启用开发模式", "PlaySettings:SwitchEnableWebPlayerDevMode:SEPlayerType" },
            { "WebPlayer 音视频进度同步阈值1", "PlaySettings:NumberBoxWebPlayerAVPositionSyncThreshold1:SEPlayerType" },
            { "WebPlayer 音视频进度同步阈值2", "PlaySettings:NumberBoxWebPlayerAVPositionSyncThreshold2:SEPlayerType" },
            { "播放倍速", "PlaySettings:BtnEditPlaySpeedMenu" },
            { "默认播放倍速", "PlaySettings:cbVideoSpeed" },
            { "临时播放倍速", "PlaySettings:cbRatePlaySpeed" },
            { "空降助手", "PlaySettings:SpBlockCard" },
            { "连续播放分P视频", "PlaySettings:swAutoNext:SEPlayerAuto" },
            { "自动播放加载完成的视频", "PlaySettings:swAutoPlay:SEPlayerAuto" },
            { "自动跳过番剧 OP/ED", "PlaySettings:SwSkipOpEd:SEPlayerAuto" },
            { "自动全屏播放", "PlaySettings:swPlayerSettingAutoFullScreen:SEPlayerAuto" },
            { "自动隐藏视频信息", "PlaySettings:swPlayerSettingAutoFullWindows:SEPlayerAuto" },
            { "默认最大音质", "PlaySettings:SwitchEnableDefaultMaxSoundQuality:SEPlayerAuto" },
            { "音量", "PlaySettings:SliderVolume" },
            { "锁定默认音量", "PlaySettings:SwLockPlayerVolume:SEVolume" },
            { "亮度", "PlaySettings:SliderBrightness" },
            { "锁定默认亮度", "PlaySettings:SwLockPlayerBrightness:SEBrightness" },
            { "从已有的记录位置继续观看", "PlaySettings:swPlayerSettingAutoToPosition:SEPlayHistory" },
            { "上报历史记录", "PlaySettings:SwitchPlayerReportHistory:SEPlayHistory" },
            { "视频结束后不上报历史记录", "PlaySettings:SwitchReportHistoryZeroWhenVideoEnd:SEPlayHistory" },
            { "从头播放完播视频", "PlaySettings:NumReplayVideoFromEndLastTime:SEPlayHistory" },
            { "播放源自动刷新", "PlaySettings:SwitchAutoRefreshPlayUrl:SEPlaySource" },
            { "播放源刷新间隔", "PlaySettings:NumAutoRefreshPlayUrlTime:SEPlaySource" },
            { "总是显示进度条", "PlaySettings:SwAlwaysShowVideoProgress:SEProgress" },
            { "自动最小化到播放窗口底部", "PlaySettings:SwShowVideoBottomProgress:SEProgress" },
            { "自动打开 AI 字幕", "PlaySettings:swPlayerSettingAutoOpenAISubtitle:SESub" },
            { "繁体字幕转简体", "PlaySettings:RoamingSettingToSimplified:SESub" },
            { "播放器按住手势行为", "PlaySettings:cbPlayerHoldingGestureAction" },
            { "双击播放器全屏", "PlaySettings:swPlayerSettingDoubleClickFullScreen" },

            // Performance Settings
            { "图片质量", "PerformanceSettings:btnCleanImageCache" },
            { "加载原图", "PerformanceSettings:swPictureQuality" },
            { "缓存首页", "PerformanceSettings:swHomeCache" },
            { "使用内置浏览器", "PerformanceSettings:swOpenUrlWithBrowser" },
            { "恢复未关闭的页面", "PerformanceSettings:SwitchOpenLastPage" },
            { "需要恢复的数量", "PerformanceSettings:NumberOpenLastPageCount" },
            { "每次滚动加载更多数据", "PerformanceSettings:NumberScrollViewLoadMoreBottomOffset" },

            // Proxy Settings
            { "首选代理服务器", "ProxySettings:RoamingSettingCustomServer" },
            { "自定义港澳代理服务器", "ProxySettings:RoamingSettingCustomServerHK" },
            { "自定义台湾代理服务器", "ProxySettings:RoamingSettingCustomServerTW" },
            { "自定义大陆代理服务器", "ProxySettings:RoamingSettingCustomServerCN" },
            { "替换 CDN 链接", "ProxySettings:cbPlayerReplaceCDN" },
            { "替换 CDN 服务器", "ProxySettings:RoamingSettingCDNServer" },

            // Video Danmaku Settings
            { "弹幕引擎", "VideoDanmakuSettings:cbDanmakuEngine" },
            { "默认弹幕状态", "VideoDanmakuSettings:DanmuSettingState" },
            { "弹幕顶部距离", "VideoDanmakuSettings:numDanmakuTopMargin" },
            { "弹幕每秒最大数量", "VideoDanmakuSettings:numDanmakuMaxNum" },
            { "弹幕屏蔽", "VideoDanmakuSettings:DanmuSettingSyncWords" },
            { "关键词屏蔽", "VideoDanmakuSettings:DanmuSettingTxtWord" },
            { "正则屏蔽", "VideoDanmakuSettings:DanmuSettingTxtRegex" },
            { "用户屏蔽", "VideoDanmakuSettings:DanmuSettingTxtUser" },

            // Live Settings
            { "直播弹幕引擎", "LiveSettings:cbLiveDanmakuEngine" },
            { "直播默认弹幕状态", "LiveSettings:LiveDanmuSettingState" },
            { "直播弹幕屏蔽", "LiveSettings:LiveDanmuSettingTxtWord" },

            // Download Settings
            { "下载存放目录", "DownloadSettings:txtDownloadPath" },
            { "旧版下载目录", "DownloadSettings:txtDownloadOldPath" },
            { "加载旧版下载的视频", "DownloadSettings:swDownloadLoadOld" },
            { "优先下载视频类型", "DownloadSettings:cbDownloadVideoType" },
            { "同时下载多个任务", "DownloadSettings:swDownloadParallelDownload" },
            { "允许使用流量下载", "DownloadSettings:swDownloadAllowCostNetwork" },
            { "下载完成发送通知", "DownloadSettings:swDownloadSendToast" },
            { "使用下载索引", "DownloadSettings:SwUseDownloadIndex" },

            // Dev Settings
            { "收集级别", "DevSettings:cbLogLevel" },
            { "保护日志中敏感信息", "DevSettings:swProtectLogInfo" },
            { "自动清理日志文件", "DevSettings:swAutoClearLogFile" },
            { "自动清理几天前的日志", "DevSettings:numAutoClearLogDay" },
            { "优先使用 Grpc 请求动态", "DevSettings:swFirstGrpcRequestDynamic" },
            { "发起请求时使用的 Build 值", "DevSettings:RequestBuildTextBox" },
            { "BiliLite-WebApi", "DevSettings:BiliLiteWebApiTextBox" },
            { "更新 JSON 请求地址", "DevSettings:updateJsonAddress" },

            // Filter Settings
            { "过滤推荐页直播", "FilterSettings:FilterRecommendLiveSwitch" },
            { "推荐页过滤", "FilterSettings:RecommendFilterRuleListView" },
            { "搜索页过滤", "FilterSettings:SearchFilterRuleListView" },
            { "动态页过滤", "FilterSettings:DynamicFilterRuleListView" },

            // ShortcutKeySettings
            { "快捷键自定义", "ShortcutKeySettings:ShortcutKeySettingPanel" },
            { "选择鼠标中键/侧键行为", "ShortcutKeySettings:cbMouseMiddleAction" },
        };

        public static IReadOnlyDictionary<string, string> Map => _map;
    }
}
