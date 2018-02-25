namespace ShopErp.Domain
{
    public enum OrderType
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("正常")]
        NORMAL = 1,

        [EnumDescription("刷单")]
        SHUA = 2,
    }
}
