using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public abstract class DeliveryPrintDocument
    {
        public ShopErp.Domain.Order[] Orders { get; protected set; }

        public WuliuNumber[] WuliuNumbers { get; protected set; }

        public Dictionary<string, string>[] UserDatas { get; set; }

        public PrintTemplate WuliuTemplate { get; protected set; }

        /// <summary>
        /// 整个文档开始打印
        /// </summary>
        public event EventHandler PrintStarting;

        /// <summary>
        /// 开始打印一页
        /// </summary>
        public event Func<object, int, bool> PagePrintStarting;

        /// <summary>
        /// 结束打印一页
        /// </summary>
        public event Func<object, int, bool> PagePrintEnded;

        /// <summary>
        /// 整个文档结束
        /// </summary>
        public event EventHandler PrintEnded;

        /// <summary>
        /// 出现错误
        /// </summary>
        public event EventHandler<string> Error;

        protected virtual void BenginPrint() { }

        protected virtual void EndPrint() { }

        public abstract string StartPrint(string printer,string printServerAdd);

        protected void OnPrintStarting()
        {
            if (this.PrintStarting != null)
            {
                this.PrintStarting(this, new EventArgs());
            }
        }

        protected void OnPrintEnded()
        {
            if (this.PrintEnded != null)
            {
                this.PrintEnded(this, new EventArgs());
            }
        }

        protected bool OnPagePrintStarting(int i)
        {
            if (PagePrintStarting != null)
            {
                return this.PagePrintStarting(this, i);
            }
            return false;
        }

        protected bool OnPagePrintEnded(int i)
        {
            if (PagePrintEnded != null)
            {
                return this.PagePrintEnded(this, i);
            }
            return false;
        }

        protected void OnError(string msg)
        {
            if (Error != null)
            {
                this.Error(this, msg);
            }
        }

        public DeliveryPrintDocument(Order[] orders, WuliuNumber[] wuliuNumbers, Dictionary<string, string>[] userDatas, PrintTemplate wuliuTemplate)
        {
            this.WuliuNumbers = wuliuNumbers;
            this.Orders = orders;
            this.UserDatas = userDatas;
            this.WuliuTemplate = wuliuTemplate;
        }
    }
}
