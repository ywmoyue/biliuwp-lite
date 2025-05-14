using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Models.Responses;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.UserDynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;

namespace BiliLite.ViewModels.User.SendDynamic
{
    public class SendDynamicViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        readonly DynamicAPI m_dynamicApi;
        private List<AtDisplayModel> m_atDisplaylist = new List<AtDisplayModel>();
        private List<AtModel> m_atlist = new List<AtModel>();

        #endregion

        #region Constructors

        public SendDynamicViewModel()
        {
            m_dynamicApi = new DynamicAPI();
            Images = new ObservableCollection<UploadImagesModel>();
        }

        #endregion

        #region Properties

        public bool IsRepost { get; set; }

        public UserDynamicItemDisplayViewModel RepostInfo { get; set; }

        public ObservableCollection<UploadImagesModel> Images { get; set; }

        public bool Uploading { get; set; }

        public bool ShowImage { get; set; }

        public string Content { get; set; } = "";

        [DependsOn(nameof(Content))]
        public int Count => Content.Length;

        #endregion

        #region Public Methods

        public void SetRepost(UserDynamicItemDisplayViewModel repostInfo)
        {
            RepostInfo = repostInfo;
            IsRepost = true;
        }

        public void AddAtItem(AtDisplayModel atDisplayModel)
        {
            m_atDisplaylist.Add(atDisplayModel);
        }

        public async void UploadImage(StorageFile file)
        {
            try
            {
                Uploading = true;
                var fileStream = await file.OpenAsync(FileAccessMode.Read);
                var bytes = new byte[fileStream.Size];
                await fileStream.ReadAsync(bytes.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                var fileInfo = new UploadFileInfo()
                {
                    Data = bytes,
                    FileName = file.Name,
                };
                var api = new CommentApi().UploadDraw(fileInfo);
                var result = await api.Request();
                if (!result.status)
                    throw new CustomizedErrorException(result.message);
                var uploadDrawResult = await result.GetData<DynamicPicture>();
                if (!uploadDrawResult.success)
                    throw new CustomizedErrorException(uploadDrawResult.message);
            }
            catch (Exception ex)
            {
                _logger.Log("图片上传失败", LogType.Fatal, ex);
                NotificationShowExtensions.ShowMessageToast("图片上传失败");
            }
            finally
            {
                Uploading = false;
                ShowImage = Images.Count > 0;
            }
        }

        /// <summary>
        /// 转发
        /// </summary>
        public async Task<bool> SendRepost()
        {
            var ctrl = "[]";
            var at_uids = "";
            m_atlist.Clear();

            if (m_atDisplaylist.Count != 0)
            {

                foreach (var item in m_atDisplaylist.Where(item => Content.Contains(item.Text)))
                {
                    m_atlist.Add(new AtModel()
                    {
                        Data = item.Data.ToString(),
                        Length = item.Length - 2,
                        Location = Content.IndexOf(item.Text),
                        Type = 1
                    });
                    var d = item.Text.Replace("[", "").Replace("]", "");
                    Content = Content.Replace(item.Text, d);
                    at_uids += item.Data.ToString() + ",";
                }
                ctrl = JsonConvert.SerializeObject(m_atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);
                m_atDisplaylist.Clear();
            }
            if (Content == "")
            {
                Content = "转发动态";
            }

            try
            {
                var httpResults = await m_dynamicApi.RepostDynamic(RepostInfo.DynamicID, Content, at_uids, ctrl)
                    .Request();
                if (!httpResults.status)
                    throw new CustomizedErrorException(httpResults.message);
                var data = await httpResults.GetData<JObject>();
                if (data.code != 0)
                    throw new CustomizedErrorException("发表动态失败:" + data.message);

                NotificationShowExtensions.ShowMessageToast("转发成功");
                m_atDisplaylist.Clear();
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Log(ex.Message, LogType.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("转发动态失败" + ex.Message);
                _logger.Log("转发动态失败", LogType.Error, ex);
                return false;
            }
        }

        public async Task<bool> SendDynamic()
        {
            if (Content.Trim().Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("不能发送空白动态");
                return false;
            }

            var ctrl = "[]";
            var at_uids = "";
            m_atlist.Clear();
            var txt = Content;
            if (m_atDisplaylist.Count != 0)
            {
                foreach (var item in m_atDisplaylist.Where(item => txt.Contains(item.Text)))
                {
                    m_atlist.Add(new AtModel()
                    {
                        Data = item.Data.ToString(),
                        Length = item.Length - 2,
                        Location = txt.IndexOf(item.Text),
                        Type = 1
                    });
                    var d = item.Text.Replace("[", "").Replace("]", "");
                    txt = txt.Replace(item.Text, d);
                    at_uids += item.Data.ToString() + ",";
                }
                ctrl = JsonConvert.SerializeObject(m_atlist);
                at_uids = at_uids.Remove(at_uids.Length - 1, 1);

            }

            var sendPics = Images.Select(item => new SendImagesModel() { ImgHeight = item.ImageHeight, ImgSize = item.ImageSize, ImgSrc = item.ImageUrl, ImgWidth = item.ImageWidth }).ToList();
            var imgStr = JsonConvert.SerializeObject(sendPics);
            try
            {
                HttpResults httpResults;
                if (sendPics.Count == 0)
                {
                    httpResults = await m_dynamicApi.CreateDynamicText(txt, at_uids, ctrl).Request();
                }
                else
                {
                    httpResults = await m_dynamicApi.CreateDynamicPhoto(imgStr, txt, at_uids, ctrl).Request();
                }

                if (!httpResults.status)
                    throw new CustomizedErrorException(httpResults.message);

                var data = await httpResults.GetData<JObject>();
                if (data.code != 0)
                    throw new CustomizedErrorException("发表动态失败:" + data.message);

                NotificationShowExtensions.ShowMessageToast("发表动态成功");
                m_atDisplaylist.Clear();
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Log(ex.Message, LogType.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("发表动态发生错误");
                _logger.Log("发表动态失败", LogType.Error, ex);
                return false;
            }

        }

        #endregion
    }
}
