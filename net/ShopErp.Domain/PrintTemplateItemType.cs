using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.Domain
{
    public class PrintTemplateItemType
    {
        private static Dictionary<string, PrintTemplateItemTypeGroup> typeGroups = new Dictionary<string, PrintTemplateItemTypeGroup>();

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.SHOP)]
        public const string SHOP_MARK = "店铺标识";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.SHOP)]
        public const string SHOP_SHOPIMAGE = "店铺图片";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.GOODS)]
        public const string GOODS_NUMBER = "商品货号";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.GOODS)]
        public const string GOODS_COLOR = "商品颜色";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.GOODS)]
        public const string GOODS_SIZE = "商品尺码";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.GOODS)]
        public const string GOODS_MEATERIAL = "商品材质";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.RETURN)]
        public const string RETURN_ORDERCHANDE_ID_TEXT = "退货编号（文本）";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.RETURN)]
        public const string RETURN_ORDERCHANDE_ID_BARCODE = "退货编号（条码）";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.RETURN)]
        public const string RETURN_VENDOR = "退货厂家";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.RETURN)]
        public const string RETURN_VENDORDOOR = "退货厂家门牌";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.RETURN)]
        public const string RETURN_GOODSINFO = "退货商品信息";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.PRINT)]
        public const string PRINT_PAGENUMBER = "打印页码";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.PRINT)]
        public const string PRINT_DATETIME = "打印时间";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.OTHER)]
        public const string OTHER_STATICTEXT = "自定义文本";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.OTHER)]
        public const string OTHER_BARCODE = "条形码";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.OTHER)]
        public const string OTHER_IMAGE = "自定义图片";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.OTHER)]
        public const string OTHER_LINE = "自定义线条";

        [PrintTemplateItemTypeGroup(PrintTemplateItemTypeGroup.OTHER)]
        public const string OTHER_RANDOM = "随机序列";


        public static PrintTemplateItemTypeGroup GetGroup(string type)
        {
            if (typeGroups.Count < 1)
            {
                lock (typeGroups)
                {
                    if (typeGroups.Count < 1)
                    {
                        var ff = typeof(PrintTemplateItemType).GetFields().Where(f => f.FieldType == typeof(string)).ToArray();
                        foreach (var f in ff)
                        {
                            var c = f.GetCustomAttributes(typeof(PrintTemplateItemTypeGroupAttribute), true);
                            if (c != null && c.Length > 0)
                            {
                                typeGroups[f.GetRawConstantValue().ToString()] = ((PrintTemplateItemTypeGroupAttribute)c[0]).Group;
                            }
                        }
                    }
                }
            }

            if (typeGroups.ContainsKey(type) == false)
            {
                throw new Exception("指定的类型:" + type + "不存在打印类型枚举中");
            }

            return typeGroups[type];
        }
    }
}
