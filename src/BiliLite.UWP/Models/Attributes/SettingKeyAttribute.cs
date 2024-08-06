using System;

namespace BiliLite.Models.Attributes
{
    public class SettingKeyAttribute:Attribute
    {
        public SettingKeyAttribute()
        {
            Type = typeof(string);
        }

        public SettingKeyAttribute(Type type, bool useSqlDb = false)
        {
            Type = type;
            UseSqlDb = useSqlDb;
        }

        public Type Type { get; set; }

        public bool UseSqlDb { get; set; }
    }
}
