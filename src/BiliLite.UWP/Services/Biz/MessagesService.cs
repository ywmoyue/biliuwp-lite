using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Msg;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.ViewModels.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Services.Biz
{
    [RegisterTransientService]
    public class MessagesService : BaseBizService
    {
        private readonly MessageApi m_messageApi;
        private readonly IMapper m_mapper;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public MessagesService(IMapper mapper)
        {
            m_mapper = mapper;
            m_messageApi = new MessageApi();
        }

        private async Task<List<UserInfo>> GetUserList(List<long> userIdList)
        {
            var api = m_messageApi.UserList(userIdList);
            var results = await api.Request();
            if (!results.status)
                throw new CustomizedErrorException(results.message);
            var data = await results.GetData<JObject>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            var result = new List<UserInfo>();

            foreach (var userId in userIdList)
            {
                var userInfo =
                    JsonConvert.DeserializeObject<UserInfo>(JsonConvert.SerializeObject(data.data[$"{userId}"]));
                result.Add(userInfo);
            }

            return result;
        }

        private List<ChatContextViewModel> ConstructChatContexts(BiliMessageSessionResponse sessionResponse, List<UserInfo> UserList)
        {
            var chatContexts = m_mapper.Map<List<ChatContextViewModel>>(sessionResponse.SessionList);
            var chatContextsDict = chatContexts.ToDictionary(x => x.ChatContextId);

            foreach (var userInfo in UserList)
            {
                if (!chatContextsDict.TryGetValue(userInfo.Mid.ToString(), out var context)) continue;
                context.Cover = userInfo.Face;
                context.Title = userInfo.Name;
            }

            return chatContexts;
        }

        public async Task<List<ChatContextViewModel>> GetChatContexts(MessagesViewModel viewModel)
        {
            try
            {
                var api = m_messageApi.MessageSessions();
                var results = await api.Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);
                var data = await results.GetData<BiliMessageSessionResponse>();
                if (!data.success)
                    throw new CustomizedErrorException(data.message);
                var userIdList = data.data.SessionList
                    .Where(x => x.SessionType == 1 && x.SystemMsgType == 0)
                    .Select(x => x.TalkerId).ToList();
                var userList = await GetUserList(userIdList);
                var chatContexts = ConstructChatContexts(data.data, userList);
                viewModel.ChatContexts = new ObservableCollection<ChatContextViewModel>(chatContexts);
                viewModel.HasMore = data.data.HasMore == 1;
                return chatContexts;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return new List<ChatContextViewModel>();
            }
        }

        public async Task<List<ChatMessage>> GetChatMessages(ChatContextViewModel chatContext)
        {
            try
            {
                var api = m_messageApi.SessionMsgs(chatContext.ChatContextId, chatContext.Type);
                var results = await api.Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);
                var data = await results.GetData<BiliSessionMessagesResponse>();
                if (!data.success)
                    throw new CustomizedErrorException(data.message);

                var chatMessages = new List<ChatMessage>();

                foreach (var message in data.data.Messages)
                {
                    try
                    {
                        var chatMessage = m_mapper.Map<ChatMessage>(message);
                        if (chatMessage.UserId == chatContext.ChatContextId)
                        {
                            chatMessage.Face = chatContext.Cover;
                            chatMessage.UserName = chatContext.Title;
                        }

                        chatMessages.Insert(0, chatMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                }

                return chatMessages;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return new List<ChatMessage>();
            }
        }
    }
}
