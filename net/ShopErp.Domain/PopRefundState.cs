namespace ShopErp.Domain
{
    public enum PopRefundState
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("未发生退款")]
        NOT = 1,

        [EnumDescription("已接受退款")]
        ACCEPT = 2,

        [EnumDescription("拒绝退款")]
        REJECT = 3,

        [EnumDescription("退款取消")]
        CANCEL = 4,

        [EnumDescription("退款完成")]
        OK = 5
    }
}
