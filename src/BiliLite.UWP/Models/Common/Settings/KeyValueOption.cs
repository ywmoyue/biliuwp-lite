namespace BiliLite.Models.Common.Settings;

public class KeyValueOption<T>
{
    public KeyValueOption(string key, T value)
    {
        Key = key; 
        Value = value;
    }

    public KeyValueOption(){}

    public string Key { get; set; }

    public T Value { get; set; }
}