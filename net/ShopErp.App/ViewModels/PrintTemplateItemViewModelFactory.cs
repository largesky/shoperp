using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.App.Domain;
using ShopErp.App.Service.Print;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelFactory
    {
        public static PrintTemplateItemViewModelCommon Create(Service.Print.PrintTemplate template, string type, string previewValue)
        {
            //退货类型
            if (type == Service.Print.PrintTemplateItemType.RETURN_ORDERCHANDE_ID_BARCODE)
            {
                return new PrintTemplateItemViewModelForBarcode(template);
            }

            //订单类型
            if (type == Service.Print.PrintTemplateItemType.ORDER_PAYTIME)
            {
                return new PrintTemplateItemViewModelForDate(template);
            }

            if (type == Service.Print.PrintTemplateItemType.ORDER_MONEY_INBIG)
            {
                return new PrintTemplateItemViewModelForPriceInBig(template);
            }

            if (type == Service.Print.PrintTemplateItemType.ORDER_RECEIVER_PHONE || type == Service.Print.PrintTemplateItemType.ORDER_RECEIVER_MOBILE)
            {
                return new PrintTemplateItemViewModelForReciverPhone(template);
            }

            if (type == Service.Print.PrintTemplateItemType.ORDER_RECEIVER_INFOALL)
            {
                return new PrintTemplateItemViewModelForReciverInfoAll(template);
            }

            //物流
            if (type == Service.Print.PrintTemplateItemType.DELIVERY_DELIVERYNUMBERBARCODE)
            {
                return new PrintTemplateItemViewModelForDeliveryNumberBarcode(template);
            }

            if (type == Service.Print.PrintTemplateItemType.DELIVERY_CONSOLIDATIONCODE_BARCODE)
            {
                return new PrintTemplateItemConsolidationCodeBarCode(template);
            }

            if (type == Service.Print.PrintTemplateItemType.DELIVERY_ROUTECODE)
            {
                return new PrintTemplateItemViewModelForRouteCode(template);
            }

            //其它
            if (type == Service.Print.PrintTemplateItemType.OTHER_RANDOM)
            {
                return new PrintTemplateItemViewModelForRandom(template) { PreviewValue = "DFF2-3" };
            }

            if (type == Service.Print.PrintTemplateItemType.OTHER_BARCODE)
            {
                return new PrintTemplateItemViewModelForBarcode(template);
            }

            if (type == Service.Print.PrintTemplateItemType.OTHER_STATICTEXT)
            {
                return new PrintTemplateItemViewModelForText(template);
            }

            if (type == Service.Print.PrintTemplateItemType.OTHER_IMAGE)
            {
                return new PrintTemplateItemViewModelForImage(template) { PreviewValue = "请选择图片" };
            }

            if (type == Service.Print.PrintTemplateItemType.OTHER_LINE)
            {
                return new PrintTemplateItemViewModelForLine(template);
            }

            return new PrintTemplateItemViewModelCommon(template) { PreviewValue = previewValue };
        }
    }
}