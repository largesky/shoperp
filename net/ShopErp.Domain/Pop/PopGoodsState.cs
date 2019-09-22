namespace ShopErp.Domain.Pop
{
    public enum PopGoodsState
    {
        [EnumDescription("所有")]
        NONE = 0,
        [EnumDescription("在售")]
        ONSALE = 1,
        [EnumDescription("下架")]
        NOTSALE = 2,
    }
}
