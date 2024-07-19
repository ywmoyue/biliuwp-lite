using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Settings
{
    public class CDNServerItemViewModel : BaseViewModel
    {
        public CDNServerItemViewModel(string server, string remark)
        {
            this.Server = server;
            this.Remark = remark;
        }

        public string Server { get; set; }

        public string Remark { get; set; }

        public bool ShowDelay => Delay > 0;

        public bool ShowTimeOut => Delay < 0;

        [AlsoNotifyFor(nameof(ShowDelay),nameof(ShowTimeOut))]
        public long Delay { get; set; }
    }
}