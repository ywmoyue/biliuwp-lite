using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using BiliLite.ViewModels.Common;
using Flurl.Http;

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
            };
        }

        public List<CDNServerItemViewModel> CDNServers { get; set; }

        /// <summary>
        /// CDN延迟测试
        /// </summary>
        public async void CDNServerDelayTest()
        {
            foreach (var item in CDNServers)
            {
                var time = await GetDelay(item.Server);
                item.Delay = time;
            }
        }

        private async Task<long> GetDelay(string server)
        {
            //随便整个链接
            var testUrl = $"https://{server}/upgcxcode/76/62/729206276/729206276_nb2-1-30112.m4s";

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var res = await testUrl.WithTimeout(2).GetAsync();
                sw.Stop();
                return sw.ElapsedMilliseconds;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // UWP不支持ping，后续更新WindowsAppSDK再考虑启用
        //private async Task<long> GetPing(string address){}
    }
}
