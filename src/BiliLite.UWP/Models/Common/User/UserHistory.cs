using System.Collections.Generic;

namespace BiliLite.Models.Common.User
{
    public class UserHistory
    {
        public HistoryCursor Cursor { get; set; }

        public List<UserHistoryItem> List { get; set; }
    }
}
