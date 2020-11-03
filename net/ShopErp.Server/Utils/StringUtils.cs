using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Utils
{
    class StringUtils
    {
        /// <summary>
        /// 过滤，控制字符，无法看见的字符，以及EMOJI 类的字符
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string FilterUnReadableChar(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return string.Empty;
            }

            char[] c = new char[str.Length];
            int count = 0;

            for (int i = 0; i < c.Length; i++)
            {
                var cc = Char.GetUnicodeCategory(str[i]);
                switch (cc)
                {
                    case System.Globalization.UnicodeCategory.Control:
                    case System.Globalization.UnicodeCategory.Surrogate:
                    case System.Globalization.UnicodeCategory.Format:
                    case System.Globalization.UnicodeCategory.LineSeparator:
                    case System.Globalization.UnicodeCategory.ParagraphSeparator:
                        {
                            break;
                        }
                    default:
                        {
                            c[count++] = str[i];
                            break;
                        }
                }
            }

            if (count <= 0)
            {
                return string.Empty;
            }

            return new string(c, 0, count);
        }
    }
}
