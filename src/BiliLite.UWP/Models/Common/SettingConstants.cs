﻿using System.Collections.Generic;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Player;
using BiliLite.Services;

namespace BiliLite.Models.Common
{
    public static class SettingConstants
    {
        public class UI
        {
            /// <summary>
            /// 加载原图
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string ORTGINAL_IMAGE = "originalImage";

            /// <summary>
            /// 主题颜色
            /// </summary>
            [SettingKey(typeof(int))]
            public const string THEME_COLOR = "themeColor";

            /// <summary>
            /// 主题,0为默认，1为浅色，2为深色
            /// </summary>
            [SettingKey(typeof(int))]
            public const string THEME = "theme";

            /// <summary>
            /// 显示模式,0为多标签，1为单窗口，2为多窗口
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DISPLAY_MODE = "displayMode";

            /// <summary>
            /// 缓存首页
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string CACHE_HOME = "cacheHome";

            /// <summary>
            /// 首页排序
            /// </summary>
            [SettingKey(typeof(object))]
            public const string HOEM_ORDER = "homePageOrder";

            /// <summary>
            /// 显示推荐页横幅
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string DISPLAY_RECOMMEND_BANNER = "DisplayRecommendBanner";

            /// <summary>
            /// 默认显示推荐页横幅
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_DISPLAY_RECOMMEND_BANNER = true;

            /// <summary>
            /// 右侧详情宽度
            /// </summary>
            [SettingKey(typeof(double))]
            public const string RIGHT_DETAIL_WIDTH = "PlayerRightDetailWidth";

            /// <summary>
            /// 右侧详情宽度可调整
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string RIGHT_WIDTH_CHANGEABLE = "PlayerRightDetailWidthChangeable";

            [SettingKey(typeof(double))]
            public const string DYNAMIC_COMMENT_WIDTH = "DynamicCommentWidth";

            [SettingDefaultValue]
            public const double DEFAULT_DYNAMIC_COMMENT_WIDTH = 480;

            /// <summary>
            /// 图片圆角半径
            /// </summary>
            [SettingKey(typeof(double))]
            public const string IMAGE_CORNER_RADIUS = "ImageCornerRadius";

            /// <summary>
            /// 视频详情显示封面
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SHOW_DETAIL_COVER = "showDetailCover";

            /// <summary>
            /// 新窗口打开图片预览
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string NEW_WINDOW_PREVIEW_IMAGE = "newWindowPreviewImage";
            /// <summary>
            /// 动态显示样式
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DYNAMIC_DISPLAY_MODE = "dynamicDiaplayMode";
            /// <summary>
            /// 首页推荐样式
            /// </summary>
            [SettingKey(typeof(int))]
            public const string RECMEND_DISPLAY_MODE = "recomendDiaplayMode";
            /// <summary>
            /// 右侧选项卡
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DETAIL_DISPLAY = "detailDisplay";
            /// <summary>
            /// 动态显示样式
            /// </summary>
            [SettingKey]
            public const string BACKGROUND_IMAGE = "BackgroundImage";
            /// <summary>
            /// 鼠标功能键行为
            /// </summary>
            [SettingKey(typeof(int))]
            public const string MOUSE_MIDDLE_ACTION = "MouseMiddleAction";

            /// <summary>
            /// 快速收藏
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string QUICK_DO_FAV = "QuickDoFav";

            /// <summary>
            /// 默认快速收藏
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_QUICK_DO_FAV = true;

            /// <summary>
            /// 默认收藏夹
            /// </summary>
            [SettingKey(typeof(string))]
            public const string DEFAULT_USE_FAV = "DefaultUseFav";

            /// <summary>
            /// 默认收藏夹值
            /// </summary>
            [SettingDefaultValue]
            public const string DEFAULT_USE_FAV_VALUE = "默认收藏夹";

            /// <summary>
            /// 隐藏赞助按钮
            /// </summary>
            public const string HIDE_SPONSOR = "HideSponsor";
            /// <summary>
            /// 隐藏广告按钮
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_AD = "HideAD";
            /// <summary>
            /// 浏览器打开无法处理的链接
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string OPEN_URL_BROWSER = "OpenUrlWithBrowser";

            /// <summary>
            /// 启用长评论折叠
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string ENABLE_COMMENT_SHRINK = "EnableCommentShrink";

