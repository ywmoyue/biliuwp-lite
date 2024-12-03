using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.Models.Attributes;
using BiliLite.Services;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Region
{
    [RegisterTransientViewModel]
    public class RegionDetailViewModel : BaseViewModel
    {
        public RegionDetailViewModel()
        {

        }

        public ObservableCollection<IRegionViewModel> Regions { get; set; }

        public IRegionViewModel SelectRegion { get; set; }

        public void InitRegion(int id, int tid)
        {
            var ls = new ObservableCollection<IRegionViewModel>();
            var region = AppHelper.Regions.FirstOrDefault(x => x.Tid == id);
            ls.Add(new RegionDetailHomeViewModel(region));
            Regions = ls;
            foreach (var item in region.Children)
            {
                ls.Add(new RegionDetailChildViewModel(item));
            }
            if (tid == 0)
            {
                SelectRegion = Regions[0];
            }
            else
            {
                SelectRegion = Regions.FirstOrDefault(x => x.ID == tid);
            }
        }
    }
}
