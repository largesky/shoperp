namespace ShopErp.Domain
{
    public enum GoodsType
    {
        [EnumDescription("所有")]
        GOODS_SHOES_NONE = 0,
        [EnumDescription("其它")]
        GOODS_SHOES_OTHER = 1,
        [EnumDescription("凉鞋")]
        GOODS_SHOES_LIANGXIE,
        [EnumDescription("低帮鞋")]
        GOODS_SHOES_DIBANGXIE,
        [EnumDescription("拖鞋")]
        GOODS_SHOES_TUOXIE,
        [EnumDescription("高帮鞋")]
        GOODS_SHOES_GAOBANGXIE,
        [EnumDescription("靴子")]
        GOODS_SHOES_XUEZI,
        [EnumDescription("男鞋")]
        GOODS_SHOES_NANXIE,
        [EnumDescription("凡布鞋")]
        GOODS_SHOES_FANBUXIE,
        [EnumDescription("雨鞋")]
        GOODS_SHOES_YUXIE,
    }
}
