using System.ComponentModel.DataAnnotations;

namespace BiliLite.Models.Databases
{
    public class SettingItem
    {
        [Key]
        public string Key { get; set; }

        public string? Value { get; set; }
    }
}
