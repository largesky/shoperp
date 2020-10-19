namespace ShopErp.Domain
{
    public enum OrderState
    {
        [EnumDescription("所有")]
        NONE = 0,

        //[EnumDescription("待付款")]
        //WAITPAY = 10,

        [EnumDescription("已付款")]
        PAYED = 20,

        [EnumDescription("已打印")]
        PRINTED = 30,

        [EnumDescription("检查未过")]
        CHECKFAIL = 40,

        [EnumDescription("已拿货")]
        GETED,

        [EnumDescription("已发货")]
        SHIPPED = 50,

        [EnumDescription("已完成")]
        SUCCESS = 54,

        [EnumDescription("退货中")]
        RETURNING = 60,

        [EnumDescription("已关闭")]
        CLOSED = 64,

        [EnumDescription("已下架")]
        NOTSALE = 65,

        [EnumDescription("已拆分")]
        SPILTED = 66,
    }
}
