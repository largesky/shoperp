using System;
using System.Windows;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    public class OrderFormatterManager : PrintDataFormatterManagerBase<IOrderFormatter>
    {
        public static object Format(PrintTemplate template, PrintTemplateItem templateItem, Order order)
        {
            if (order == null)
            {
                throw new Exception("要格式化的订单参数为空");
            }

            if (templateItem == null)
            {
                throw new Exception("要格式化的格式参数为空");
            }

            IOrderFormatter formatter = GetPrintDataFormatter(templateItem.Type);
            try
            {
                return formatter.Format(template, templateItem, order);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "格式化数据错误，订单编号:" + order.Id);
                return "";
            }
        }
    }
}
