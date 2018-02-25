using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Windows.Controls;

namespace ShopErp.App.Utils
{
    public class PrintUtil
    {
        /// <summary>
        /// 获取指定打印机
        /// </summary>
        /// <param name="printer"></param>
        /// <returns></returns>
        public static PrintDialog GetPrinter(string printer)
        {
            if (string.IsNullOrWhiteSpace(printer))
            {
                throw new Exception("打印机名称为空，请设置打印机");
            }
            PrintQueue pq = null;
            try
            {
                pq = new LocalPrintServer().GetPrintQueue(printer);
            }
            catch (Exception ex)
            {
                throw new Exception("获取打印机失败:" + ex.Message);
            }

            if (pq == null)
            {
                throw new Exception("打印机:" + printer + "不存在");
            }
            var pd = new PrintDialog {PrintQueue = pq};
            var ca = pq.GetPrintCapabilities(pd.PrintTicket);
            pd.PrintTicket.PageBorderless = PageBorderless.Borderless;
            pd.PrintTicket.PageOrientation = PageOrientation.Portrait;
            pd.PrintTicket.PagesPerSheetDirection = PagesPerSheetDirection.LeftBottom;
            return pd;
        }
    }
}