using BiliLite.Modules;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Download;
using BiliLite.Extensions;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.ViewModels.Download;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class DownloadDialog : ContentDialog
    {
        PlayerVM playerVM;
        DownloadItemViewModel downloadItem;
        List<DownloadEpisodeItemViewModel> allEpisodes;
        private readonly DownloadDialogViewModel m_viewModel;
        private readonly DownloadService m_downloadService;
        private readonly IMapper m_mapper;

        public DownloadDialog(DownloadItem downloadItem)
        {
            this.InitializeComponent(); 
            playerVM = new PlayerVM(true);

            m_viewModel = App.ServiceProvider.GetService<DownloadDialogViewModel>();
            m_downloadService = App.ServiceProvider.GetService<DownloadService>();
            m_mapper = App.ServiceProvider.GetService<IMapper>();

            var downloadItemViewModel = m_mapper.Map<DownloadItemViewModel>(downloadItem);

            allEpisodes = downloadItemViewModel.Episodes;
            if (downloadItem.Type == DownloadType.Season)
            {
                checkHidePreview.Visibility = Visibility.Visible;
            }

            this.downloadItem = downloadItemViewModel;

            var selectedValue = (PlayUrlCodecMode)SettingService.GetValue(SettingConstants.Download.DEFAULT_VIDEO_TYPE, (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
            m_viewModel.SelectedVideoType = DefaultVideoTypeOptions.GetOption(selectedValue);
            cbVideoType.Loaded += (sender, e) =>
            {
                cbVideoType.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Download.DEFAULT_VIDEO_TYPE, (int)cbVideoType.SelectedValue);
                    LoadQuality();
                };
            };

            LoadQuality();

        }
        private async void LoadQuality()
        {
            var episode = downloadItem.Episodes.OrderByDescending(x => x.Index).FirstOrDefault(x => !x.IsPreview);
            if (episode == null)
            {
                episode = downloadItem.Episodes.OrderByDescending(x => x.Index).FirstOrDefault();
            }
            var data = await playerVM.GetPlayUrls(new PlayInfo()
            {
                avid = episode.AVID,
                cid = episode.CID,
                ep_id = episode.EpisodeID,
                play_mode = downloadItem.Type == DownloadType.Season ? VideoPlayType.Season : VideoPlayType.Video,
                season_id = downloadItem.SeasonID,
                season_type = downloadItem.SeasonType,
                area = downloadItem.Title.ParseArea(downloadItem.UpMid)
            }, 0);
            if (!data.Success)
            {
                Notify.ShowMessageToast("读取可下载清晰度时失败：" + data.Message);
                return;
            }
            if (data.Qualites == null || data.Qualites.Count < 1)
            {
                return;
            }
            m_viewModel.Qualities = data.Qualites;
            m_viewModel.SelectedQualityIndex = 0;

            m_viewModel.AudioQualities = data.AudioQualites;
            m_viewModel.SelectedAudioQuality = data.AudioQualites?.FirstOrDefault();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;

            if (listView.SelectedItems == null || listView.SelectedItems.Count == 0)
            {
                Notify.ShowMessageToast("请选择要下载的剧集");
                return;
            }

            IsPrimaryButtonEnabled = false;
            bool hide = true;

            foreach (DownloadEpisodeItemViewModel item in listView.SelectedItems)
            {
                if (item.State == 3)
                {
                    await HandleDownloadedItem(item);
                    continue;
                }

                if (item.State != 0 && item.State != 99)
                {
                    continue;
                }

                try
                {
                    item.State = 1;

                    var downloadInfo = CreateDownloadInfo(item);

                    // 读取视频信息
                    var info = await playerVM.GetPlayInfo(item.AVID, item.CID);
                    if (info.subtitle?.subtitles?.Count > 0)
                    {
                        downloadInfo.Subtitles = info.subtitle.subtitles.Select(sub => new DownloadSubtitleInfo
                        {
                            Name = sub.lan_doc,
                            Url = sub.subtitle_url
                        }).ToList();
                    }

                    var playUrl = await GetPlayUrl(item);
                    if (!playUrl.Success)
                    {
                        item.State = 99;
                        item.ErrorMessage = playUrl.Message;
                        continue;
                    }

                    downloadInfo.QualityID = playUrl.CurrentQuality.QualityID;
                    downloadInfo.QualityName = playUrl.CurrentQuality.QualityName;
                    downloadInfo.Urls = new List<DownloadUrlInfo>();

                    await AddDownloadUrls(downloadInfo, playUrl);

                    await DownloadHelper.AddDownload(downloadInfo);

                    item.State = 2;
                }
                catch (Exception ex)
                {
                    hide = false;
                    item.State = 99;
                    item.ErrorMessage = ex.Message;
                }
            }

            m_downloadService.LoadDownloading();
            IsPrimaryButtonEnabled = true;

            Notify.ShowMessageToast(hide ? "已添加至下载列表" : "有视频下载失败");

            if (hide)
            {
                this.Hide();
            }
        }

        private async Task HandleDownloadedItem(DownloadEpisodeItemViewModel item)
        {
            var queryId = item.CID;
            var isSeason = downloadItem.Type == DownloadType.Season;
            if (isSeason)
            {
                queryId = item.EpisodeID;
            }

            var downloadSubItem = m_downloadService.FindDownloadSubItemById(queryId, isSeason);
            if (downloadSubItem == null) return;

            var needDownloadVideoTrack = !downloadSubItem.GetVideoDownloadTrackInfoList()
                .Any(x => x.CodecId == m_viewModel.SelectedVideoType.Value.CodecModeToCodecId() &&
                          x.QualityId == m_viewModel.SelectedQuality.QualityID);

            var needDownloadAudioTrack = downloadSubItem.GetAudioDownloadTrackInfoList()
                .All(x => x.QualityId != m_viewModel.SelectedAudioQuality.QualityID);

            if (!needDownloadVideoTrack && !needDownloadAudioTrack) return;

            var downloadInfo = CreateDownloadInfo(item);
            downloadInfo.AddOthersTrack = true;

            var playUrl = await GetPlayUrl(item);
            if (!playUrl.Success)
            {
                item.State = 99;
                item.ErrorMessage = playUrl.Message;
                return;
            }

            downloadInfo.QualityID = playUrl.CurrentQuality.QualityID;
            downloadInfo.QualityName = playUrl.CurrentQuality.QualityName;
            downloadInfo.Urls = new List<DownloadUrlInfo>();

            var quality = playUrl.CurrentQuality;
            var audio = playUrl.CurrentAudioQuality.Audio;
            var video = playUrl.CurrentQuality.DashInfo.Video;

            if (needDownloadVideoTrack)
            {
                downloadInfo.Urls.Add(new DownloadUrlInfo
                {
                    FileName = $"video-{video.ID}-{video.CodecID}.m4s",
                    HttpHeader = quality.GetHttpHeader(),
                    Url = video.Url
                });
            }

            if (needDownloadAudioTrack && audio != null)
            {
                downloadInfo.Urls.Add(new DownloadUrlInfo
                {
                    FileName = $"audio-{audio.ID}-1.m4s",
                    HttpHeader = quality.GetHttpHeader(),
                    Url = audio.Url
                });
            }

            await DownloadHelper.AddDownload(downloadInfo);
        }

        private DownloadInfo CreateDownloadInfo(DownloadEpisodeItemViewModel item)
        {
            return new DownloadInfo
            {
                CoverUrl = downloadItem.Cover,
                DanmakuUrl = $"{ApiHelper.API_BASE_URL}/x/v1/dm/list.so?oid=" + item.CID,
                EpisodeID = item.EpisodeID,
                CID = item.CID,
                AVID = downloadItem.ID,
                SeasonID = downloadItem.SeasonID,
                SeasonType = downloadItem.SeasonType,
                Title = downloadItem.Title,
                EpisodeTitle = item.Title,
                Type = downloadItem.Type,
                Index = item.Index
            };
        }

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrl(DownloadEpisodeItemViewModel item)
        {
            var soundQualityId = m_viewModel.SelectedAudioQuality?.QualityID ?? 0;
            return await playerVM.GetPlayUrls(new PlayInfo
            {
                avid = item.AVID,
                cid = item.CID,
                ep_id = item.EpisodeID,
                play_mode = downloadItem.Type == DownloadType.Season ? VideoPlayType.Season : VideoPlayType.Video,
                season_id = downloadItem.SeasonID,
                season_type = downloadItem.SeasonType,
                area = downloadItem.Title.ParseArea(downloadItem.UpMid)
            }, qn: (cbQuality.SelectedItem as BiliPlayUrlInfo).QualityID, soundQualityId);
        }

        private async Task AddDownloadUrls(DownloadInfo downloadInfo, BiliPlayUrlQualitesInfo playUrl)
        {
            if (playUrl.CurrentQuality.PlayUrlType == BiliPlayUrlType.DASH)
            {
                var quality = playUrl.CurrentQuality;
                var audio = playUrl.CurrentAudioQuality.Audio;
                var video = playUrl.CurrentQuality.DashInfo.Video;

                if (audio != null)
                {
                    downloadInfo.Urls.Add(new DownloadUrlInfo
                    {
                        FileName = $"video-{video.ID}-{video.CodecID}.m4s",
                        HttpHeader = quality.GetHttpHeader(),
                        Url = video.Url
                    });

                    downloadInfo.Urls.Add(new DownloadUrlInfo
                    {
                        FileName = $"audio-{audio.ID}-1.m4s",
                        HttpHeader = quality.GetHttpHeader(),
                        Url = audio.Url
                    });
                }
                else
                {
                    downloadInfo.Urls.Add(new DownloadUrlInfo
                    {
                        FileName = "0.blv",
                        HttpHeader = quality.GetHttpHeader(),
                        Url = video.Url
                    });
                }
            }
            else if (playUrl.CurrentQuality.PlayUrlType == BiliPlayUrlType.MultiFLV)
            {
                int i = 0;
                foreach (var videoItem in playUrl.CurrentQuality.FlvInfo)
                {
                    downloadInfo.Urls.Add(new DownloadUrlInfo
                    {
                        FileName = $"{i}.blv",
                        HttpHeader = playUrl.CurrentQuality.GetHttpHeader(),
                        Url = videoItem.Url
                    });
                    i++;
                }
            }
            else if (playUrl.CurrentQuality.PlayUrlType == BiliPlayUrlType.SingleFLV)
            {
                downloadInfo.Urls.Add(new DownloadUrlInfo
                {
                    FileName = "0.blv",
                    HttpHeader = playUrl.CurrentQuality.GetHttpHeader(),
                    Url = playUrl.CurrentQuality.FlvInfo.First().Url
                });
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            m_downloadService.LoadDownloading();
            this.Hide();
        }

        private void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }

        private void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            listView.SelectedItems.Clear();
        }

        private void checkHidePreview_Checked(object sender, RoutedEventArgs e)
        {
            downloadItem.Episodes = allEpisodes.Where(x => !x.IsPreview).ToList();
        }

        private void checkHidePreview_Unchecked(object sender, RoutedEventArgs e)
        {
            downloadItem.Episodes = allEpisodes;
        }
    }
}
