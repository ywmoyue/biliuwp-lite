using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Bilibili.App.Dynamic.V2;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Msg;
using BiliLite.Models.Common.Msg.MsgContent;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.ViewModels.Messages;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace BiliLite.Services.Biz
{
    [RegisterTransientService]
    public class MessagesService : BaseBizService
    {
        private readonly MessageApi m_messageApi;
        private readonly CommentApi m_commentApi;
        private readonly IMapper m_mapper;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private string m_selfFace;
        private readonly string m_selfDevId;
        private long m_selfId;

        public MessagesService(IMapper mapper)
        {
            m_mapper = mapper;
            m_selfDevId =
                SettingService.GetValue(SettingConstants.Account.MESSAGE_DEVICE_ID, Guid.NewGuid().ToString());
            m_messageApi = new MessageApi();
            m_commentApi = new CommentApi();
        }

        private async Task GetSelfInfo()
        {
            m_selfId = SettingService.Account.UserID;
            var api = m_messageApi.UserList([m_selfId]);
            var results = await api.Request();
            if (!results.status)
                throw new CustomizedErrorException(results.message);
            var data = await results.GetData<JObject>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            var userInfo =
                JsonConvert.DeserializeObject<UserInfo>(JsonConvert.SerializeObject(data.data[$"{m_selfId}"]));
            m_selfFace = userInfo.Face;
        }

        private async Task<List<UserInfo>> GetUserList(List<long> userIdList)
        {
            if (string.IsNullOrEmpty(m_selfFace))
            {
                await GetSelfInfo();
            }
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

        public async Task<List<ChatContextViewModel>> GetChatContexts(MessagesViewModel viewModel,
            bool loadMore = false)
        {
            viewModel.ChatContextLoading = true;
            viewModel.HasMoreContexts = false;
            try
            {
                var api = m_messageApi.MessageSessions();
                if (loadMore && viewModel.ChatContexts.Any())
                {
                    api = m_messageApi.MessageSessions(endTime: viewModel.ChatContexts.Last().Time);
                }
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
                if (loadMore && viewModel.ChatContexts.Any())
                {
                    viewModel.ChatContexts.AddRange(chatContexts);
                }
                else
                {
                    viewModel.ChatContexts = new ObservableCollection<ChatContextViewModel>(chatContexts);
                }
                viewModel.HasMoreContexts = data.data.HasMore == 1;
                return chatContexts;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return new List<ChatContextViewModel>();
            }
            finally
            {
                viewModel.ChatContextLoading = false;
            }
        }

        public async Task<List<ChatMessage>> GetChatMessages(MessagesViewModel viewModel, ChatContextViewModel chatContext,
            bool loadMore = false)
        {
            viewModel.ChatMessagesLoading = true;
            viewModel.HasMoreMessages = false;
            try
            {
                var api = m_messageApi.SessionMsgs(chatContext.ChatContextId, chatContext.Type);

                if (loadMore && viewModel.ChatMessages.Any())
                {
                    api = m_messageApi.SessionMsgs(chatContext.ChatContextId, chatContext.Type, beginSeqno: "0",
                        endSeqno: viewModel.LastMsgId);
                }

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

                        if (chatMessage.UserId == SettingService.Account.UserID.ToString())
                        {
                            chatMessage.IsSelf = true;
                            chatMessage.Face = m_selfFace;
                            chatMessage.RevokeCommand = new RelayCommand<ChatMessage>(CommandRevoke);
                        }

                        chatMessage.ImageCommand = new RelayCommand<object>(OpenImage);
                        chatMessage.NotificationActionCommand = new RelayCommand<object>(NotificationMsgDoAction);

                        chatMessage.UpdateContent();

                        chatMessages.Insert(0, chatMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message, ex);
                    }
                }

                if (loadMore && viewModel.ChatContexts.Any())
                {
                    // 反转 chatMessages 列表以保持正确的顺序
                    var reversedChatMessages = chatMessages.AsEnumerable().Reverse();

                    // 将反转后的消息插入到 viewModel.ChatMessages 列表的前面
                    foreach (var chatMessage in reversedChatMessages)
                    {
                        viewModel.ChatMessages.Insert(0, chatMessage);
                    }
                }
                else
                {
                    viewModel.ChatMessages = new ObservableCollection<ChatMessage>(chatMessages);
                }

                viewModel.LastMsgId = data.data.MinSeqno.ToString();
                if(!loadMore)
                    viewModel.NewMsgId = data.data.MaxSeqno.ToString();
                viewModel.HasMoreMessages = data.data.HasMore;

                return chatMessages;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return new List<ChatMessage>();
            }
            finally
            {
                viewModel.ChatMessagesLoading = false;
            }
        }

        public async Task SetReaded(MessagesViewModel viewModel, ChatContextViewModel chatContext)
        {
            try
            {
                var api = m_messageApi.UpdateAck(chatContext.ChatContextId, chatContext.Type, viewModel.NewMsgId);
                var results = await api.Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);
                chatContext.UnreadMsgCount = 0;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        public async Task<ChatMessage> SendTextMsg(MessagesViewModel viewModel, ChatContextViewModel chatContext, string text)
        {
            var message = new TextChatMessageContent()
            {
                Content = text,
            };
            var chatMessage = await SendMessageAsync(viewModel, chatContext, message, 1);
            viewModel.ChatMessageInput = "";
            return chatMessage;
        }

        public async Task<ChatMessage> SendImageMsg(MessagesViewModel viewModel, ChatContextViewModel chatContext, UploadFileInfo fileInfo)
        {
            var imageResult = await UploadImage(fileInfo);

            var message = new ImageChatMessageContent()
            {
                Url = imageResult.ImageUrl,
                Height = imageResult.ImageHeight,
                Width = imageResult.ImageWidth,
                Size = imageResult.ImgSize,
                Original = 1,
                ImageType = fileInfo.FileName.GetImageTypeFromFileName(),
            };
            var chatMessage = await SendMessageAsync(viewModel, chatContext, message, 2);
            chatMessage.ImageCommand = new RelayCommand<object>(OpenImage);
            return chatMessage;
        }

        public async Task<ChatMessage> RevokeMsg(MessagesViewModel viewModel, ChatContextViewModel chatContext, string chatMessageId)
        {
            var message = chatMessageId;
            return await SendMessageAsync(viewModel, chatContext, message, 5);
        }

        private void OpenImage(object data)
        {
            if (!(data is ImageChatMessageContent imageContent))
            {
                return;
            }

            MessageCenter.OpenImageViewer(new List<string>() { imageContent.Url }, 0);
        }

        private async void NotificationMsgDoAction(object data)
        {
            if (data is not string url)
            {
                return;
            }
            var result = await MessageCenter.HandelUrl(url);
            if (!result)
            {
                Notify.ShowMessageToast("无法打开Url");
            }
        }

        private async void CommandRevoke(ChatMessage chatMessage)
        {
            if (DateTimeOffset.Now - chatMessage.Time > TimeSpan.FromMinutes(3))
            {
                Notify.ShowMessageToast("发送时间超过两分钟不能撤回");
                return;
            }
            var viewModel = App.ServiceProvider.GetRequiredService<MessagesViewModel>();
            await RevokeMsg(viewModel, viewModel.SelectedChatContext, chatMessage.ChatMessageId);
        }

        private async Task<ChatMessage> SendMessageAsync(MessagesViewModel viewModel, ChatContextViewModel chatContext, object messageContent, int messageType)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                var content = "";
                if (messageType == 5)
                {
                    content = (string)messageContent;
                }
                else
                {
                    content = JsonConvert.SerializeObject(messageContent, settings);
                }
                
                var api = m_messageApi.SendMsg(m_selfId.ToString(), chatContext.ChatContextId, chatContext.Type, messageType, content, m_selfDevId);
                var results = await api.Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);

                var data = await results.GetData<JObject>();
                var codeDic = new BiliSendMessageResultCode();
                var msg = codeDic[data.code];
                Notify.ShowMessageToast($"发送消息： {msg}");

                if (!data.success)
                {
                    _logger.Error($"发送消息({data.code})： {msg}");
                    return null;
                }

                var chatMessage = new ChatMessage
                {
                    UserId = m_selfId.ToString(),
                    Face = m_selfFace,
                    IsSelf = true,
                    Time = DateTimeOffset.Now,
                    MsgType = (ChatMsgType)messageType,
                    ChatMessageId = data.data["msg_key"].ToString(),
                    RevokeCommand = new RelayCommand<ChatMessage>(CommandRevoke),
                };
                chatMessage.ContentStr = messageType == 1 ? data.data["msg_content"].ToString() : content;
                chatMessage.UpdateContent();

                viewModel.ChatMessages.Add(chatMessage);

                return chatMessage;
            }
            catch (Exception ex)
            {
                HandleError(ex);
                return null;
            }
        }

        private async Task<DynamicPicture> UploadImage(UploadFileInfo fileInfo)
        {
            var api = m_commentApi.UploadDraw(fileInfo, "im");
            var result = await api.Request();
            if (!result.status)
                throw new CustomizedErrorException(result.message);
            var uploadDrawResult = await result.GetData<DynamicPicture>();
            if (!uploadDrawResult.success)
                throw new CustomizedErrorException(uploadDrawResult.message);
            return uploadDrawResult.data;
        }
    }
}
