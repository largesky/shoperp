using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.App.Service.Net
{
    public class MsHttpRestful
    {
        private static readonly Newtonsoft.Json.JsonSerializerSettings JsonDatetimeSetting = new Newtonsoft.Json.JsonSerializerSettings { DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat };
        private static readonly System.Text.RegularExpressions.Regex RegUrlEncoding = new System.Text.RegularExpressions.Regex(@"%[a-f0-9]{2}");
        private static readonly Dictionary<string, string> EmptyDicValues = new Dictionary<string, string>();
        public static int NETWORK_MAX_TIME_OUT = 10;

        public static string UrlEncode(string str, System.Text.Encoding e)
        {
            if (str == null)
            {
                return "";
            }
            String stringToEncode = System.Web.HttpUtility.UrlEncode(str, e ?? System.Text.Encoding.UTF8).Replace("+", "%20").Replace("*", "%2A").Replace("(", "%28").Replace(")", "%29");
            return RegUrlEncoding.Replace(stringToEncode, m => m.Value.ToUpperInvariant());
        }

        private static System.Net.Http.HttpResponseMessage SendHttpRequestMessage(System.Net.Http.HttpMethod httpMethod, string url, System.Net.Http.HttpContent httpContent, IDictionary<string, string> headers, string referer = null)
        {
            var timeout = System.Diagnostics.Debugger.IsAttached ? new TimeSpan(1, 23, 59, 59) : new TimeSpan(0, NETWORK_MAX_TIME_OUT, 0);
            var client = new System.Net.Http.HttpClient(new System.Net.Http.HttpClientHandler { UseCookies = false, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate }) { Timeout = timeout };
            var httpRequestMessage = new System.Net.Http.HttpRequestMessage(httpMethod, url);
            httpRequestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            httpRequestMessage.Headers.Add("Accept-Encoding", "gzip,deflate");
            httpRequestMessage.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            httpRequestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36 Edg/85.0.564.68");

            if (string.IsNullOrWhiteSpace(referer) == false)
            {
                httpRequestMessage.Headers.Add("Referer", referer);
            }
            if (headers != null && headers.Count > 0)
            {
                foreach (var paire in headers)
                {
                    httpRequestMessage.Headers.Add(paire.Key, paire.Value ?? "");
                }
            }
            httpRequestMessage.Content = httpContent;
            try
            {
                System.Net.Http.HttpResponseMessage ret = client.SendAsync(httpRequestMessage).Result;
                if (ret.IsSuccessStatusCode == false)
                {
                    throw new Exception("HTTP请求错误:" + ret.StatusCode);
                }
                return ret;
            }
            catch (Exception ex)
            {
                Exception e = ex;
                while (e.InnerException != null)
                {
                    e = e.InnerException;
                }
                throw e;
            }
        }


        private static byte[] SendHttpRequestMessageAndReturnBytes(System.Net.Http.HttpMethod httpMethod, string url, System.Net.Http.HttpContent httpContent, IDictionary<string, string> headers, string referer = null)
        {
            return SendHttpRequestMessage(httpMethod, url, httpContent, headers, referer).Content.ReadAsByteArrayAsync().Result;
        }

        private static string SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod httpMethod, string url, System.Net.Http.HttpContent httpContent, IDictionary<string, string> headers, string referer = null)
        {
            var ret = SendHttpRequestMessage(httpMethod, url, httpContent, headers, referer);
            string str = ret.Content.ReadAsStringAsync().Result;
            return str;
        }


        #region 返回字符的方法

        public static string PostJsonBodyReturnString(string url, IDictionary<string, object> values, IDictionary<string, string> headers = null, System.Text.Encoding encoding = null)
        {
            var json = (values == null || values.Count < 1) ? "" : Newtonsoft.Json.JsonConvert.SerializeObject(values, JsonDatetimeSetting);
            return SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod.Post, url, new System.Net.Http.StringContent(json, (encoding ?? System.Text.Encoding.UTF8), "application/json"), headers);
        }

        public static string PostUrlEncodeBodyReturnString(string url, IDictionary<string, string> values, IDictionary<string, string> headers = null, System.Text.Encoding encoding = null, string referer = null)
        {
            string scontent = string.Join("&", values.Select(obj => obj.Key + "=" + UrlEncode(obj.Value, (encoding ?? System.Text.Encoding.UTF8))));
            var content = new System.Net.Http.StringContent(scontent, (encoding ?? System.Text.Encoding.UTF8), "application/x-www-form-urlencoded");
            return SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod.Post, url, content, headers, referer);
        }

        public static String PostBytesBodyReturnString(string url, byte[] values, IDictionary<string, string> headers = null, System.Text.Encoding encoding = null)
        {
            return SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod.Post, url, new System.Net.Http.ByteArrayContent(values), headers);
        }

        public static string PostMultipartFormDataBodyReturnString(string url, IDictionary<string, string> values, IDictionary<string, System.IO.FileInfo> files, IDictionary<string, string> headers = null, System.Text.Encoding encoding = null)
        {
            var content = new System.Net.Http.MultipartFormDataContent();
            foreach (var item in values)
            {
                content.Add(new System.Net.Http.StringContent(item.Value as string, (encoding ?? System.Text.Encoding.UTF8), "application/x-www-form-urlencoded"), item.Key);
            }
            foreach (var item in files)
            {
                var vv = new System.Net.Http.ByteArrayContent(System.IO.File.ReadAllBytes(item.Value.FullName));
                vv.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/" + item.Value.Extension.ToLower());
                content.Add(vv, item.Key, item.Value.Name);
            }
            return SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod.Post, url, content, headers);
        }

        public static string GetReturnString(string url, IDictionary<string, string> headers = null)
        {
            return SendHttpRequestMessageAndReturnString(System.Net.Http.HttpMethod.Get, url, null, headers);
        }

        #endregion

        #region 返回字节数据

        public static byte[] GetReturnBytes(string url, IDictionary<string, string> headers = null, string referer = null)
        {
            return SendHttpRequestMessageAndReturnBytes(System.Net.Http.HttpMethod.Get, url, null, headers);
        }

        #endregion

    }
}