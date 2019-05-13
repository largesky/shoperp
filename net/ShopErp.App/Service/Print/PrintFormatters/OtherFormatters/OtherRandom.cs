using System;
using System.Linq;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OtherFormatters
{
    public class OtherRandom : IOtherFormatter
    {
        Random r = new Random((int)DateTime.Now.Ticks);

        public string AcceptType { get { return PrintTemplateItemType.OTHER_RANDOM; } }

        public object Format(PrintTemplate template, PrintTemplateItem item)
        {
            if (string.IsNullOrWhiteSpace(item.Format) || string.IsNullOrWhiteSpace(item.Value))
            {
                throw new Exception("配置项参数不完整");
            }

            char[] cc = new char[int.Parse(item.Value)];
            for (int i = 0; i < cc.Length; i++)
            {
                int j = r.Next(item.Format.Length);
                cc[i] = item.Format[j];
            }
            if (cc[0] == '-' && cc.Length > 1)
            {
                cc[0] = item.Format.FirstOrDefault(c => c != '-');
            }

            return item.Value1 + new string(cc);
        }
    }
}
