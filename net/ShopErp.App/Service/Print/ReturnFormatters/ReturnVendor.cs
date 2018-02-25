using System;
using ShopErp.App.Domain;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    class ReturnVendor : IReturnFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.RETURN_VENDOR; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            var ss = or.GoodsInfo.Split(new char[] { ',', '，', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 0)
            {
                return ss[0];
            }
            throw new Exception("无法解析信息:" + or.GoodsInfo);
        }
    }
}
