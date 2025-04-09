using System.Text;

namespace BiliLite.Player.Mpv
{
    public static class MpvUtilsExtensions
    {
        public static byte[] GetUtf8Bytes(string s)
        {
            return Encoding.UTF8.GetBytes(s + "\0");
        }
    }
}
