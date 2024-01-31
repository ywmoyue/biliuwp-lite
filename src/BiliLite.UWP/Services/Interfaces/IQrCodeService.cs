using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace BiliLite.Services.Interfaces
{
    public interface IQrCodeService
    {
        public Task<ImageSource> GenerateQrCode(string content, int size = 200);
    }
}
