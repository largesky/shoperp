using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.DeliveryFormatters
{
    class DeliveryNumberText : IDeliveryFormatter
    {
        public string AcceptType
        {
            get { return PrintTemplateItemType.DELIVERY_DELIVERYNUMBERTEXT; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, WuliuNumber wuliuNumber)
        {
            if (string.IsNullOrWhiteSpace(wuliuNumber.DeliveryNumber))
            {
                throw new Exception("快递单号为空");
            }

            string s = "";
            string number = wuliuNumber.DeliveryNumber;
            int div_count = 4;
            int count = (number.Length + div_count - 1) / div_count;
            int mod = number.Length % div_count;

            for (int i = 0; i < count; i++)
            {
                if (mod == 0)
                {
                    s += number.Substring(i * div_count, div_count) + " ";
                }
                else
                {
                    if (i == 0)
                    {
                        s += number.Substring(0, mod) + " ";
                    }
                    else
                    {
                        s += number.Substring(mod + (i - 1) * div_count, div_count)+" ";
                    }
                }
            }
            return s.Trim();
        }
    }
}
