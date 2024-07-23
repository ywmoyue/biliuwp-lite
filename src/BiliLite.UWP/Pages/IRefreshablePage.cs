using System.Threading.Tasks;

namespace BiliLite.Pages
{
    public interface IRefreshablePage
    {
        public Task Refresh();
    }
}
