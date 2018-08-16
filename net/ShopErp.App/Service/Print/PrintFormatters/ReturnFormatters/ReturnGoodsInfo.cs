using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    class ReturnGoodsInfo : IReturnFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.RETURN_GOODSINFO; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            var ss = or.GoodsInfo.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string newInfo = "";
            for (int i = 1; i < ss.Length; i++)
            {
                newInfo += ss[i];
            }
            return newInfo;
        }
    }
}
