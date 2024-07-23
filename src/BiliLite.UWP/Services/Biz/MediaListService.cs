using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Requests.Api;
using BiliLite.ViewModels.Video;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Services.Biz
{
    public class MediaListService
    {
        private readonly IMapper m_mapper;

        public MediaListService(IMapper mapper)
        {
            m_mapper = mapper;
        }

        public async Task LoadMoreMediaList(VideoListSectionViewModel videoListSectionViewModel)
        {
            var api = new VideoAPI().GetMediaList(videoListSectionViewModel.OnlineListId, videoListSectionViewModel.Items.Last().Id);
            var results = await api.Request();
            var data = await results.GetData<MediaListResources>();
            var mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            var moreItems = mapper.Map<List<VideoListItem>>(data.data.MediaList);
            videoListSectionViewModel.Items.AddRange(moreItems);
        }

        public async Task<List<MediaListItem>> GetMediaList(string mediaListId)
        {
            var api = new VideoAPI().GetMediaList(mediaListId, "");
            var results = await api.Request();
            var data = await results.GetData<MediaListResources>();
            return data.data.MediaList;
        }
    }
}