            /// <summary>
            /// 折叠评论长度
            /// </summary>
            [SettingKey(typeof(int))]
            public const string COMMENT_SHRINK_LENGTH = "CommentShrinkLength";

            /// <summary>
            /// 默认折叠评论长度
            /// </summary>
            [SettingDefaultValue]
            public const int COMMENT_SHRINK_DEFAULT_LENGTH = 75;

            /// <summary>
            /// 显示评论热门回复
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SHOW_HOT_REPLIES = "ShowHotReplies";

            /// <summary>
            /// 显示评论热门回复默认选项
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_SHOW_HOT_REPLIES = true;

            /// <summary>
            /// 视频详情页分集列表设计宽度
            /// </summary>
            [SettingKey(typeof(double))]
            public const string VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH = "VideoDetailListEpisodeDesiredWidth";

            /// <summary>
            /// 视频详情页分集列表设计宽度默认值
            /// </summary>
            [SettingDefaultValue]
            public const double DEFAULT_VIDEO_DETAIL_LIST_EPISODE_DESIRED_WIDTH = 180;

            /// <summary>
            /// 是否在视频底部显示进度条
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR = "ShowVideoBottomVirtualProgressBar";

            /// <summary>
            /// 默认不在视频底部显示进度条
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR = false;

            [SettingKey(typeof(bool))]
            public const string ENABLE_TAB_ITEM_FIXED_WIDTH = "EnableTabItemFixedWidth";

            [SettingDefaultValue]
            public const bool DEFAULT_ENABLE_TAB_ITEM_FIXED_WIDTH = true;

            [SettingKey(typeof(double))]
            public const string TAB_ITEM_FIXED_WIDTH = "TabItemFixedWidth";

            [SettingDefaultValue]
            public const double DEFAULT_TAB_ITEM_FIXED_WIDTH = 224;

            [SettingKey(typeof(double))]
            public const string TAB_ITEM_MAX_WIDTH = "TabItemMaxWidth";

            [SettingDefaultValue]
            public const double DEFAULT_TAB_ITEM_MAX_WIDTH = 240;

            [SettingKey(typeof(double))]
            public const string TAB_ITEM_MIN_WIDTH = "TabItemMinWidth";

            [SettingDefaultValue]
            public const double DEFAULT_TAB_ITEM_MIN_WIDTH = 192;
        }

        public class Account
        {
            /// <summary>
            /// 登录后ACCESS_KEY
            /// </summary>
            [SettingKey]
            public const string ACCESS_KEY = "accesskey";
            /// <summary>
            /// 登录后REFRESH_KEY
            /// </summary>
            [SettingKey]
            public const string REFRESH_KEY = "refreshkey";
            /// <summary>
            /// 到期时间
            /// </summary>
            [SettingKey(typeof(object))]
            public const string ACCESS_KEY_EXPIRE_DATE = "expireDate";
            /// <summary>
            /// 用户ID
            /// </summary>
            [SettingKey(typeof(long))]
            public const string USER_ID = "uid";
            /// <summary>
            /// 到期时间
            /// </summary>
            [SettingKey(typeof(object))]
            public const string USER_PROFILE = "userProfile";

            /// <summary>
            /// 是否web登录
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string IS_WEB_LOGIN = "isWebLogin";

            /// <summary>
            /// Cookies
            /// </summary>
            [SettingKey]
            public const string BILIBILI_COOKIES = "BiliBiliCookies";

            /// <summary>
            /// 登录用AppKey
            /// </summary>
            [SettingKey]
            public const string LOGIN_APP_KEY_SECRET = "LoginAppKeySecret";

            /// <summary>
            /// 默认登录用AppKey
            /// </summary>
            [SettingDefaultValue]
            public static ApiKeyInfo DefaultLoginAppKeySecret = ApiHelper.AndroidKey;

            /// <summary>
            /// Wbi令牌ImgKey参数
            /// </summary>
            [SettingKey]
            public const string WBI_IMG_KEY = "WbiImgKey";

            /// <summary>
            /// Wbi令牌SubKey参数
            /// </summary>
            [SettingKey]
            public const string WBI_SUB_KEY = "WbiSubKey";

            /// <summary>
            /// Wbi令牌参数获取时间（unix时间戳）
            /// </summary>
            [SettingKey(typeof(long))]
            public const string WBI_KEY_TIME = "WbiKeyTime";

