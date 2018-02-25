namespace ShopErp.Domain
{
    public enum OrderReturnState
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("等待处理")]
        WAITPROCESS,

        [EnumDescription("已处理")]
        PROCESSED,
    }
}
