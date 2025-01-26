using System;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace BiliLite.Models.Requests.Api.User
{
    public class MessageApi : BaseApi
    {
        private readonly CookieService m_cookieService;

        public MessageApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        public ApiModel MessageSessions(int page = 1, int pagesize = 20, long? endTime = null)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/session_svr/v1/session_svr/get_sessions",
                parameter =
                    $"session_type=1&group_fold=1&unfollow_fold=0&sort_rule=2&build=0&mobi_app=web",
                need_cookie = true,
            };

            if (endTime != null)
            {
                api.parameter += $"&end_ts={endTime}";
            }

            return api;
        }

        public ApiModel UserList(List<long> userIdList)
        {
            var uids = string.Join(',', userIdList);
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/polymer/pc-electron/v1/user/cards",
                parameter = $"uids={uids}",
                need_cookie = true,
            };

            return api;
        }

        public ApiModel SessionMsgs(string talkerId, int sessionType, int size = 20, string beginSeqno = null, string endSeqno = null)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/svr_sync/v1/svr_sync/fetch_session_msgs",
                parameter = $"talker_id={talkerId}&session_type={sessionType}&size={size}&sender_device_id=1&build=0" +
                            $"&mobi_app=web",
                need_cookie = true,
            };

            if (beginSeqno != null)
            {
                api.parameter += $"&begin_seqno={beginSeqno}";
            }

            if (endSeqno != null)
            {
                api.parameter += $"&end_seqno={endSeqno}";
            }

            return api;
        }

        public ApiModel SendMsg(string senderId, string receiverId, int receiveType, int msgType, string content, string devId)
        {
            var csrf = m_cookieService.GetCSRFToken();

            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.vc.bilibili.com/web_im/v1/web_im/send_msg",
                parameter = $"w_sender_uid={senderId}&w_receiver_id={receiverId}",
                body = $"msg[sender_uid]={senderId}&" +
                     $"msg[receiver_id]={receiverId}&" +
                     $"msg[receiver_type]={receiveType}&" +
                     $"msg[msg_type]={msgType}&" +
                     $"msg[msg_status]=0&" +
                     $"msg[content]={content}&" +
                     $"msg[timestamp]={DateTimeOffset.Now.ToUnixTimeSeconds()}&" +
                     $"msg[new_face_version]=0&" +
                     $"msg[dev_id]={devId}&" +
                     $"from_firework=0&" +
                     $"build=0&" +
                     $"mobi_app=web&" +
                     $"csrf_token={csrf}&" +
                     $"csrf={csrf}",
                need_cookie = true,
            };

            return api;
        }

        public ApiModel UpdateAck(string talkerId, int sessionType, string seqno)
        {
            var csrf = m_cookieService.GetCSRFToken();

            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.vc.bilibili.com/session_svr/v1/session_svr/update_ack",
                body = $"talker_id={talkerId}&session_type={sessionType}&ack_seqno={seqno}&build=0&mobi_app=web&csrf_token={csrf}&csrf={csrf}",
                need_cookie = true,
            };

            return api;
        }

        public ApiModel ReplyMe(long? cursorId, long? cursorTime)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/msgfeed/reply",
                parameter = $"platform=web&build=0&mobi_app=web",
                need_cookie = true,
            };
            if (cursorId != null && cursorTime != null)
            {
                api.parameter += $"&id={cursorId}&reply_time={cursorTime}";
            }
            return api;
        }
    }
}
