namespace ShopErp.Domain
{
    public enum OrderState
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("待付款")]
        WAITPAY = 10,

        [EnumDescription("已付款")]
        PAYED = 20,

        [EnumDescription("已打印")]
        PRINTED = 30,

        //[EnumDescription("备货中")]
        //STOCKING = 40,

        //[EnumDescription("未拿到货")]
        //OUTOFSTOCK,

        //[EnumDescription("检查未过")]
        //CHECKFAIL,

        //[EnumDescription("定做中")]
        //CUSTOMIZING,

        [EnumDescription("已拿货")]
        GETED = 40,

        [EnumDescription("检查未过")]
        CHECKFAIL,

        //[EnumDescription("下架")]
        //NOTSALE,

        [EnumDescription("已发货")]
        SHIPPED = 50,

        //[EnumDescription("已签收")]
        //SIGNED,

        //[EnumDescription("已拒签")]
        //REFUSED,

        //[EnumDescription("返款中")]
        //SYS_PAYING,

        [EnumDescription("已完成")]
        SUCCESS = 54,

        [EnumDescription("退货中")]
        RETURNING = 60,

        //[EnumDescription("待退款")]
        //WAIT_REFUNED=61,

        [EnumDescription("已拆分")]
        SPILTED = 62,

        [EnumDescription("已取消")]
        CANCLED = 63,

        [EnumDescription("已关闭")]
        CLOSED = 64,
    }
}
