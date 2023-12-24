using System;
using Newtonsoft.Json;

namespace BiliLite.Models.Exceptions
{
    public class CustomizedErrorWithDataException:Exception
    {
        private readonly object m_data;

        public string DataText => JsonConvert.SerializeObject(m_data);

        public CustomizedErrorWithDataException(string msg, object data) : base(msg)
        {
            m_data = data;
        }
    }
}
