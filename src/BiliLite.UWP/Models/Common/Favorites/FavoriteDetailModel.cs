using System.Collections.Generic;
using BiliLite.Modules;

namespace BiliLite.Models.Common.Favorites;

public class FavoriteDetailModel
{
    public FavoriteInfoModel Info { get; set; }

    public List<FavoriteInfoVideoItemModel> Medias { get; set; }
}