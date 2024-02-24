using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.ViewModels.Home
{
    public class HomeViewModel : BaseViewModel
    {
        #region Fields

        private readonly IMapper m_mapper;
        private readonly Account m_account;

        #endregion

        #region Constructors

        public HomeViewModel()
        {
            m_account = new Account();
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            var homeNavItemList = SettingService.GetValue(SettingConstants.UI.HOEM_ORDER, DefaultHomeNavItems.GetDefaultHomeNavItems());
            HomeNavItems = m_mapper.Map<ObservableCollection<HomeNavItemViewModel>>(homeNavItemList);
            SelectItem = HomeNavItems.FirstOrDefault();
            if (!SettingService.Account.Logined) return;
            IsLogin = true;
            foreach (var item in HomeNavItems)
            {
                if (!item.Show && item.NeedLogin) item.Show = true;
            }
        }

        #endregion

        #region Properties

        public ObservableCollection<HomeNavItemViewModel> HomeNavItems { get; set; }

        public HomeNavItemViewModel SelectItem { get; set; }

        public bool IsLogin { get; set; }

        public HomeUserCardModel Profile { get; set; }

        public ObservableCollection<string> SuggestSearchContents { get; set; }

        #endregion

        #region Public Methods

        public async Task LoginUserCard()
        {
            var data = await m_account.GetHomeUserCard();
            if (data != null)
            {
                Profile = data;
                return;
            }
            //检查Token
        }

        #endregion
    }
}