            /// <summary>
            /// Wbi令牌参数刷新时间（单位分钟，暂定2小时）
            /// </summary>
            [SettingDefaultValue]
            public const int WBI_KEY_REFRESH_TIME = 120;

            [SettingKey]
            public const string BILI_TICKET = "BiliTicket";
        }

        public class VideoDanmaku
        {
            /// <summary>
            /// 默认弹幕引擎
            /// </summary>
            [SettingDefaultValue]
            public const DanmakuEngineType DEFAULT_DANMAKU_ENGINE = DanmakuEngineType.NSDanmaku;
            /// <summary>
            /// 弹幕引擎
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DANMAKU_ENGINE = "DanmakuEngine";
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHOW = "VideoDanmuShow";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            [SettingKey(typeof(double))]
            public const string FONT_ZOOM = "VideoDanmuFontZoom";
            /// <summary>
            /// 弹幕显示区域
            /// </summary>
            [SettingKey(typeof(double))]
            public const string AREA = "VideoDanmuArea";
            /// <summary>
            /// 弹幕速度 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SPEED = "VideoDanmuSpeed";
            /// <summary>
            /// 弹幕加粗 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string BOLD = "VideoDanmuBold";
            /// <summary>
            /// 弹幕边框样式 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string BORDER_STYLE = "VideoDanmuStyle";
            /// <summary>
            /// 弹幕合并 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string MERGE = "VideoDanmuMerge";
            /// <summary>
            /// 弹幕半屏显示 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string DOTNET_HIDE_SUBTITLE = "VideoDanmuDotHide";
            /// <summary>
            /// 弹幕透明度 double，0-1
            /// </summary>
            [SettingKey(typeof(double))]
            public const string OPACITY = "VideoDanmuOpacity";
            /// <summary>
            /// 隐藏顶部 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_TOP = "VideoDanmuHideTop";
            /// <summary>
            /// 隐藏底部 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_BOTTOM = "VideoDanmuHideBottom";
            /// <summary>
            /// 隐藏滚动 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_ROLL = "VideoDanmuHideRoll";
            /// <summary>
            /// 隐藏高级弹幕 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_ADVANCED = "VideoDanmuHideAdvanced";

            /// <summary>
            /// 关键词屏蔽 ObservableCollection<string>
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHIELD_WORD = "VideoDanmuShieldWord";

            /// <summary>
            /// 用户屏蔽 ObservableCollection<string>
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHIELD_USER = "VideoDanmuShieldUser";

            /// <summary>
            /// 正则屏蔽 ObservableCollection<string>
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHIELD_REGULAR = "VideoDanmuShieldRegular";

            /// <summary>
            /// 顶部距离
            /// </summary>
            [SettingKey(typeof(double))]
            public const string TOP_MARGIN = "VideoDanmuTopMargin";
            /// <summary>
            /// 最大数量
            /// </summary>
            [SettingKey(typeof(double))]
            public const string MAX_NUM = "VideoDanmuMaxNum";
            /// <summary>
            /// 弹幕云屏蔽等级
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SHIELD_LEVEL = "VideoDanmuShieldLevel";

            /// <summary>
            /// 弹幕字体
            /// </summary>
            [SettingKey(typeof(string))]
            public const string DANMAKU_FONT_FAMILY = "VideoDanmuFontFamily";

            /// <summary>
            /// 屏蔽彩色弹幕
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string DISABLE_COLORFUL = "DisableColorful";
        }

        public class Live
        {
            /// <summary>
            /// 默认弹幕引擎
            /// </summary>
            [SettingDefaultValue]
            public const DanmakuEngineType DEFAULT_DANMAKU_ENGINE = DanmakuEngineType.NSDanmaku;

            /// <summary>
            /// 弹幕引擎
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DANMAKU_ENGINE = "DanmakuEngine";

            /// <summary>
            /// 直播默认清晰度
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_QUALITY = "LiveDefaultQuality";
            /// <summary>
            /// 显示弹幕 Visibility
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHOW = "LiveDanmuShow";

