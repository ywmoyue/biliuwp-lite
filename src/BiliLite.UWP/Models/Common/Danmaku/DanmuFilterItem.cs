namespace BiliLite.Models.Common.Danmaku
{
    public class DanmuFilterItem
    {
        public int Id { get; set; }

        public long Mid { get; set; }

        public int Type { get; set; }

        public string Filter { get; set; }

        public string Comment { get; set; }

        public long Ctime { get; set; }

        public long Mtime { get; set; }
    }
}