using Flurl.Http.Configuration;
using System.Net.Http;

namespace BiliLite.Modules.SpBlock.Services;

public class SponsorBlockHttpClientFactory : DefaultHttpClientFactory
{
    public override HttpMessageHandler CreateMessageHandler()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
    }
}