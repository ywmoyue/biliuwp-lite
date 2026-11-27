using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Season
{
    [RegisterTransientViewModel]
    public class AnimeTimelineViewModel : BaseViewModel
    {
        #region Fields

        private readonly AnimeBizService m_animeService;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public AnimeTimelineViewModel()
        {
            m_animeService = App.ServiceProvider.GetRequiredService<AnimeBizService>();
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
                Timelines = await m_animeService.GetTimeline((int)AnimeType);
                Today = Timelines.FirstOrDefault(x => x.IsToday);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<AnimeTimelineViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
