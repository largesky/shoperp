using System.Linq;
using System.Text;
using ShopErp.App.Domain;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.OrderFormatters
{
    class OrderGoodsInfoAndSellerComment : IOrderFormatter
    {
        private VendorService vs = null;

        public string AcceptType
        {
            get { return PrintTemplateItemType.ORDER_GOODS_INFO_AND_SELLER_COMMENT; }
        }

        public object Format(PrintTemplate template, PrintTemplateItem item, Order order)
        {
            StringBuilder sb = new StringBuilder();
            if (vs == null)
            {
                vs = ServiceContainer.GetService<VendorService>();
            }

            if (order.Type != OrderType.SHUA || (order.Type == OrderType.SHUA && item.Format == "是"))
            {
                if (order.OrderGoodss != null && order.OrderGoodss.Count > 0)
                {
                    foreach (var goods in order.OrderGoodss.Where(obj => (int)obj.State <= (int)OrderState.SUCCESS))
                    {
                        string areaAndDoor = VendorService.FindAreaOrStreet(vs.GetVendorAddress_InCach(goods.Vendor), "区") + "-" + VendorService.FindDoor(vs.GetVendorAddress_InCach(goods.Vendor));
                        sb.AppendLine(areaAndDoor + " " + vs.GetVendorPingyingName(goods.Vendor).ToUpper() + " " + goods.Number + " " + goods.Edtion + " " + goods.Color + " " + goods.Size + " (" + goods.Count + ")");
                    }
                }
                if (order.PopPayType != PopPayType.COD)
                    sb.AppendLine(order.PopSellerComment);
            }
            else
            {
                sb.AppendLine(item.Value);
            }

            return sb.ToString();
        }
    }
}
