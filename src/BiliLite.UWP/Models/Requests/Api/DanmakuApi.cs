using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Models.Requests.Api;

public class DanmakuApi : BaseApi
{
    private readonly CookieService m_cookieService;
    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

    public DanmakuApi()
    {
        m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
    }

    /// <summary>
    /// 点赞弹幕
    /// </summary>
    /// <param name="dmid">弹幕id</param>
    /// <param name="oid">视频cid</param>
    /// <param name="op">操作（1点赞，2取消点赞）</param>
    /// <returns></returns>
    public ApiModel Like(string dmid, string oid, int op)
    {
        var csrf = m_cookieService.GetCSRFToken();
        var api = new ApiModel()
        {
            method = HttpMethods.Post,
            baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/dm/thumbup/add",
            body = $"&dmid={dmid}&oid={oid}&op={op}&csrf={csrf}",
            need_cookie = true,
        };
        return api;
    }
}