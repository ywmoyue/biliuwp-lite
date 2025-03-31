using System;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.ViewModels.UserDynamic;

namespace BiliLite.Services.Biz;

[RegisterTransientService]
public class UserDynamicService : BaseBizService
{
    private readonly DynamicAPI m_dynamicAPI;
    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

    public UserDynamicService()
    {
        m_dynamicAPI = new DynamicAPI();
    }

    private async Task DoLikeCore(DynamicV2ItemViewModel item)
    {
        var results = await m_dynamicAPI.Like(item.Extend.DynIdStr, item.Liked ? 2 : 1).Request();
        if (!results.status)
        {
            throw new CustomizedErrorException(results.message);
        }

        var data = await results.GetJson<ApiDataModel<object>>();
        if (!data.success)
        {
            throw new CustomizedErrorException(data.message);
        }

        if (item.Liked)
        {
            item.Liked = false;
            item.LikeCount -= 1;
        }
        else
        {
            item.Liked = true;
            item.LikeCount += 1;
        }
    }

    public async Task DoLike(DynamicV2ItemViewModel item)
    {
        if (!await BiliExtensions.ActionCheckLogin()) return;

        try
        {
            await DoLikeCore(item);
        }
        catch (Exception ex)
        {
            HandleError(ex);
        }
    }
}