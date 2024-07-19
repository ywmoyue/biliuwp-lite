using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Comment
{
    public class SendCommentDialogViewModel : BaseViewModel
    {
        public ObservableCollection<DynamicPicture> Pictures { get; set; } = new ObservableCollection<DynamicPicture>();

        public bool ShowPictures { get; set; } = false;

        public void AddPicture(DynamicPicture picture)
        {
            Pictures.Add(picture);
            ShowPictures = true;
        }

        public void RemovePicture(DynamicPicture picture)
        {
            Pictures.Remove(picture);
            ShowPictures = Pictures.Any();
        }
    }
}
