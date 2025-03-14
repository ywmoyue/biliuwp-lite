﻿using BiliLite.Models;
using BiliLite.Models.Requests.Api.User;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common.Favorites;
using BiliLite.Models.Responses;
using BiliLite.ViewModels.User;
using BiliLite.Models.Common.User;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class CopyOrMoveFavVideoDialog : ContentDialog
    {
        readonly string fid, mid;
        readonly bool isMove;
        readonly List<FavoriteInfoVideoItemModel> selectItems;
        readonly FavoriteApi favoriteApi;
        private readonly IMapper m_mapper;

        public CopyOrMoveFavVideoDialog(string fid, string mid, bool isMove, List<FavoriteInfoVideoItemModel> items)
        {
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            this.InitializeComponent();
            favoriteApi = new FavoriteApi();
            this.fid = fid;
            this.mid = mid;
            this.isMove = isMove;
            this.selectItems = items;
            LoadFav();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            if (listView.SelectedItem == null) { Notify.ShowMessageToast("请选择收藏夹"); return; }
            try
            {
                IsPrimaryButtonEnabled = false;
                HttpResults results;
                var item = listView.SelectedItem as FavoriteItemViewModel;
                List<string> ids = new List<string>();
                foreach (var videoItem in selectItems)
                {
                    ids.Add(videoItem.Id);
                }
                if (isMove)
                {
                    results = await favoriteApi.Move(fid, item.Id, ids).Request();
                }
                else
                {
                    results = await favoriteApi.Copy(fid, item.Id, ids, mid).Request();
                }
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<object>>();
                    if (data.success)
                    {
                        Notify.ShowMessageToast("操作完成");
                        this.Hide();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }


            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                IsPrimaryButtonEnabled = true;
            }

        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        private async void LoadFav()
        {
            try
            {
                prLoading.Visibility = Visibility.Visible;

                var results = await favoriteApi.MyCreatedFavorite("").Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var myFavorite = await data.data["list"].ToString().DeserializeJson<List<FavoriteItemModel>>();
                        var list = m_mapper.Map<List<FavoriteItemViewModel>>(myFavorite);
                        listView.ItemsSource = list.Where(x => x.Id != fid).ToList();
                    }
                    else
                    {
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {

                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                prLoading.Visibility = Visibility.Collapsed;
            }
        }

    }
}
