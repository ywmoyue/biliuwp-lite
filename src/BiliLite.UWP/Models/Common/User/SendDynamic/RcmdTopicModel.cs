namespace BiliLite.Models.Common.User.SendDynamic
{
    public class RcmdTopicModel
    {
        public int topic_id { get; set; }
        public string topic_name { get; set; }
        public int is_activity { get; set; }
        public string display
        {
            get { return "#" + topic_name + "#"; }
        }
    }
}