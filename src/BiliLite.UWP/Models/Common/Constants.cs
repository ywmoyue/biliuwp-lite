namespace BiliLite.Models.Common
{
    public static class Constants
    {
        /// <summary>
        /// 获取Cookie使用的域名
        /// </summary>
        public const string GET_COOKIE_DOMAIN = "https://bilibili.com";

        /// <summary>
        /// b站官网域名
        /// </summary>
        public const string BILIBILI_DOMAIN = "https://www.bilibili.com";

        /// <summary>
        /// b站Host
        /// </summary>
        public const string BILIBILI_HOST = ".bilibili.com";

        /// <summary>
        /// 评论中匹配特殊文本正则表达式
        /// </summary>
        public const string COMMENT_SPECIAL_TEXT_REGULAR = @"\[(.*?)\]|https?:\/\/\S+|http?:\/\/\S+|\p{Cs}";

        public const string CHROME_USER_AGENT = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
        
        public const string IOS_USER_AGENT = "bili-universal/75200100 CFNetwork/1.0 Darwin/23.0.0 os/ios model/iPad Air 3G mobi_app/iphone build/75200100 osVer/17.0.3 network/2 channel/AppStore";

        public const string ANDROID_USER_AGENT = CHROME_USER_AGENT;

        public const string IOS_MOBI_APP = "iphone";

        public const string ANDROID_MOBI_APP = "android";

        public const string IOS_APP_KEY = "27eb53fc9058f8c3";

        public const string ANDROID_APP_KEY = "1d8b6e7d45233436";

        public static class App
        {
            /// <summary>
            /// 透明图片
            /// </summary>
            public const string TRANSPARENT_IMAGE = "ms-appx:///Assets/MiniIcon/transparent.png";

            /// <summary>
            /// 个人认证图片
            /// </summary>
            public const string VERIFY_PERSONAL_IMAGE = "ms-appx:///Assets/Icon/verify0.png";

            /// <summary>
            /// 企业认证图片
            /// </summary>
            public const string VERIFY_OGANIZATION_IMAGE = "ms-appx:///Assets/Icon/verify1.png";

            /// <summary>
            /// 背景图片
            /// </summary>
            public const string BACKGROUND_IAMGE_URL = "ms-appx:///Assets/Image/background.jpg";
        }

        public static class Images
        {
            /// <summary>
            /// 榜单图标
            /// </summary>
            public const string RANK_ICON_IMAGE = "ms-appx:///Assets/Icon/榜单.png";

            /// <summary>
            /// 索引图标
            /// </summary>
            public const string INDEX_ICON_IMAGE = "ms-appx:///Assets/Icon/索引.png";

            /// <summary>
            /// 时间表图标
            /// </summary>
            public const string TIMELINE_ICON_IMAGE = "ms-appx:///Assets/Icon/时间表.png";

            /// <summary>
            /// 我的图标
            /// </summary>
            public const string MY_ICON_IMAGE = "ms-appx:///Assets/Icon/我的.png";
        }

        public static class DynamicTypes
        {
            public const string AV = "Av";

            public const string PGC = "Pgc";

            public const string WORD = "Word";

            public const string DRAW = "Draw";

            public const string MUSIC = "Music";

            public const string ARTICLE = "Article";

            public const string FORWARD = "Forward";

            public const string COMMON_SQUARE = "CommonSquare";
        }
    }
}
