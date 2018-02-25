using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ShopErp.App.Service.Print
{
    public class PrintDataFormatterManagerBase<T> where T : class, PrintDataFormatterBase
    {
        private static List<T> formatters = new List<T>();

        static PrintDataFormatterManagerBase()
        {
            Type[] types = Assembly.GetExecutingAssembly().GetTypes().Where(obj => obj.GetInterface(typeof(T).FullName) != null).ToArray();
            T[] temp = types.Select(obj => Activator.CreateInstance(obj) as T).ToArray();
            formatters.AddRange(temp);
        }

        public static T GetPrintDataFormatter(string type)
        {
            T formatter = formatters.FirstOrDefault(obj => obj.AcceptType == type);
            if (formatter == null)
            {
                throw new Exception("未能找到格式化算法：" + type);
            }
            return formatter;
        }
    }
}
