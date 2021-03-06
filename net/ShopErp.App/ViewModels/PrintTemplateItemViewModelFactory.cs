﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.App.Domain;
using ShopErp.App.Service.Print;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    class PrintTemplateItemViewModelFactory
    {
        public static PrintTemplateItemViewModelCommon Create(PrintTemplate template, string type, string previewValue)
        {
            //退货类型
            if (type == PrintTemplateItemType.RETURN_ORDERCHANDE_ID_BARCODE)
            {
                return new PrintTemplateItemViewModelForBarcode(template);
            }

            //其它
            if (type == PrintTemplateItemType.OTHER_RANDOM)
            {
                return new PrintTemplateItemViewModelForRandom(template) { PreviewValue = "DFF2-3" };
            }

            if (type == PrintTemplateItemType.OTHER_BARCODE)
            {
                return new PrintTemplateItemViewModelForBarcode(template);
            }

            if (type == PrintTemplateItemType.OTHER_STATICTEXT)
            {
                return new PrintTemplateItemViewModelForText(template);
            }

            if (type == PrintTemplateItemType.OTHER_IMAGE)
            {
                return new PrintTemplateItemViewModelForImage(template) { PreviewValue = "请选择图片" };
            }

            if (type == PrintTemplateItemType.OTHER_LINE)
            {
                return new PrintTemplateItemViewModelForLine(template);
            }

            return new PrintTemplateItemViewModelCommon(template) { PreviewValue = previewValue };
        }
    }
}