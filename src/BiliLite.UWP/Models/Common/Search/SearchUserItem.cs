namespace BiliLite.Models.Common.Search
{
    public class SearchUserItem
    {
        public string mid { get; set; }
        public string uname { get; set; }
        private string _pic;
        public string upic
        {
            get { return _pic; }
            set { _pic = "https:" + value; }
        }

        public int level { get; set; }
        public int videos { get; set; }
        public int fans { get; set; }
        public int is_upuser { get; set; }
        public string lv
        {
            get
            {
                return $"ms-appx:///Assets/Icon/lv{level}.png";
            }
        }
        public SearchUserOfficialVerifyItem official_verify { get; set; }
        public string Verify
        {
            get
            {
                if (official_verify == null)
                {
                    return "";
                }
                switch (official_verify.type)
                {
                    case 0:
                        return Constants.App.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return Constants.App.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }
        public string usign { get; set; }
        public string sign
        {
            get
            {
                if (official_verify != null && !string.IsNullOrEmpty(official_verify.desc))
                {
                    return official_verify.desc;
                }
                return usign;
            }
        }
    }
}