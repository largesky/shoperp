using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShopErp.App.CefSharpUtils
{
    class CefCookieVisitor : ICookieVisitor
    {
        public string Domain { get; private set; }

        public string Name { get; set; }

        private Dictionary<string, string> Cookies { get; set; }

        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private bool find = false;

        public CefCookieVisitor(string domain, string name)
        {
            this.Domain = domain;
            this.Name = name;
            this.Cookies = new Dictionary<string, string>();
        }

        public void Dispose()
        {
        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            //查找某个指定的COOKIE
            if (string.IsNullOrWhiteSpace(Name) == false)
            {
                Debug.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(cookie));
                if (Domain.IndexOf(cookie.Domain, StringComparison.OrdinalIgnoreCase) >= 0 && cookie.Name == this.Name)
                {
                    this.Cookies.Add(Name, cookie.Value);
                    find = true;
                }

                if (find || count >= total - 1)
                {
                    this.autoResetEvent.Set();
                }
                return !find;
            }

            //查找某个域下所有的COOKIE
            if (Domain.IndexOf(cookie.Domain, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Cookies.Add(cookie.Name, cookie.Value);
            }
            if (count >= total - 1)
            {
                this.autoResetEvent.Set();
            }
            return true;
        }

        private Dictionary<string, string> WaitValue()
        {
            bool ret = this.autoResetEvent.WaitOne(1000 * 60 * 10);
            if (ret)
            {
                return Cookies;
            }
            throw new Exception("等待读取COOKIE超时10分钟");
        }

        public static string GetCookieValues(string domain, string name)
        {
            CefCookieVisitor cefCookieVisitor = new CefCookieVisitor(domain, name);
            //VisitAllCookies 是一个异步方法，方法内部会使用其它线程执行
            Cef.GetGlobalCookieManager().VisitAllCookies(cefCookieVisitor);
            var dic = cefCookieVisitor.WaitValue();
            return string.Join(";", dic.Select(obj => obj.Key + "=" + obj.Value));
        }
    }
}
