using System.Windows.Input;

namespace BiliLite.ViewModels.Region
{
    public interface IRegionViewModel
    {
        ICommand RefreshCommand { get; set; }

        ICommand LoadMoreCommand { get; set; }

        int ID { get; set; }

        string RegionName { get; set; }

        bool Loading { get; set; }
    }
}