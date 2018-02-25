namespace ShopErp.Domain
{
    public enum OrderReturnType
    {
        [EnumDescription("所有")]
        NONE = 0,
        [EnumDescription("退货")]
        RETURN,
        [EnumDescription("换货")]
        EXCHANGE,
        [EnumDescription("拒收")]
        REFUSED,
        [EnumDescription("无单")]
        NONEORDER
    }
}
