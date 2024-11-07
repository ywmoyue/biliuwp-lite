using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Models.Common;

namespace BiliLite.ViewModels.Search
{
    public interface ISearchPivotViewModel
    {
        public ICommand LoadMoreCommand { get; }

        public ICommand RefreshCommand { get; }

        public SearchType SearchType { get; set; }

        public string Title { get; set; }

        public string Keyword { get; set; }

        public string Area { get; set; }

        public int Page { get; set; }

        public bool Loading { get; set; }

        public bool Nothing { get; set; }

        public bool ShowLoadMore { get; set; }

        public bool HasData { get; set; }

        public void Refresh();

        public Task LoadData();
    }
}
