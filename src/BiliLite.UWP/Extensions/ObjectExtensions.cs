using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace BiliLite.Extensions
{
    public static class ObjectExtensions
    {
        public static long ToInt64(this object obj)
        {

            if (long.TryParse(obj.ToString(), out var value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public static int ToInt32(this object obj)
        {

            if (int.TryParse(obj.ToString(), out var value))
            {
                return value;
            }
            else
            {
                return 0;
            }
        }

        public static string ToCountString(this object obj)
        {
            if (obj == null) return "0";
            if (double.TryParse(obj.ToString(), out var number))
            {

                if (number >= 10000)
                {
                    return ((double)number / 10000).ToString("0.0") + "万";
                }
                return obj.ToString();
            }
            else
            {
                return obj.ToString();
            }
        }

        public static T ObjectClone<T>(this T obj)
        {
            if (ReferenceEquals(obj, null))
                return default;

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(obj);
                return System.Text.Json.JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception)
            {
                return default;
            }
        }


        public static T ObjectCloneWithoutSerializable<T>(this T obj)
        {
            try
            {
                var serializedObj = JsonConvert.SerializeObject(obj);
                return JsonConvert.DeserializeObject<T>(serializedObj);
            }
            catch (Exception e)
            {
                return default;
            }
        }
    }
}
