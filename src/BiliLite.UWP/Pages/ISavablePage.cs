using System.Threading.Tasks;

namespace BiliLite.Pages
{
    public interface ISavablePage
    {
        public Task Save();
    }
}
