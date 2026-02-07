using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace BiliLite.Models.Common.User.WatchLater;

public class WatchlaterItemModel
{
    public ICommand DeleteCommand { get; set; }
    public string aid { get; set; }
    public int videos { get; set; }
    public string tname { get; set; }
    public string pic { get; set; }
    public string title { get; set; }
    public string desc { get; set; }
    public string dynamic { get; set; }
    public long cid { get; set; }
    public long add_at { get; set; }

    public WatchlaterOwnerModel owner { get; set; }

    public List<WatchlaterPagesModel> pages { get; set; }

    public int progress { get; set; }
    public int duration { get; set; }

    public string display
    {
        get
        {
            var ts = TimeSpan.FromSeconds(duration);
            return $"{videos}P {ts.ToString("c")}";
        }
    }

    public string state
    {
        get
        {
            if (progress == -1)
            {
                return "已看完";
            }
            else
            {
                if (progress != 0)
                {
                    return "看到 " + TimeSpan.FromSeconds(progress).ToString(@"mm\:ss");
                }
                else
                {
                    return "尚未观看";
                }

            }
        }
    }

}
