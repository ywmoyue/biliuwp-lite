using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Flurl.Http;
using PropertyChanged;

namespace BiliLite.ViewModels.Settings
{
    public class PlaySettingsControlViewModel : BaseViewModel
    {
        public PlaySettingsControlViewModel()
        {
            CDNServers = new List<CDNServerItemViewModel>()
            {
                new CDNServerItemViewModel("upos-sz-mirrorhwo1.bilivideo.com","华为云"),
                new CDNServerItemViewModel("upos-sz-mirrorcos.bilivideo.com","腾讯云"),
                new CDNServerItemViewModel("upos-sz-mirrorali.bilivideo.com","阿里云"),
                new CDNServerItemViewModel("upos-sz-mirrorhw.bilivideo.com","华为云"),
                new CDNServerItemViewModel("upos-sz-mirrorks3.bilivideo.com","金山云"),
                new CDNServerItemViewModel("upos-tf-all-js.bilivideo.com","JS"),
                new CDNServerItemViewModel("cn-hk-eq-bcache-01.bilivideo.com","香港"),
                new CDNServerItemViewModel("cn-hk-eq-bcache-16.bilivideo.com","香港"),
                new CDNServerItemViewModel("upos-hz-mirrorakam.akamaized.net","Akamaized"),
                new CDNServerItemViewModel("","自定义服务器"),
            };
            FFmpegInteropXOptions = SettingService
                .GetValue(SettingConstants.Player.FFMPEG_INTEROP_X_OPTIONS, "");
        }

        public List<CDNServerItemViewModel> CDNServers { get; set; }

        public string FFmpegInteropXOptions { get; set; }

        public bool ShowCustomCDNTimeOut => CustomCDNDelay < 0;

        public bool ShowCustomCDNDelay => CustomCDNDelay > 0;

        [AlsoNotifyFor(nameof(ShowCustomCDNDelay),nameof(ShowCustomCDNTimeOut))]
        public long CustomCDNDelay { get; set; }

        /// <summary>
        /// CDN延迟测试
        /// </summary>
        public async void CDNServerDelayTest()
        {
            foreach (var item in CDNServers)
            {
                if (item.Server == "") continue;
                var time = await GetDelay(item.Server);
                item.Delay = time;
            }
        }

        /// <summary>
        /// CDN延迟测试
        /// </summary>
        public async void CustomCDNServerDelayTest(string url)
        {
            var time = await GetDelay(url);
            CustomCDNDelay = time;
        }

        private async Task<long> GetDelay(string server)
        {
            //随便整个链接
            var testUrl = $"https://{server}/upgcxcode/76/62/729206276/729206276_nb2-1-30112.m4s";

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                var res = await testUrl.WithHeader(
                        "user-agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36")
                    .WithTimeout(2).GetAsync();
            }
            catch (FlurlHttpException ex)
            {
                if (ex.StatusCode != 959 && ex.StatusCode != 403)
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                return -1;
            }

            sw.Stop();
            return sw.ElapsedMilliseconds;
        }

        // UWP不支持ping，后续更新WindowsAppSDK再考虑启用
        //private async Task<long> GetPing(string address){}
    }
}
