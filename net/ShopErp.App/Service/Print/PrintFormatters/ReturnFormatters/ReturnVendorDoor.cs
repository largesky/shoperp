using System;
using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.ReturnFormatters
{
    class ReturnVendorDoor : IReturnFormatter
    {
        public string AcceptType { get { return PrintTemplateItemType.RETURN_VENDORDOOR; } }

        public object Format(PrintTemplate template, PrintTemplateItem item, OrderReturn or)
        {
            string vendorName = "";
            var ss = or.GoodsInfo.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (ss.Length > 0)
            {
                vendorName = ss[0];
            }
            if (string.IsNullOrWhiteSpace(vendorName))
            {
                return "";
            }
            var vens = ServiceContainer.GetService<VendorService>().GetVendorAddress_InCach(vendorName);
            if (string.IsNullOrWhiteSpace(vens))
            {
                return "";
            }
            var door = VendorService.FormatVendorDoor(vens);
            return door;
        }
    }
}