            [SettingKey(typeof(double))]
            public const string AREA = "LiveDanmuArea";
            /// <summary>
            /// 弹幕缩放 double
            /// </summary>
            [SettingKey(typeof(double))]
            public const string FONT_ZOOM = "LiveDanmuFontZoom";
            /// <summary>
            /// 弹幕速度 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SPEED = "LiveDanmuSpeed";
            /// <summary>
            /// 弹幕加粗 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string BOLD = "LiveDanmuBold";
            /// <summary>
            /// 弹幕边框样式 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string BORDER_STYLE = "LiveDanmuStyle";
            /// <summary>
            /// 弹幕半屏显示 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string DOTNET_HIDE_SUBTITLE = "LiveDanmuDotHide";
            /// <summary>
            /// 弹幕透明度 double，0-1
            /// </summary>
            [SettingKey(typeof(double))]
            public const string OPACITY = "LiveDanmuOpacity";
            /// <summary>
            /// 关键词屏蔽 ObservableCollection<string>
            /// </summary>
            [SettingKey(typeof(object))]
            public const string SHIELD_WORD = "LiveDanmuShieldWord";

            /// <summary>
            /// 硬解 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HARDWARE_DECODING = "LiveHardwareDecoding";

            /// <summary>
            /// 自动开启宝箱 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_OPEN_BOX = "LiveAutoOpenBox";

            /// <summary>
            /// 直播弹幕延迟
            /// </summary>
            public const string DELAY = "LiveDelay";

            /// <summary>
            /// 直播弹幕清理
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DANMU_CLEAN_COUNT = "LiveCleanCount";

            /// <summary>
            /// 隐藏进场
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_WELCOME = "LiveHideWelcome";

            /// <summary>
            /// 隐藏礼物
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_GIFT = "LiveHideGift";

            /// <summary>
            /// 隐藏公告
            /// </summary>
            public const string HIDE_SYSTEM = "LiveSystemMessage";
            /// <summary>
            /// 隐藏抽奖
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_LOTTERY = "LiveHideLottery";

            /// <summary>
            /// 隐藏抽奖弹幕关键字
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HIDE_LOTTERY_DANMU = "LiveHideLotteryDanmu";

            /// <summary>
            /// 直播流默认源
            /// </summary>
            [SettingKey(typeof(string))]
            public const string DEFAULT_LIVE_PLAY_URL_SOURCE = "DefaultLivePlayUrlSource";

            /// <summary>
            /// 低延迟模式开关
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string LOW_DELAY_MODE = "LowDelayMode";

            /// <summary>
            /// 低延迟模式开关
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_LOW_DELAY_MODE = false;

            /// <summary>
            /// 显示底部礼物栏
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SHOW_BOTTOM_GIFT_BAR = "ShowBottomGiftBar";

            /// <summary>
            /// 默认显示底部礼物栏
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_SHOW_BOTTOM_GIFT_BAR = true;

            /// <summary>
            /// 弹幕字体
            /// </summary>
            [SettingKey(typeof(string))]
            public const string DANMAKU_FONT_FAMILY = "VideoDanmuFontFamily";
        }

        public class Player
        {
            /// <summary>
            /// 优先使用播放器类型
            /// </summary>
            [SettingKey(typeof(int))]
            public const string USE_REAL_PLAYER_TYPE = "UseRealPlayerType";

            [SettingDefaultValue]
            public const RealPlayerType DEFAULT_USE_REAL_PLAYER_TYPE = RealPlayerType.Native;

            [SettingKey(typeof(object))]
            public const string FfmpegOptions = "FfmpegOptions";

                /// <summary>
            /// 使用外站视频替换无法播放的视频 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string USE_OTHER_SITEVIDEO = "PlayerUseOther";

            /// <summary>
            /// 硬解 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HARDWARE_DECODING = "PlayerHardwareDecoding";

            /// <summary>
            /// 自动播放 bool
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_PLAY = "PlayerAutoPlay";
            /// <summary>
            /// 自动切换下一个视频
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_NEXT = "PlayerAutoNext";
            /// <summary>
            /// 默认清晰度 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_QUALITY = "PlayerDefaultQuality";

            /// <summary>
            /// 默认音质 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_SOUND_QUALITY = "PlayerDefaultSoundQuality";

            /// <summary>
            /// 启用默认最大音质
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string ENABLE_DEFAULT_MAX_SOUND_QUALITY = "PlayerEnableDefaultMaxSoundQuality";

