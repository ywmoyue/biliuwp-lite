using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.User.WatchLater;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Modules.User;

/// <summary>
/// 稍后再看
/// </summary>
[RegisterSingletonViewModel]
public class WatchLaterViewModel : BaseViewModel
{
    private readonly WatchLaterService m_watchLaterService;

    public WatchLaterViewModel()
    {
        m_watchLaterService = App.ServiceProvider.GetRequiredService<WatchLaterService>();
        AddCommand = new RelayCommand<string>(AddToWatchlater);
        AddCommandWithAvId = new RelayCommand<long>(AddToWatchlater);
        RefreshCommand = new RelayCommand(Refresh);
        CleanCommand = new RelayCommand(Clear);
        DeleteCommand = new RelayCommand<WatchlaterItemModel>(Del);
        CleanViewedCommand = new RelayCommand(ClearViewed);
        SelectCommand = new RelayCommand<object>(SetSelectMode);
    }

    public ICommand AddCommand { get; private set; }

    public ICommand AddCommandWithAvId { get; private set; }

    public ICommand CleanCommand { get; private set; }
    public ICommand CleanViewedCommand { get; private set; }
    public ICommand DeleteCommand { get; private set; }
    public ICommand RefreshCommand { get; private set; }
    public ICommand SelectCommand { get; private set; }

    public bool Loading { get; set; }

    public bool Nothing { get; set; }

    public ListViewSelectionMode SelectionMode { get; set; } = ListViewSelectionMode.None;

    public bool IsItemClickEnabled { get; set; } = true;

    public ObservableCollection<WatchlaterItemModel> Videos { get; set; }

    public async void AddToWatchlater(long aid)
    {
        AddToWatchlater(aid.ToString());
    }

    public async void AddToWatchlater(string aid)
    {
        await m_watchLaterService.AddToWatchlater(aid);
    }

    public async Task LoadData()
    {
        try
        {

            Loading = true;
            Nothing = false;

            var results = await m_watchLaterService.GetWatchLaterItems();
            if(results == null || results.Count == 0)
            {
                Nothing = true;
            }
            else
            {
                foreach (var item in results)
                {
                    item.DeleteCommand = DeleteCommand;
                }
            }

            Videos = new ObservableCollection<WatchlaterItemModel>(results);
        }
        catch (Exception ex)
        {
            var handel = HandelError<WatchLaterViewModel>(ex);
            NotificationShowExtensions.ShowMessageToast(handel.message);
        }
        finally
        {
            Loading = false;
        }
    }

    public async void Refresh()
    {
        if (Loading)
        {
            return;
        }
        Videos = null;
        await LoadData();
    }

    public async void Clear()
    {
        if(await m_watchLaterService.Clear())
        {
            Videos.Clear();
        }
    }

    public async void ClearViewed()
    {
        if(await m_watchLaterService.ClearViewed())
        {
            Refresh();
        }
    }

    public async void Del(WatchlaterItemModel item)
    {
        if(await m_watchLaterService.Remove(item.aid))
        {
            Videos?.Remove(item);
        }
    }

    public async Task<bool> Del(string aid)
    {
        var item = Videos?.FirstOrDefault(x => x.aid == aid);
        if (await m_watchLaterService.Remove(aid))
        {
            Videos?.Remove(item);
            return true;
        }
        return false;
    }

    private void SetSelectMode(object data)
    {
        if (data == null)
        {
            IsItemClickEnabled = true;
            SelectionMode = ListViewSelectionMode.None;
        }
        else
        {
            IsItemClickEnabled = false;
            SelectionMode = ListViewSelectionMode.Multiple;
        }
    }

    public async Task DelBatch(IList<WatchlaterItemModel> items)
    {
        int successCount = 0;
        NotificationShowExtensions.ShowMessageToast("批量操作中...");
        foreach (var item in items)
        {
            if (await m_watchLaterService.Remove(item.aid))
            {
                successCount++;
            }
            await Task.Delay(500);
        }
        foreach (var item in items)
        {
            Videos?.Remove(item);
        }
        NotificationShowExtensions.ShowMessageToast($"已成功移除{successCount}个视频");
    }
}
