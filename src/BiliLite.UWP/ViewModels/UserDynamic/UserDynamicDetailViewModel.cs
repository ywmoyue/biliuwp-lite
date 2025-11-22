using MapsterMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Modules;
using BiliLite.Modules.User;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.UserDynamic;

[RegisterTransientViewModel]
public class UserDynamicDetailViewModel : BaseViewModel, IUserDynamicCommands
{
    private readonly GrpcService m_grpcService;
    private readonly IMapper m_mapper;
    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
    private readonly WatchLaterVM m_watchLaterVm;

    public UserDynamicDetailViewModel(GrpcService grpcService, IMapper mapper)
    {
        m_grpcService = grpcService;
        m_mapper = mapper;
        m_watchLaterVm = new WatchLaterVM();
        UserCommand = new RelayCommand<object>(OpenUser);
        DetailCommand = new RelayCommand<string>(DynamicExtensions.OpenDetail);
        ImageCommand = new RelayCommand<object>(DynamicExtensions.OpenImage);
        WebDetailCommand = new RelayCommand<string>(DynamicExtensions.OpenWebDetail);
        CommentCommand = new RelayCommand<DynamicV2ItemViewModel>(OpenComment);
        LikeCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.DoLike);
        RepostCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.OpenSendDynamicDialog);
        LaunchUrlCommand = new RelayCommand<string>(DynamicExtensions.LaunchUrl);
        CopyDynCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.CopyDyn);
        OpenArticleCommand = new RelayCommand<DynamicV2ItemViewModel>(DynamicExtensions.OpenArticle);
        TagCommand = new RelayCommand<object>(DynamicExtensions.OpenTag);
        WatchLaterCommand = m_watchLaterVm.AddCommandWithAvId;
    }

    public ObservableCollection<DynamicV2ItemViewModel> DynamicItems { get; set; }

    [DependsOn(nameof(DynamicItems))]
    public long ReplyCount
    {
        get
        {
            var item = DynamicItems?.FirstOrDefault();
            return item == null ? 0 : item.Stat.Reply;
        }
    }

    [DependsOn(nameof(DynamicItems))]
    public long RepostCount
    {
        get
        {
            var item = DynamicItems?.FirstOrDefault();
            return item == null ? 0 : item.Stat.Repost;
        }
    }

    public bool Loading { get; set; }

    public ICommand LaunchUrlCommand { get; set; }

    public ICommand RepostCommand { get; set; }

    public ICommand LikeCommand { get; set; }

    public ICommand CommentCommand { get; set; }

    public ICommand UserCommand { get; set; }

    public ICommand LoadMoreCommand { get; private set; }

    public ICommand WebDetailCommand { get; set; }

    public ICommand DetailCommand { get; set; }

    public ICommand ImageCommand { get; set; }

    public ICommand WatchLaterCommand { get; set; }

    public ICommand CopyDynCommand { get; set; }

    public ICommand OpenArticleCommand { get; set; }

    public ICommand TagCommand { get; set; }

    public event EventHandler<DynamicV2ItemViewModel> OpenCommentEvent;

    private void OpenUser(object parameter)
    {
        this.OpenUserEx(parameter);
    }

    private void OpenComment(DynamicV2ItemViewModel data)
    {
        OpenCommentEvent?.Invoke(this, data);
    }

    public async Task LoadData(string id)
    {
        try
        {
            Loading = true;
            var dynDetail = await m_grpcService.GetDynDetail(id);
            var item = m_mapper.Map<DynamicV2ItemViewModel>(dynDetail.Item);
            item.Parent = this;
            DynamicItems = new ObservableCollection<DynamicV2ItemViewModel>() { item };
        }
        catch (Exception ex)
        {
            NotificationShowExtensions.ShowMessageToast("加载动态数据失败");
            _logger.Warn(ex.Message, ex);
        }
        Loading = false;
    }
}