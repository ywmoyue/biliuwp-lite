using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models.Attributes;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BiliLite.Services
{
    /// <summary>
    /// 为 UP 主生成桌面快捷方式：bilibili:// 协议链接 + 圆形头像图标
    /// </summary>
    [RegisterSingletonService]
    public class DesktopShortcutService
    {
        private static readonly ILogger m_logger = GlobalLogger.FromCurrentType();
        private static readonly HttpClient m_httpClient = new HttpClient();

        /// <summary>
        /// 生成图标的尺寸，256 用 0 表示（ICO 规范）
        /// </summary>
        private static readonly int[] IconSizes = { 256, 64, 32 };

        /// <summary>
        /// 快捷方式冷启动标记参数，冷启动时需要靠它等待外壳完成布局
        /// </summary>
        private const string ShortcutQuery = "shortcut=1";

        private const string ShortcutFolderName = "shortcuts";

        static DesktopShortcutService()
        {
            m_httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }

        /// <summary>
        /// 创建指向 UP 主主页的桌面快捷方式
        /// </summary>
        /// <param name="mid">UP 主 mid</param>
        /// <param name="userName">UP 主昵称，用于快捷方式文件名</param>
        /// <param name="faceUrl">头像地址，失败时退化为无自定义图标</param>
        public async Task<bool> CreateUserSpaceShortcutAsync(string mid, string userName, string faceUrl)
        {
            if (string.IsNullOrWhiteSpace(mid))
            {
                m_logger.Warn("创建桌面快捷方式失败：mid 为空");
                return false;
            }

            try
            {
                var iconPath = "";
                if (!string.IsNullOrWhiteSpace(faceUrl))
                {
                    try
                    {
                        iconPath = await CreateIconFileAsync(mid, faceUrl);
                    }
                    catch (Exception ex)
                    {
                        // 图标失败不阻断快捷方式创建
                        m_logger.Warn($"创建快捷方式图标失败，将使用默认图标：{ex.Message}");
                        iconPath = "";
                    }
                }

                var shortcutPath = GetShortcutPath(userName, mid);
                // .url 使用 UTF-16 编码写入，保证 IconFile 路径包含非 ASCII 字符时也能被正确解析
                await File.WriteAllTextAsync(shortcutPath, BuildShortcutContent(mid, iconPath),
                    new System.Text.UnicodeEncoding(false, true));
                m_logger.Info($"已创建桌面快捷方式：{shortcutPath}");
                return true;
            }
            catch (Exception ex)
            {
                m_logger.Error("创建桌面快捷方式失败", ex);
                return false;
            }
        }

        private static async Task<string> CreateIconFileAsync(string mid, string faceUrl)
        {
            var folder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(ShortcutFolderName,
                CreationCollisionOption.OpenIfExists);
            // 文件名带 mid：不同 UP 主不会复用同一个图标文件，避免图标缓存串号
            var file = await folder.CreateFileAsync($"up_{mid}.ico", CreationCollisionOption.ReplaceExisting);

            var faceBytes = await DownloadAsync(faceUrl);
            var frames = new List<byte[]>();
            foreach (var size in IconSizes)
            {
                frames.Add(await CreateRoundPngAsync(faceBytes, size));
            }

            await FileIO.WriteBytesAsync(file, BuildIco(frames));
            return file.Path;
        }

        private static async Task<byte[]> CreateRoundPngAsync(byte[] faceBytes, int size)
        {
            using var sourceStream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(sourceStream))
            {
                writer.WriteBytes(faceBytes);
                await writer.StoreAsync();
                await writer.FlushAsync();
                writer.DetachStream();
            }

            sourceStream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(sourceStream);
            var transform = new BitmapTransform
            {
                ScaledWidth = (uint)size,
                ScaledHeight = (uint)size,
                InterpolationMode = BitmapInterpolationMode.Cubic,
            };

            using var bitmap = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            var pixels = ReadPixels(bitmap);
            ApplyCircleMask(pixels, size, size);

            using var masked = CreateSoftwareBitmap(pixels, size, size);
            using var pngStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, pngStream);
            encoder.SetSoftwareBitmap(masked);
            await encoder.FlushAsync();
            return await ReadStreamBytesAsync(pngStream);
        }

        private static byte[] ReadPixels(SoftwareBitmap bitmap)
        {
            var length = (uint)(bitmap.PixelWidth * bitmap.PixelHeight * 4);
            var buffer = new Windows.Storage.Streams.Buffer(length) { Length = length };
            bitmap.CopyToBuffer(buffer);
            using var reader = DataReader.FromBuffer(buffer);
            var bytes = new byte[buffer.Length];
            reader.ReadBytes(bytes);
            return bytes;
        }

        private static SoftwareBitmap CreateSoftwareBitmap(byte[] pixels, int width, int height)
        {
            using var writer = new DataWriter();
            writer.WriteBytes(pixels);
            var buffer = writer.DetachBuffer();
            return SoftwareBitmap.CreateCopyFromBuffer(buffer, BitmapPixelFormat.Bgra8,
                width, height, BitmapAlphaMode.Straight);
        }

        /// <summary>
        /// 把方形头像裁成圆形，边缘留一像素做透明度过渡，避免锯齿
        /// </summary>
        private static void ApplyCircleMask(byte[] pixels, int width, int height)
        {
            var centerX = (width - 1) / 2.0;
            var centerY = (height - 1) / 2.0;
            var radius = Math.Min(width, height) / 2.0;
            const double edge = 1.0;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    var index = (y * width + x) * 4;

                    if (distance <= radius - edge)
                    {
                        continue;
                    }

                    var alpha = distance >= radius + edge
                        ? 0d
                        : (radius + edge - distance) / (edge * 2);
                    var factor = Math.Clamp(alpha, 0d, 1d);
                    pixels[index] = (byte)(pixels[index] * factor);
                    pixels[index + 1] = (byte)(pixels[index + 1] * factor);
                    pixels[index + 2] = (byte)(pixels[index + 2] * factor);
                    pixels[index + 3] = (byte)(pixels[index + 3] * factor);
                }
            }
        }

        /// <summary>
        /// 组装 ICO 文件，每个尺寸用一张 PNG 帧（Vista 及以上支持 PNG 压缩帧）
        /// </summary>
        private static byte[] BuildIco(IReadOnlyList<byte[]> pngFrames)
        {
            using var memory = new MemoryStream();
            using var writer = new BinaryWriter(memory);

            writer.Write((ushort)0); // reserved
            writer.Write((ushort)1); // type: icon
            writer.Write((ushort)pngFrames.Count);

            var offset = 6 + 16 * pngFrames.Count;
            for (var i = 0; i < pngFrames.Count; i++)
            {
                var size = IconSizes[i];
                writer.Write((byte)(size >= 256 ? 0 : size)); // width，256 记为 0
                writer.Write((byte)(size >= 256 ? 0 : size)); // height，256 记为 0
                writer.Write((byte)0); // 调色板数量
                writer.Write((byte)0); // reserved
                writer.Write((ushort)1); // color planes
                writer.Write((ushort)32); // bits per pixel
                writer.Write(pngFrames[i].Length);
                writer.Write(offset);
                offset += pngFrames[i].Length;
            }

            foreach (var frame in pngFrames)
            {
                writer.Write(frame);
            }

            writer.Flush();
            return memory.ToArray();
        }

        private static async Task<byte[]> ReadStreamBytesAsync(IRandomAccessStream stream)
        {
            stream.Seek(0);
            var size = (uint)stream.Size;
            using var reader = new DataReader(stream);
            await reader.LoadAsync(size);
            var bytes = new byte[size];
            reader.ReadBytes(bytes);
            return bytes;
        }

        private static async Task<byte[]> DownloadAsync(string url)
        {
            using var response = await m_httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsByteArrayAsync();
        }

        private static string BuildShortcutContent(string mid, string iconPath)
        {
            var builder = new StringBuilder();
            builder.AppendLine("[InternetShortcut]");
            // 必须带 shortcut 参数：冷启动时应用靠它判断需要等待外壳完成布局再处理协议链接
            builder.AppendLine($"URL=bilibili://space.bilibili.com/{mid}?{ShortcutQuery}");
            if (!string.IsNullOrEmpty(iconPath))
            {
                builder.AppendLine($"IconFile={iconPath}");
                builder.AppendLine("IconIndex=0");
            }

            return builder.ToString();
        }

        private static string GetShortcutPath(string userName, string mid)
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var fileName = SanitizeFileName(userName);
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = $"UP主_{mid}";
            }

            var shortcutPath = Path.Combine(desktopPath, fileName + ".url");
            if (!File.Exists(shortcutPath))
            {
                return shortcutPath;
            }

            // 已有同名快捷方式：同一个 UP 主直接覆盖，不同 UP 主则带上 mid 避免互相覆盖
            try
            {
                var existing = File.ReadAllText(shortcutPath);
                if (existing.Contains($"/{mid}?", StringComparison.Ordinal))
                {
                    return shortcutPath;
                }
            }
            catch (Exception)
            {
                // 读不出来就当作冲突处理
            }

            return Path.Combine(desktopPath, $"{fileName}_{mid}.url");
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            var builder = new StringBuilder(name.Trim());
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                builder.Replace(invalid, '_');
            }

            var sanitized = builder.ToString().Trim().TrimEnd('.');
            return sanitized.Length > 60 ? sanitized.Substring(0, 60) : sanitized;
        }
    }
}
