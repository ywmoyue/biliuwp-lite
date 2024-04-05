using System;
using System.Threading.Tasks;
using Bilibili.App.Dynamic.V2;
using Bilibili.App.Interface.V1;
using BiliLite.gRPC.Api;
using BiliLite.Models.Exceptions;

namespace BiliLite.Services
{
    public class GrpcService
    {
        public async Task<SearchArchiveReply> SearchSpaceArchive(string mid, int page = 1, int pageSize = 30, string keyword = "")
        {
            var message = new SearchArchiveReq()
            {
                Keyword = keyword,
                Mid = long.Parse(mid),
                Pn = page,
                Ps = pageSize,
            };
            var requestUserInfo = new GrpcBiliUserInfo(
                SettingService.Account.AccessKey, 
                SettingService.Account.UserID,
                SettingService.Account.GetLoginAppKeySecret().Appkey);

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.interface.v1.Space/SearchArchive", message, requestUserInfo);
            if (result.status)
            {
                var reply = SearchArchiveReply.Parser.ParseFrom(result.results);
                return reply;
            } 
            // 用户搜索一个不存在的关键字导致的
            else if (result.code == -102 && result.message == "请求失败,没有数据返回")
            {
                throw new NotFoundException(result.message);
            }
            else
            {
                throw new Exception(result.message);
            }
        }

        public async Task<DynAllReply> GetDynAll(int page = 1)
        {
            var message = new DynAllReq()
            {
                Page = page
            };
            var requestUserInfo = new GrpcBiliUserInfo(
                SettingService.Account.AccessKey,
                SettingService.Account.UserID,
                SettingService.Account.GetLoginAppKeySecret().Appkey);

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.dynamic.v2.Dynamic/DynAll", message, requestUserInfo);
            if (result.status)
            {
                var reply = DynAllReply.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }

        public async Task<DynSpaceRsp> GetDynSpace(long mid,string from = "space", int page = 1,string offset = null)
        {
            var message = new DynSpaceReq()
            {
                Page = page,
                From = from,
                HostUid = mid
            };
            if (offset != null)
            {
                message.HistoryOffset = offset;
            }
            var requestUserInfo = new GrpcBiliUserInfo(
                SettingService.Account.AccessKey,
                SettingService.Account.UserID,
                SettingService.Account.GetLoginAppKeySecret().Appkey);

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.dynamic.v2.Dynamic/DynSpace", message, requestUserInfo);
            if (result.status)
            {
                var reply = DynSpaceRsp.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }

        public async Task<DynVideoReply> GetDynVideo(int page, string historyOffset, string updateBaseline)
        {
            var message = new DynVideoReq()
            {
                LocalTime = 8,
            };
            if (page > 1)
            {
                message.Offset = historyOffset;
                message.UpdateBaseline = updateBaseline;
                message.Page = page;
                message.RefreshType = Refresh.History;
            }
            var requestUserInfo = new GrpcBiliUserInfo(
                SettingService.Account.AccessKey,
                SettingService.Account.UserID,
                SettingService.Account.GetLoginAppKeySecret().Appkey);

            var result = await GrpcRequest.Instance.SendMessage("https://grpc.biliapi.net:443/bilibili.app.dynamic.v2.Dynamic/DynVideo", message, requestUserInfo);
            if (result.status)
            {
                var reply = DynVideoReply.Parser.ParseFrom(result.results);
                return reply;
            }
            else
            {
                throw new Exception(result.message);
            }
        }
    }
}
