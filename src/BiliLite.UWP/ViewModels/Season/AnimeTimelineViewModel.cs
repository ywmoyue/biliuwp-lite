using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Season
{
    [RegisterTransientViewModel]
    public class AnimeTimelineViewModel : BaseViewModel
    {
        #region Fields

        private readonly AnimeAPI m_animeApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public AnimeTimelineViewModel()
        {
            m_animeApi = new AnimeAPI();
            AnimeTypeItems = new List<AnimeTypeItem>()
            {
                new AnimeTypeItem()
                {
                    Name="番剧",
                    AnimeType= AnimeType.Bangumi
                },
                new AnimeTypeItem()
                {
                    Name="国创",
                    AnimeType= AnimeType.GuoChuang
                }
            };
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public AnimeType AnimeType { get; set; }

        public bool Loading { get; set; } = true;

        public List<AnimeTypeItem> AnimeTypeItems { get; set; }

        public AnimeTypeItem SelectAnimeType { get; set; }

        public AnimeTimelineModel Today { get; set; }

        public List<AnimeTimelineModel> Timelines { get; set; }

        #endregion

        #region Public Methods

        public void Init(AnimeType type)
        {
            SelectAnimeType = AnimeTypeItems.FirstOrDefault(x => x.AnimeType == type);
            AnimeType = type;
        }

        public async Task GetTimeline()
        {
            try
            {
                Loading = true;
                var api = m_animeApi.Timeline((int)AnimeType);

                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<List<AnimeTimelineModel>>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                Timelines = data.data;
                Today = data.data.FirstOrDefault(x => x.IsToday);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeTimelineViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
