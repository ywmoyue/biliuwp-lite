using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Models.Download;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Download;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Controls.Dialogs
{
    public sealed partial class DownloadDialog : ContentDialog
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();
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
            var settingDownloadQuality = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_QUALITY, -1);
            var settingDownloadSoundQuality = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_SOUND_QUALITY, -1);

            DownloadEpisodeItemViewModel episode;
            if (m_viewModel.OnlyLoadCurrentVideoQuality)
            {
                episode = downloadItem.Episodes.FirstOrDefault(x => x.AVID == downloadItem.ID);
            }
            else
            {
                episode = downloadItem.Episodes.OrderByDescending(x => x.Index).FirstOrDefault(x => !x.IsPreview);
                if (episode == null)
                {
                    episode = downloadItem.Episodes.OrderByDescending(x => x.Index).FirstOrDefault();
                }
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
                NotificationShowExtensions.ShowMessageToast("读取可下载清晰度时失败：" + data.Message);
                return;
            }
            if (data.Qualites == null || data.Qualites.Count < 1)
            {
                return;
            }
            m_viewModel.Qualities = data.Qualites;

            var quality = m_viewModel.Qualities.FirstOrDefault(x => x.QualityID == settingDownloadQuality);
            if (quality != null)
                m_viewModel.SelectedQualityIndex = m_viewModel.Qualities.IndexOf(quality);
            else m_viewModel.SelectedQualityIndex = m_viewModel.Qualities.Count - 1;

            m_viewModel.AudioQualities = [.. data.AudioQualites.OrderBy(x => x.QualityID)];
            var audioQuality =
                m_viewModel.AudioQualities.FirstOrDefault(x => x.QualityID == settingDownloadSoundQuality);
            if (audioQuality != null)
                m_viewModel.SelectedAudioQuality = audioQuality;
            else m_viewModel.SelectedAudioQuality = m_viewModel.AudioQualities.LastOrDefault();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (listView.SelectedItems == null || listView.SelectedItems.Count == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请选择要下载的剧集");
                args.Cancel = true;
                return;
            }

            var quality = m_viewModel.Qualities != null &&
                          m_viewModel.SelectedQualityIndex >= 0 &&
                          m_viewModel.SelectedQualityIndex < m_viewModel.Qualities.Count
                ? m_viewModel.Qualities[m_viewModel.SelectedQualityIndex]
                : null;
            if (quality == null)
            {
                NotificationShowExtensions.ShowMessageToast("还没有读取到可下载的清晰度，请稍后再试");
                args.Cancel = true;
                return;
            }

            var selectedItems = listView.SelectedItems.Cast<DownloadEpisodeItemViewModel>().ToList();

            // 对话框马上就会被关闭，这里先把处理要用到的设置快照下来
            var qualityId = quality.QualityID;
            var soundQualityId = m_viewModel.SelectedAudioQuality?.QualityID ?? 0;
            var codecMode = m_viewModel.SelectedVideoType?.Value ?? PlayUrlCodecMode.DASH_H264;

            SettingService.SetValue(SettingConstants.Download.DOWNLOAD_QUALITY, qualityId);
            SettingService.SetValue(SettingConstants.Download.DOWNLOAD_SOUND_QUALITY, soundQualityId);

            // 关闭对话框，处理过程交给右下角的进度悬浮窗
            args.Cancel = false;
            _ = AddDownloadsAsync(selectedItems, qualityId, soundQualityId, codecMode);
        }

        /// <summary>
        /// 逐集读取播放地址并加入下载列表，处理进度显示在右下角悬浮窗里
        /// </summary>
        private async Task AddDownloadsAsync(List<DownloadEpisodeItemViewModel> items, int qualityId,
            int soundQualityId, PlayUrlCodecMode codecMode)
        {
            var batch = DownloadAddToast.ShowBatch(
                string.IsNullOrWhiteSpace(downloadItem.Title) ? "下载任务" : downloadItem.Title, items);
            try
            {
                foreach (var item in items)
                {
                    if (item.State == 3)
                    {
                        await HandleDownloadedItem(item, qualityId, soundQualityId, codecMode);
                        batch.NotifyProgress();
                        continue;
                    }

                    if (item.State != 0 && item.State != 99)
                    {
                        continue;
                    }

                    try
                    {
                        item.State = 1;
                        batch.NotifyProgress();

                        var downloadInfo = CreateDownloadInfo(item);

                        // 读取视频信息
                        var info = await playerVM.GetPlayInfo(item.AVID, item.CID);
                        if (info.subtitle?.subtitles?.Count > 0)
                        {
                            downloadInfo.Subtitles = info.subtitle.subtitles.Select(sub => new DownloadSubtitleInfo
                            {
                                Name = $"{sub.lan_doc}({sub.lan})",
                                Url = sub.subtitle_url
                            }).ToList();
                        }

                        var playUrl = await GetPlayUrl(item, qualityId, soundQualityId);
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
                        item.State = 99;
                        item.ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        batch.NotifyProgress();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log("添加下载任务失败", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("添加下载任务时出现错误，已记录");
            }
            finally
            {
                m_downloadService.LoadDownloading();
                DownloadAddToast.FinishBatch(batch);
            }
        }

        private async Task HandleDownloadedItem(DownloadEpisodeItemViewModel item, int qualityId,
            int soundQualityId, PlayUrlCodecMode codecMode)
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
                .Any(x => x.CodecId == codecMode.CodecModeToCodecId() && x.QualityId == qualityId);

            var needDownloadAudioTrack = downloadSubItem.GetAudioDownloadTrackInfoList()
                .All(x => x.QualityId != soundQualityId);

            if (!needDownloadVideoTrack && !needDownloadAudioTrack) return;

            var downloadInfo = CreateDownloadInfo(item);
            downloadInfo.AddOthersTrack = true;

            var playUrl = await GetPlayUrl(item, qualityId, soundQualityId);
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

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrl(DownloadEpisodeItemViewModel item, int qualityId,
            int soundQualityId)
        {
            return await playerVM.GetPlayUrls(new PlayInfo
            {
                avid = item.AVID,
                cid = item.CID,
                ep_id = item.EpisodeID,
                play_mode = downloadItem.Type == DownloadType.Season ? VideoPlayType.Season : VideoPlayType.Video,
                season_id = downloadItem.SeasonID,
                season_type = downloadItem.SeasonType,
                area = downloadItem.Title.ParseArea(downloadItem.UpMid)
            }, qn: qualityId, soundQualityId);
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

        private async void SwOnlyCurrentVideoQuality_OnToggled(object sender, RoutedEventArgs e)
        {
            // 等待viewModel数据变更
            await Task.Delay(100);
            LoadQuality();
        }
    }
}