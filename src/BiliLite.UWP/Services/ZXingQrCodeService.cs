using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using BiliLite.Services.Interfaces;

namespace BiliLite.Services
{
    public class ZXingQrCodeService : IQrCodeService
    {
        public Task<ImageSource> GenerateQrCode(string content,int size=200)
        {
            var barcodeWriter = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new ZXing.Common.EncodingOptions()
                {
                    Margin = 1,
                    Height = size,
                    Width = size
                }
            };
            var img = barcodeWriter.Write(content);
            return Task.FromResult<ImageSource>(img);
        }
    }
}
