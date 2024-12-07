using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Other;
using BiliLite.Models.Requests.Api;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Other
{
    [RegisterTransientViewModel]
    public class FindMoreViewModel : BaseViewModel
    {
        private readonly GitApi m_gitApi;

        public FindMoreViewModel()
        {
            m_gitApi = new GitApi();
        }

        public bool Loading { get; set; } = true;

        public List<FindMoreEntranceModel> Items { get; set; }

        public async void LoadEntrance()
        {
            try
            {
                Loading = true;
                var results = await m_gitApi.FindMoreEntrance().Request();
                if (results.status)
                {
                    var data = await results.GetJson<List<FindMoreEntranceModel>>();
                    await Task.Delay(2000);
                    Items = data;
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<FindMoreViewModel> (ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
