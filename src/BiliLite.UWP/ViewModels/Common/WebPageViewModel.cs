using BiliLite.Models.Attributes;

namespace BiliLite.ViewModels.Common;

[RegisterTransientViewModel]
public class WebPageViewModel : BaseViewModel
{
    public bool IsEnableGoBack { get; set; }

    public bool IsEnableGoForward { get; set; }
}