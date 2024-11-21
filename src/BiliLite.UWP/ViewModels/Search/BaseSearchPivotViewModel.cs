using System;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Search
{
    public class BaseSearchPivotViewModel : BaseViewModel, ISearchPivotViewModel
    {
        #region Fields

        protected SearchAPI SearchApi;

        #endregion

        #region Constructors

        public BaseSearchPivotViewModel()
        {
            SearchApi = new SearchAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        public ICommand LoadMoreCommand { get; private set; }

        public ICommand RefreshCommand { get; private set; }

        [DoNotNotify]
        public SearchType SearchType { get; set; }

        [DoNotNotify]
        public string Title { get; set; }

        [DoNotNotify]
        public string Keyword { get; set; }

        [DoNotNotify]
        public string Area { get; set; } = "";

        [DoNotNotify]
        public int Page { get; set; } = 1;

        public bool Loading { get; set; }

        public bool Nothing { get; set; }

        public bool ShowLoadMore { get; set; }

        public bool HasData { get; set; } = false;

        #endregion

        #region Public Methods

        public virtual async void Refresh()
        {
            HasData = false;
            Page = 1;
            await LoadData();
        }
        public virtual async void LoadMore()
        {
            await LoadData();
        }

        public virtual Task LoadData()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
