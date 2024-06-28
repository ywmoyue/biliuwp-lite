using System.Collections.Generic;
using System.Net.Http;
using BiliLite.Models.Requests;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    /// <summary>
    /// 网络请求构造器
    /// </summary>
    public class BiliRequestBuilder
    {
        private IDictionary<string, string> m_headers;
        private IDictionary<string, string> m_cookies;
        private HttpMethod m_method = HttpMethod.Get;
        private HttpContent m_body;
        private readonly string m_url;
        private bool m_needRedirect = false;

        public BiliRequestBuilder(string url)
        {
            m_url = url;
        }

        public BiliRequestBuilder SetPostBody(string body, Dictionary<string, object> formBody)
        {
            if (formBody != null && formBody.Count > 0)
            {
                SetFormBody(formBody);
            }
            else
            {
                SetUrlEncodeBody(body);
            }

            m_method = HttpMethod.Post;
            return this;
        }

        public BiliRequestBuilder SetPostForm(Dictionary<string, object> formBody)
        {
            var content = new MultipartFormDataContent();
            foreach (var formKeyValuePair in formBody)
            {
                if (formKeyValuePair.Value is UploadFileInfo file)
                {
                    content.Add(new ByteArrayContent(file.Data), formKeyValuePair.Key, file.FileName);
                }else if (formKeyValuePair.Value is string valueStr)
                {
                    content.Add(new StringContent(valueStr), formKeyValuePair.Key);
                }
            }

            m_body = content;
            return this;
        }

        public BiliRequestBuilder SetHeaders(IDictionary<string, string>  headers = null)
        {
            m_headers = headers;
            return this;
        }

        public BiliRequestBuilder SetCookies(IDictionary<string, string> cookies = null)
        {
            m_cookies = cookies;
            return this;
        }

        public BiliRequestBuilder SetNeedRedirect()
        {
            m_needRedirect = true;
            return this;
        }

        public BiliRequest Build()
        {
            return new BiliRequest(m_url, m_headers, m_cookies, m_method, m_body, m_needRedirect);
        }

        private void SetUrlEncodeBody(string body)
        {
            m_body = string.IsNullOrEmpty(body) ? null : new StringContent(
                body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        }

        private void SetFormBody(Dictionary<string, object> formBody)
        {
            var content = new MultipartFormDataContent();
            foreach (var formKeyValuePair in formBody)
            {
                if (formKeyValuePair.Value is UploadFileInfo file)
                {
                    content.Add(new ByteArrayContent(file.Data), formKeyValuePair.Key, file.FileName);
                }
                else if (formKeyValuePair.Value is string valueStr)
                {
                    content.Add(new StringContent(valueStr), formKeyValuePair.Key);
                }
            }

            m_body = content;
        }
    }
}
