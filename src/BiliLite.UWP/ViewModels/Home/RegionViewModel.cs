using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Home
{
    [RegisterTransientViewModel]
    public class RegionViewModel : BaseViewModel
    {
        #region Fields

        private readonly RegionAPI m_regionApi;

        #endregion

        #region Constructors

        public RegionViewModel()
        {
            m_regionApi = new RegionAPI();
        }

        #endregion

        #region Properties

        public bool Loading { get; set; } = true;

        public List<RegionItem> Regions { get; set; }

        #endregion

        #region Public Methods

        public async Task GetRegions()
        {
            try
            {
                Loading = true;
                if (AppHelper.Regions == null || AppHelper.Regions.Count == 0)
                {
                    await AppHelper.SetRegions();
                }
                Regions = AppHelper.Regions;
            }
            catch (Exception ex)
            {
                Regions = await AppHelper.GetDefaultRegions();
                var handel = HandelError<RegionViewModel>(ex);
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
