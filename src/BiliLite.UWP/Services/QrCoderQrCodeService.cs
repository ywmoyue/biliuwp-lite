using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using BiliLite.Services.Interfaces;
using QRCoder;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliLite.Services
{
    public class QrCoderQrCodeService : IQrCodeService
    {
        public async Task<ImageSource> GenerateQrCode(string content, int size = 200)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            var qrCodeBmp = new BitmapByteQRCode(qrCodeData);
            var qrCodeImageBmp = qrCodeBmp.GetGraphic(size);

            using var stream = new InMemoryRandomAccessStream();
            using var writer = new DataWriter(stream.GetOutputStreamAt(0));
            writer.WriteBytes(qrCodeImageBmp);
            await writer.StoreAsync();
            var image = new BitmapImage();
            await image.SetSourceAsync(stream);
            return image;
        }
    }
}
