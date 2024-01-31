namespace BiliLite.Models.Common
{
    public class WbiKey
    {
        public WbiKey(string imgKey, string subKey)
        {
            ImgKey = imgKey;
            SubKey = subKey;
        }

        public WbiKey()
        {
            
        }

        public string ImgKey { get; set; }

        public string SubKey { get; set; }
    }
}
