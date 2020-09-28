using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Utils
{
    public class DateTimeUtil
    {
        /// <summary>
        /// 数据库最小时间
        /// </summary>
        public static readonly DateTime DbMinTime = new DateTime(1970, 01, 01);

        /// <summary>
        /// 格式化成 yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 是否比数据库最小时间小
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static bool IsDbMinTime(DateTime time)
        {
            return time < DbMinTime;
        }
    }
}
