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

        public string Name { get; private set; }

        public string Value { get; private set; }

        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private bool find = false;

        public CefCookieVisitor(string domain, string name)
        {
            this.Domain = domain;
            this.Name = name;
        }

        public void Dispose()
        {

        }

        public bool Visit(Cookie cookie, int count, int total, ref bool deleteCookie)
        {
            if (cookie.Domain == this.Domain && cookie.Name == this.Name)
            {
                this.Value = cookie.Value;
                find = true;
            }

            if (find || count >= total)
            {
                this.autoResetEvent.Set();
            }
            return !find;
        }

        public string WaitValue()
        {
            bool ret = this.autoResetEvent.WaitOne(1000 * 60);
            if (ret)
            {
                return Value;
            }
            throw new Exception("等待读取COOKIE超时1分钟");
        }
    }
}
