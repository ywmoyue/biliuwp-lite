using BiliLite.Modules.Player;
using BiliLite.ViewModels.Common;
using System.Collections.Generic;

namespace BiliLite.ViewModels
{
    public class PlayControlViewModel: BaseViewModel
    {
        public List<InteractionEdgeInfoQuestionModel> Questions { get; set; }
    }
}