            /// <summary>
            /// 默认不启用默认最大音质
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_ENABLE_DEFAULT_MAX_SOUND_QUALITY = false;

            /// <summary>
            /// 比例 int
            /// </summary>
            [SettingKey(typeof(int))]
            public const string RATIO = "PlayerDefaultRatio";

            /// <summary>
            /// 默认视频类型 int flv=0, dash=1,dash_hevc=2
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_VIDEO_TYPE = "PlayerDefaultVideoType";

            [SettingDefaultValue]
            public static List<double> VideoSpeed = new List<double>() { 2.0d, 1.5d, 1.25d, 1.0d, 0.75d, 0.5d };

            /// <summary>
            /// 默认视频类型 int 1.0
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_VIDEO_SPEED = "PlayerDefaultSpeed";

            /// <summary>
            /// 播放模式 int 0=顺序播放，1=单集循环，2=列表循环
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_PLAY_MODE = "PlayerDefaultPlayMode";
            /// <summary>
            /// 音量
            /// </summary>
            [SettingKey(typeof(double))]
            public const string PLAYER_VOLUME = "PlayerVolume";

            /// <summary>
            /// 音量默认值
            /// </summary>
            [SettingDefaultValue]
            public const double DEFAULT_PLAYER_VOLUME = 1.0;

            /// <summary>
            /// 亮度
            /// </summary>
            [SettingKey(typeof(double))]
            public const string PLAYER_BRIGHTNESS = "PlayeBrightness";

            /// <summary>
            /// 亮度默认值
            /// </summary>
            [SettingDefaultValue]
            public const double DEFAULT_PLAYER_BRIGHTNESS = 0;

            /// <summary>
            /// 锁定播放器音量设置（播放器内修改音量时不写设置）
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string LOCK_PLAYER_VOLUME = "LockPlayerVolume";

            /// <summary>
            /// 锁定播放器音量设置默认值
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_LOCK_PLAYER_VOLUME = false;

            /// <summary>
            /// 锁定播放器亮度设置（播放器内修改亮度时不写设置）
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string LOCK_PLAYER_BRIGHTNESS = "LockPlayerBrightness";

            /// <summary>
            /// 锁定播放器亮度设置默认值
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_LOCK_PLAYER_BRIGHTNESS = true;

            /// <summary>
            /// A-B 循环播放模式的播放记录
            /// </summary>
            [SettingKey(typeof(object))]
            public const string PLAYER_ABPLAY_HISTORIES = "PlayerABPlayHistories";

            /// <summary>
            /// 字幕颜色
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SUBTITLE_COLOR = "subtitleColor";
            /// <summary>
            /// 字幕描边颜色
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SUBTITLE_BORDER_COLOR = "subtitleBorderColor";

            /// <summary>
            /// 字幕边框背景颜色
            /// </summary>
            [SettingKey(typeof(string))]
            public const string SUBTITLE_OUTSIDE_BORDER_COLOR = "subtitleOutsideBorderColor";

            /// <summary>
            /// 默认字幕边框背景颜色
            /// </summary>
            [SettingDefaultValue] public const string DEFAULT_SUBTITLE_OUTSIDE_BORDER_COLOR = "#00FFFFFF";

            /// <summary>
            /// 字幕大小
            /// </summary>
            [SettingKey(typeof(double))]
            public const string SUBTITLE_SIZE = "subtitleSize";
            /// <summary>
            /// 字幕显示
            /// </summary>
            public const string SUBTITLE_SHOW = "subtitleShow";
            /// <summary>
            /// 字幕透明度
            /// </summary>
            [SettingKey(typeof(double))]
            public const string SUBTITLE_OPACITY = "subtitleOpacity";
            /// <summary>
            /// 字幕底部距离
            /// </summary>
            [SettingKey(typeof(double))]
            public const string SUBTITLE_BOTTOM = "subtitleBottom";
            /// <summary>
            /// 字幕加粗
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SUBTITLE_BOLD = "subtitleBold";
            /// <summary>
            /// 字幕对齐
            /// 0=居中对齐，1=左对齐，2=右对齐
            /// </summary>
            [SettingKey(typeof(int))]
            public const string SUBTITLE_ALIGN = "subtitleAlign";
            /// <summary>
            /// 自动跳转进度
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_TO_POSITION = "PlayerAutoToPosition";
            /// <summary>
            /// 自动铺满窗口
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_FULL_WINDOW = "PlayerAutoToFullWindow";
            /// <summary>
            /// 自动铺满全屏
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_FULL_SCREEN = "PlayerAutoToFullScreen";
            /// <summary>
            /// 双击全屏
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string DOUBLE_CLICK_FULL_SCREEN = "PlayerDoubleClickFullScreen";

