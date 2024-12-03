namespace BiliLite.Models.Common.Region
{
    public class RegionChildOrderModel
    {
        public RegionChildOrderModel(string name, string order)
        {
            this.Name = name;
            this.Order = order;
        }

        public string Name { get; set; }

        public string Order { get; set; }
    }
}