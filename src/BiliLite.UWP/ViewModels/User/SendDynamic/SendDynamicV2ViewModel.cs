using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User.SendDynamic;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Models.Responses;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.UserDynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using RestSharp;

namespace BiliLite.ViewModels.User.SendDynamic
{
    public class SendDynamicV2ViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        readonly DynamicAPI m_dynamicApi;
        private List<AtDisplayModel> m_atDisplaylist = new List<AtDisplayModel>();
        private List<AtModel> m_atlist = new List<AtModel>();

        #endregion

        #region Constructors

        public SendDynamicV2ViewModel()
        {
            m_dynamicApi = new DynamicAPI();
            Images = new ObservableCollection<UploadImagesModel>();
        }

        #endregion

        #region Properties

        public bool IsRepost { get; set; }

        public DynamicV2ItemViewModel RepostInfo { get; set; }

        public ObservableCollection<UploadImagesModel> Images { get; set; }

        public bool Uploading { get; set; }

        public bool ShowImage { get; set; }

        public string Content { get; set; } = "";

        [DependsOn(nameof(Content))]
        public int Count => Content.Length;

        #endregion

        #region Public Methods

        public void SetRepost(DynamicV2ItemViewModel repostInfo)
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
                var api = m_dynamicApi.UploadImage();

                var fileStream = await file.OpenAsync(FileAccessMode.Read);
                var bytes = new byte[fileStream.Size];
                await fileStream.ReadAsync(bytes.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                var client = new RestClient(api.url);
                var request = new RestRequest();
                request.Method = Method.Post;
                request.AddParameter("biz", "draw");
                request.AddParameter("category", "daily");
                request.AddFile("file_up", bytes, file.Name);
                var response = await client.ExecuteAsync(request);
                var content = response.Content;

                var result = JsonConvert.DeserializeObject<ApiDataModel<UploadImagesModel>>(content);
                if (result.code == 0)
                {
                    result.data.ImageSize = (await file.GetBasicPropertiesAsync()).Size / 1024;
                    Images.Add(result.data);
                }
                else
                {
                    Notify.ShowMessageToast(result.message);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("图片上传失败", LogType.Fatal, ex);
                Notify.ShowMessageToast("图片上传失败");
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
                var httpResults = await m_dynamicApi.RepostDynamic(RepostInfo.Extend.DynIdStr, Content, at_uids, ctrl)
                    .Request();
                if (!httpResults.status)
                    throw new CustomizedErrorException(httpResults.message);
                var data = await httpResults.GetData<JObject>();
                if (data.code != 0)
                    throw new CustomizedErrorException("发表动态失败:" + data.message);

                Notify.ShowMessageToast("转发成功");
                m_atDisplaylist.Clear();
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Log(ex.Message, LogType.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("转发动态失败" + ex.Message);
                _logger.Log("转发动态失败", LogType.Error, ex);
                return false;
            }
        }

        public async Task<bool> SendDynamic()
        {
            if (Content.Trim().Length == 0)
            {
                Notify.ShowMessageToast("不能发送空白动态");
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

                Notify.ShowMessageToast("发表动态成功");
                m_atDisplaylist.Clear();
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Log(ex.Message, LogType.Error, ex);
                return false;
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("发表动态发生错误");
                _logger.Log("发表动态失败", LogType.Error, ex);
                return false;
            }

        }

        #endregion
    }
}
