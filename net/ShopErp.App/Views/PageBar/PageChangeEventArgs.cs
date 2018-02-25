using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopErp.App.Views.PageBar
{
    public class PageChangeEventArgs : EventArgs
    {
        public int CurrentPage { get; set; }

        public int PageSize { get; set; }

        public IDictionary<string, object> Parameters { get; set; }

        public PageChangeEventArgs()
        {
        }

        public PageChangeEventArgs(int currentPage, int pageSize, IDictionary<string, object> parameter)
        {
            this.CurrentPage = currentPage;
            this.PageSize = pageSize;
            this.Parameters = parameter;
        }

        public T GetParameter<T>(string name)
        {
            if (this.Parameters.ContainsKey(name))
            {
                return (T) this.Parameters[name];
            }
            throw new Exception("未能找到指定的Key:" + name);
        }
    }
}