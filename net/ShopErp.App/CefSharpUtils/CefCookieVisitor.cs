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

        /// <summary>
        /// 获取单个DOMAIN下的指定NAME的值
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSignleCookieValue(string domain, string name)
        {
            if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("domain或者name为空");
            }
            CefCookieVisitor cefCookieVisitor = new CefCookieVisitor(domain, name);
            //VisitAllCookies 是一个异步方法，方法内部会使用其它线程执行
            Cef.GetGlobalCookieManager().VisitAllCookies(cefCookieVisitor);
            var dic = cefCookieVisitor.WaitValue();
            if (dic.Count < 1)
            {
                throw new Exception(string.Format("未找到域名:{0}下名为:{1}的cookie", domain, name));
            }
            return dic.First().Value;
        }

        /// <summary>
        /// 获取某个域下的所有COOKIE，保存形式为 Dictionary["Cookie"]=所有cookie的值，可以直接用于HTTP层的HEADER，无须其它处理
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetCookieValue(string domain)
        {
            CefCookieVisitor cefCookieVisitor = new CefCookieVisitor(domain, null);
            //VisitAllCookies 是一个异步方法，方法内部会使用其它线程执行
            Cef.GetGlobalCookieManager().VisitAllCookies(cefCookieVisitor);
            var dic = cefCookieVisitor.WaitValue();
            string cookie = string.Join(";", dic.Select(obj => obj.Key + "=" + obj.Value));
            Dictionary<string, string> dd = new Dictionary<string, string>();
            dd["Cookie"] = cookie;
            return dd;
        }

    }
}
