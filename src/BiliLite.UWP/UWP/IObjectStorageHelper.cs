using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliLite.UWP
{
    public interface IObjectStorageHelper
    {
        bool KeyExists(string key);
        T Read<T>(string key);
        void Save<T>(string key, T value);
        void Remove(string key);
        void Clear();
    }
}
