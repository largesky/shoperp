namespace ShopErp.Domain
{
    public enum OrderReturnReason
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("7天无理由")]
        DAY7,

        [EnumDescription("商品质量问题")]
        GOODSBAD,

        [EnumDescription("发货错误")]
        GOODSWRONG,

        [EnumDescription("其它原因")]
        OTHER = 1000,
    }
}
