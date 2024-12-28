namespace BiliLite.Models.Common
{
    public enum LogType
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Fatal,
        Necessary,
    }

    public enum HttpMethods
    {
        Get,
        Post,
        Put,
        Patch,
        Delete,
    }

    public enum LoginStatus
    {
        /// <summary>
        /// 登录成功
        /// </summary>
        Success,
        /// <summary>
        /// 登录失败
        /// </summary>
        Fail,
        /// <summary>
        /// 登录错误
        /// </summary>
        Error,
        /// <summary>
        /// 登录需要验证码
        /// </summary>
        NeedCaptcha,
        /// <summary>
        /// 需要安全认证
        /// </summary>
        NeedValidate
    }

    public enum LoginQRStatusCode
    {
        /// <summary>
        /// 扫码成功
        /// </summary>
        Success = 0,

        /// <summary>
        /// 二维码失效
        /// </summary>
        Fail = 86038,

        /// <summary>
        /// 二维码已扫码未确认
        /// </summary>
        Unconfirmed = 86090,

        /// <summary>
        /// 未扫码
        /// </summary>
        NotScanned = 86101,
    }

    public enum MouseMiddleActions
    {
        /// <summary>
        /// 返回或关闭页面
        /// </summary>
        Back = 0,

        /// <summary>
        /// 打开新标签页但不跳转 
        /// </summary>
        NewTap = 1,

        /// <summary>
        /// 无操作
        /// </summary>
        None = 2,
    }

    public enum DownloadType
    {
        /// <summary>
        /// 视频
        /// </summary>
        Video = 0,
        /// <summary>
        /// 番剧、电影、电视剧等
        /// </summary>
        Season = 1,
        /// <summary>
        /// 音乐，暂不支持
        /// </summary>
        Music = 2,
        /// <summary>
        /// 课程，暂不支持
        /// </summary>
        Cheese = 3
    }

    public enum PlayerKeyRightAction
    {
        /// <summary>
        /// 操作进度条
        /// </summary>
        ControlProgress,

        /// <summary>
        /// 倍速播放
        /// </summary>
        AcceleratePlay
    }

    public enum PlayerHoldingAction
    {
        /// <summary>
        /// 无
        /// </summary>
        None,

        /// <summary>
        /// 倍速播放
        /// </summary>
        AcceleratePlay
    }

    public enum BiliPlayUrlType
    {
        /// <summary>
        /// 单段FLV
        /// </summary>
        SingleFLV,

        /// <summary>
        /// 多段FLV
        /// </summary>
        MultiFLV,

        /// <summary>
        /// 音视频分离DASH流
        /// </summary>
        DASH
    }

    public enum BiliPlayUrlVideoCodec
    {
        AVC = 7,
        HEVC = 12,
        AV1 = 13,
    }

    public enum PlayState
    {
        Loading,
        Playing,
        Pause,
        End,
        Error
    }

    public enum PlayEngine
    {
        Native = 1,
        FFmpegInteropMSS = 2,
        SYEngine = 3,
        FFmpegInteropMSSH265 = 4,
        VLC = 5
    }

    public enum PlayMediaType
    {
        Single,
        MultiFlv,
        Dash
    }

    public enum DynamicType
    {
        /// <summary>
        /// 用户关注动态
        /// </summary>
        UserDynamic,
        /// <summary>
        /// 话题动态
        /// </summary>
        Topic,
        /// <summary>
        /// 个人空间动态
        /// </summary>
        Space
    }

    public enum AnimeType
    {
        /// <summary>
        /// 番剧
        /// </summary>
        Bangumi = 1,

        /// <summary>
        /// 国创
        /// </summary>
        GuoChuang = 4
    }

    public enum DanmakuEngineType
    {
        NSDanmaku = 0,
        FrostDanmakuMaster = 1,
    }

    public enum PlayUrlCodecMode
    {
        // int flv=0, dash=1,dash_hevc=2
        FLV = 0,
        DASH_H264 = 1,
        DASH_H265 = 2,
        DASH_AV1 = 3
    }

    public enum UserDynamicDisplayType
    {
        /// <summary>
        /// 转发
        /// </summary>
        Repost,

        /// <summary>
        /// 文本
        /// </summary>
        Text,

        /// <summary>
        /// 图片
        /// </summary>
        Photo,

        /// <summary>
        /// 视频
        /// </summary>
        Video,

        /// <summary>
        /// 短视频
        /// </summary>
        ShortVideo,

        /// <summary>
        /// 番剧/影视
        /// </summary>
        Season,

        SeasonV2,

        /// <summary>
        /// 音乐
        /// </summary>
        Music,

        /// <summary>
        /// 网页、活动
        /// </summary>
        Web,

        /// <summary>
        /// 文章
        /// </summary>
        Article,

        /// <summary>
        /// 直播
        /// </summary>
        Live,

        /// <summary>
        /// 分享直播
        /// </summary>
        LiveShare,

        /// <summary>
        /// 付费课程
        /// </summary>
        Cheese,

        /// <summary>
        /// 播放列表(公开的收藏夹)
        /// </summary>
        MediaList,

        /// <summary>
        /// 缺失的，动态可能被删除
        /// </summary>
        Miss,

        /// <summary>
        /// 其他
        /// </summary>
        Other
    }

    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 连接成功
        /// </summary>
        ConnectSuccess,

        ///// <summary>
        ///// 在线人数(即人气值, 已弃用)
        ///// </summary>
        //Online,

        /// <summary>
        /// 弹幕
        /// </summary>
        Danmu,

        /// <summary>
        /// 赠送礼物
        /// </summary>
        Gift,

        /// <summary>
        /// 欢迎信息
        /// </summary>
        InteractWord,

        /// <summary>
        /// 系统消息 (未做实现), 先注释
        /// </summary>
        //SystemMsg,

        /// <summary>
        /// 醒目留言
        /// </summary>
        SuperChat,

        /// <summary>
        /// 醒目留言（日文）
        /// </summary>
        SuperChatJpn,

        /// <summary>
        /// 抽奖开始
        /// </summary>
        AnchorLotteryStart,


        /// 抽奖结果
        /// </summary>
        AnchorLotteryAward,

        /// <summary>
        /// 欢迎舰长
        /// </summary>
        WelcomeGuard,

        /// <summary>
        /// 上舰
        /// </summary>
        GuardBuy,

        /// <summary>
        /// 新上舰消息, 可区分续费和新人
        /// </summary>
        GuardBuyNew,

        /// <summary>
        /// 房间信息更新
        /// </summary>
        RoomChange,

        /// <summary>
        /// 指定观众禁言
        /// </summary>
        RoomBlock,

        /// <summary>
        /// 超管警告或切断
        /// </summary>
        WaringOrCutOff,

        /// <summary>
        /// 开始直播
        /// </summary>
        StartLive,

        /// <summary>
        /// 看过直播的人数变化(代替人气值)
        /// </summary>
        WatchedChange,

        /// <summary>
        /// 红包抽奖开始
        /// </summary>
        RedPocketLotteryStart,

        /// <summary>
        /// 红包抽奖赢家
        /// </summary>
        RedPocketLotteryWinner,

        /// <summary>
        /// 高能榜变动
        /// </summary>
        OnlineRankChange,

        /// <summary>
        /// 停止直播
        /// </summary>
        StopLive,

        /// <summary>
        /// 直播间等级禁言
        /// </summary>
        ChatLevelMute,

        /// <summary>
        /// 直播间观看人数变动
        /// </summary>
        OnlineCountChange,
    }

    public enum MessageDelayType {

        /// <summary>
        /// 常规弹幕消息
        /// </summary>
        DanmuMessage,

        /// <summary>
        /// 礼物消息
        /// </summary>
        GiftMessage,

        /// <summary>
        /// 其他系统消息
        /// </summary>
        SystemMessage,
    }

    public enum SeasonIdType
    {
        SeasonId,
        EpId,
    }

    public enum UserDynamicShowType
    {
        All = 0,
        Video = 1,
        Season = 2,
        Article = 3
    }

    /// <summary>
    /// 用户大航海等级
    /// </summary>
    public enum UserCaptainType
    {
        None = 0,

        /// <summary>
        /// 总督
        /// </summary>
        Zongdu = 1,

        /// <summary>
        /// 提督
        /// </summary>
        Tidu = 2,

        /// <summary>
        /// 舰长
        /// </summary>
        JianZhang = 3,
    }

    /// <summary>
    /// 排行榜分区类型
    /// </summary>
    public enum RankRegionType
    {
        /// <summary>
        /// 全部
        /// </summary>
        All,
        /// <summary>
        /// 原创
        /// </summary>
        Origin,
        /// <summary>
        /// 新人
        /// </summary>
        Rookie
    }

    public enum FilterRuleType
    {
        Recommend,
        Search,
        Dynamic,
    }

    public enum FilterType
    {
        /// <summary>
        /// 关键词
        /// </summary>
        Word,
        /// <summary>
        /// 正则
        /// </summary>
        Regular,
    }

    public enum FilterContentType
    {
        Title,
        User,
        Desc,
    }

    public enum SearchType
    {
        /// <summary>
        /// 视频
        /// </summary>
        Video = 0,
        /// <summary>
        /// 番剧
        /// </summary>
        Anime = 1,
        /// <summary>
        /// 直播
        /// </summary>
        Live = 2,
        /// <summary>
        /// 主播
        /// </summary>
        Anchor = 3,
        /// <summary>
        /// 用户
        /// </summary>
        User = 4,
        /// <summary>
        /// 影视
        /// </summary>
        Movie = 5,
        /// <summary>
        /// 专栏
        /// </summary>
        Article = 6,
        /// <summary>
        /// 话题
        /// </summary>
        Topic = 7
    }

    public enum IndexSeasonType
    {
        Anime = 1,
        Movie = 2,
        Documentary = 3,
        Guochuang = 4,
        TV = 5,
        Variety = 7
    }

    public enum DownloadedSortMode
    {
        Default,
        TimeDesc,
        TimeAsc,
        TitleDesc,
        TitleAsc,
    }
}