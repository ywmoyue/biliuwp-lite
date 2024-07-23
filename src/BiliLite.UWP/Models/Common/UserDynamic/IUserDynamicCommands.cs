using System.Windows.Input;

namespace BiliLite.Models.Common.UserDynamic
{
    public interface IUserDynamicCommands
    {
        public ICommand LaunchUrlCommand { get; set; }

        public ICommand RepostCommand { get; set; }

        public ICommand LikeCommand { get; set; }

        public ICommand CommentCommand { get; set; }

        public ICommand UserCommand { get; set; }

        public ICommand LoadMoreCommand { get; }

        public ICommand WebDetailCommand { get; set; }

        public ICommand DetailCommand { get; set; }

        public ICommand ImageCommand { get; set; }

        public ICommand WatchLaterCommand { get; set; }

        public ICommand CopyDynCommand { get; set; }

        public ICommand TagCommand { get; set; }
    }
}
