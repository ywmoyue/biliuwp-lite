using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using BiliLite.Models.Attributes;
using Windows.Security.Cryptography.Core;

namespace BiliLite.Services.Biz
{
    /// <summary>
    /// 小工具（原 BiliLite.WebApi 的 UtilsController 内置化）
    /// </summary>
    [RegisterSingletonService]
    public class UtilsBizService : BaseBizService
    {
        /// <summary>
        /// B站刷新Cookie所用的RSA公钥（X509 SubjectPublicKeyInfo，Base64）
        /// </summary>
        private const string PUBLIC_KEY =
            "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDLgd2OAkcGVtoE3ThUREbio0Eg" +
            "Uc/prcajMKXvkCKFCWhJYJcLkcM2DKKcSeFpD/j6Boy538YXnR6VhcuUJOhH2x71" +
            "nzPjfdTcqMz7djHum0qSZA0AyCBDABUqCrfNgCiJ00Ra7GmRj+YCK1NJEuewlb40" +
            "JNrRuoEUXpabUzGB8QIDAQAB";

        /// <summary>
        /// 构造 CorrespondPath：用 RSA-OAEP-SHA256 加密 refresh_{timestamp}，输出小写十六进制
        /// </summary>
        /// <param name="timestamp">服务端返回的 timestamp</param>
        public string BuildCorrespondPath(long timestamp)
        {
            var provider = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaOaepSha256);
            var key = provider.ImportPublicKey(
                Convert.FromBase64String(PUBLIC_KEY).AsBuffer(),
                CryptographicPublicKeyBlobType.X509SubjectPublicKeyInfo);

            var payload = Encoding.UTF8.GetBytes($"refresh_{timestamp}").AsBuffer();
            var ciphertext = CryptographicEngine.Encrypt(key, payload, null);

            return BitConverter.ToString(ciphertext.ToArray()).Replace("-", "").ToLower();
        }
    }
}