            /// <summary>
            /// 方向键右键行为
            /// </summary>
            [SettingKey(typeof(int))]
            public const string PLAYER_KEY_RIGHT_ACTION = "PlayerKeyRightAction";

            /// <summary>
            /// 按住手势行为
            /// </summary>
            [SettingKey(typeof(int))]
            public const string HOLDING_GESTURE_ACTION = "HoldingGestureAction";

            /// <summary>
            /// 按住手势可被其他手势取消
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string HOLDING_GESTURE_CAN_CANCEL = "HoldingGestureCanCancel";

            /// <summary>
            /// 倍速播放速度
            /// </summary>
            [SettingKey(typeof(double))]
            public const string HIGH_RATE_PLAY_SPEED = "HighRatePlaySpeed";

            [SettingDefaultValue]
            public static List<double> HIGH_RATE_PLAY_SPEED_LIST = new List<double>() { 3.0d, 2.0d };

            /// <summary>
            /// 自动打开AI字幕
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_OPEN_AI_SUBTITLE = "PlayerAutoOpenAISubtitle";


            /// <summary>
            /// 替换CDN
            /// </summary>
            [SettingKey(typeof(int))]
            public const string REPLACE_CDN = "PlayerReplaceCDN";

            /// <summary>
            /// CDN服务器
            /// </summary>
            [SettingKey]
            public const string CDN_SERVER = "PlayerCDNServer";

            /// <summary>
            /// 直播播放器默认模式
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_LIVE_PLAYER_MODE = "DefaultLivePlayerMode";

            [SettingKey(typeof(bool))]
            public const string REPORT_HISTORY = "ReportHistory";

            [SettingDefaultValue]
            public const bool DEFAULT_REPORT_HISTORY = true;

            [SettingKey(typeof(bool))]
            public const string REPORT_HISTORY_ZERO_WHEN_VIDEO_END = "ReportHistoryZeroWhenVideoEnd";

            [SettingDefaultValue]
            public const bool DEFAULT_REPORT_HISTORY_ZERO_WHEN_VIDEO_END = false;

            [SettingKey(typeof(string))]
            public const string PLAY_SPEED_MENU = "PlaySpeedMenu";

            /// <summary>
            /// 自动跳过OP/ED
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string AUTO_SKIP_OP_ED = "AutoSkipOpEd";

            /// <summary>
            /// 默认不自动跳过OP/ED
            /// </summary>
            [SettingDefaultValue]
            public const bool DEFAULT_AUTO_SKIP_OP_ED = false;
        }

        public class Filter
        {
            [SettingKey(typeof(object), useSqlDb: true)]
            public const string RECOMMEND_FILTER_RULE = "RecommendFilterRule";

            [SettingKey(typeof(object), useSqlDb: true)]
            public const string SEARCH_FILTER_RULE = "SearchFilterRule";

            [SettingKey(typeof(object), useSqlDb: true)]
            public const string DYNAMIC_FILTER_RULE = "DynamicFilterRule";
        }

        public class Roaming
        {
            /// <summary>
            /// 自定义服务器
            /// </summary>
            public const string CUSTOM_SERVER = "RoamingCustomServer";
            /// <summary>
            /// 自定义服务器链接
            /// </summary>
            [SettingKey(typeof(string))]
            public const string CUSTOM_SERVER_URL = "RoamingCustomServerUrl";

            /// <summary>
            /// 自定义香港服务器链接
            /// </summary>
            [SettingKey(typeof(string))]
            public const string CUSTOM_SERVER_URL_HK = "RoamingCustomServerUrlHK";

            /// <summary>
            /// 自定义台湾服务器链接
            /// </summary>
            [SettingKey(typeof(string))]
            public const string CUSTOM_SERVER_URL_TW = "RoamingCustomServerUrlTW";

