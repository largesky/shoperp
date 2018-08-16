using System;

namespace ShopErp.App.Service.Print
{

    /// <summary>
    /// 打印分组类型
    /// </summary>
    [Flags]
    public enum PrintTemplateItemTypeGroup
    {
        /// <summary>
        /// 订单信息
        /// </summary>
        ORDER = 1,

        /// <summary>
        /// 店铺信息
        /// </summary>
        SHOP = 2,

        /// <summary>
        /// 快递
        /// </summary>
        DELIVERY = 4,

        /// <summary>
        /// 打印时的信息
        /// </summary>
        PRINT = 8,

        /// <summary>
        /// 退货
        /// </summary>
        RETURN = 16,


        /// <summary>
        /// 商品
        /// </summary>
        GOODS = 32,


        /// <summary>
        /// 其它
        /// </summary>
        OTHER = 2048,
    }
}
