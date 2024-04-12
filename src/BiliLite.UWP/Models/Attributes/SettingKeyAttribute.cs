using System;

namespace BiliLite.Models.Attributes
{
    public class SettingKeyAttribute:Attribute
    {
        public SettingKeyAttribute()
        {
            Type = typeof(string);
        }

        public SettingKeyAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; set; }
    }
}