            /// <summary>
            /// 自定义大陆服务器链接
            /// </summary>
            [SettingKey(typeof(string))]
            public const string CUSTOM_SERVER_URL_CN = "RoamingCustomServerUrlCN";

            /// <summary>
            /// 简体中文
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string TO_SIMPLIFIED = "RoamingSubtitleToSimplified";
            ///// <summary>
            ///// 只使用AkamaiCDN链接
            ///// </summary>
            //public const string AKAMAI_CDN = "RoamingAkamaiCDN";
        }

        public class Download
        {
            /// <summary>
            /// 下载目录
            /// </summary>
            [SettingKey(typeof(string))]
            public const string DOWNLOAD_PATH = "downloadPath";
            [SettingDefaultValue]
            public const string DEFAULT_PATH = "视频库/哔哩哔哩下载";
            /// <summary>
            /// 旧版下载目录
            /// </summary>
            [SettingKey(typeof(string))]
            public const string OLD_DOWNLOAD_PATH = "downloadOldPath";
            [SettingDefaultValue]
            public const string DEFAULT_OLD_PATH = "视频库/BiliBiliDownload";
            /// <summary>
            /// 允许付费网络下载
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string ALLOW_COST_NETWORK = "allowCostNetwork";

            /// <summary>
            /// 并行下载
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string PARALLEL_DOWNLOAD = "parallelDownload";

            /// <summary>
            /// 并行下载
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string SEND_TOAST = "sendToast";

            /// <summary>
            /// 加载旧版下载视频
            /// </summary>
            [SettingKey(typeof(bool))]
            public const string LOAD_OLD_DOWNLOAD = "loadOldDownload";

            /// <summary>
            /// 下载视频类型
            /// </summary>
            [SettingKey(typeof(int))]
            public const string DEFAULT_VIDEO_TYPE = "DownloadDefaultVideoType";

            [SettingKey(typeof(bool))]
            public const string USE_DOWNLOAD_INDEX = "UseDownloadIndex";

            [SettingDefaultValue]
            public const bool DEFAULT_USE_DOWNLOAD_INDEX = false;
        }

        /// <summary>
        /// 快捷键
        /// </summary>
        public class ShortcutKey
        {
            [SettingKey(typeof(int))]
            public const string PRESS_ACTION_DELAY_TIME = "PressActionDelayTime";

            [SettingDefaultValue]
            public const int DEFAULT_PRESS_ACTION_DELAY_TIME = 200;

            [SettingKey(typeof(object), useSqlDb: true)]
            public const string SHORTCUT_KEY_FUNCTIONS = "ShortcutKeyFunctions";
        }

        /// <summary>
        /// 开发者选项
        /// </summary>
        public class Other
        {
            /// <summary>
            /// 自动清理日志文件
            /// </summary>
            public const string AUTO_CLEAR_LOG_FILE = "autoClearLogFile";

            /// <summary>
            /// 自动清理多少天前的日志文件
            /// </summary>
            public const string AUTO_CLEAR_LOG_FILE_DAY = "autoClearLogFileDay";

            /// <summary>
            /// 保护日志敏感信息
            /// </summary>
            public const string PROTECT_LOG_INFO = "protectLogInfo";

            /// <summary>
            /// 日志级别
            /// </summary>
            public const string LOG_LEVEL = "LogLevel";

            /// <summary>
            /// 忽略版本
            /// </summary>
            public const string IGNORE_VERSION = "ignoreVersion";

            /// <summary>
            /// WebApi地址
            /// </summary>
            public const string BILI_LITE_WEB_API_BASE_URL = "BiliLiteWebApiBaseUrl";

            /// <summary>
            /// 优先使用Grpc请求动态
            /// </summary>
            public const string FIRST_GRPC_REQUEST_DYNAMIC = "FirstGrpcRequestDynamic";

            /// <summary>
            /// 更新json请求地址
            /// </summary>
            public const string UPDATE_JSON_ADDRESS = "RawRepositoryAddress";

            /// <summary>
            /// 发起请求时使用的build值
            /// </summary>
            public const string REQUEST_BUILD = "RequestBuild";

            /// <summary>
            /// 默认发起请求时使用的build值
            /// </summary>
            [SettingDefaultValue]
            public const string DEFAULT_REQUEST_BUILD = "75900200";

            public const string SQL_DB_VERSION = "SqlDbVersion";
        }
    }
}
