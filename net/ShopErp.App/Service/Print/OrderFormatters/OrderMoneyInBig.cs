using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderMoneyInBig : IOrderFormatter
    {
        public const string BIG_NUMBERS = "零壹贰叁肆伍陆柒捌玖";

        public  string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_MONEY_INBIG; }
        }

        public  object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            string strMoney = item.Format;
            int money = (int)order.PopOrderTotalMoney;

            money = money % 100000;
            if (strMoney.Contains("万"))
            {
                strMoney = strMoney.Replace('万', BIG_NUMBERS[money / 10000]);
            }

            money = money % 10000;
            if (strMoney.Contains("千"))
            {
                strMoney = strMoney.Replace('千', BIG_NUMBERS[money / 1000]);
            }

            money = money % 1000;
            if (strMoney.Contains("百"))
            {
                strMoney = strMoney.Replace('百', BIG_NUMBERS[money / 100]);
            }

            money = money % 100;
            if (strMoney.Contains("十"))
            {
                strMoney = strMoney.Replace('十', BIG_NUMBERS[money / 10]);
            }

            money = money % 10;
            if (strMoney.Contains("个"))
            {
                strMoney = strMoney.Replace('个', BIG_NUMBERS[money / 1]);
            }

            return strMoney;
        }
    }
}
