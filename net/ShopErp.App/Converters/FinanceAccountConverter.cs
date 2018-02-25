using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ShopErp.App.Converters
{
    public class FinanceAccountConverter : IValueConverter
    {
        static List<FinanceAccount> fasAccounts = new List<FinanceAccount>();

        public static string Convert(long faId)
        {
            if (faId <= 0)
            {
                return "";
            }
            var fa = fasAccounts.FirstOrDefault(obj => obj.Id == faId);
            if (fa == null)
            {
                lock (fasAccounts)
                {
                    fasAccounts.Clear();
                    fasAccounts.AddRange(ServiceContainer.GetService<FinanceAccountService>().GetByAll().Datas);
                    fa = fasAccounts.FirstOrDefault(obj => obj.Id == faId);
                }
            }
            return fa == null ? "不存在" : fa.ShortInfo;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return Convert((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
