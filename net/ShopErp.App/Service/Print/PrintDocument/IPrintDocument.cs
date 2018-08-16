using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument
{
    interface IPrintDocument
    {
        /// <summary>
        /// 页面开始生成
        /// </summary>
        event Action<object, int> PageGening;

        /// <summary>
        /// 页面生成完成
        /// </summary>
        event Action<object, int> PageGened;

        /// <summary>
        /// 开始输出页面到打印机事件
        /// </summary>
        event Action<object, int> PagePrinting;

        /// <summary>
        /// 开始输出页面到打印机事件
        /// </summary>
        event Action<object, int> PagePrinted;

        void Print(string printer);
    }
}
