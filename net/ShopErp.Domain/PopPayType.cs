namespace ShopErp.Domain
{
    public enum PopPayType
    {
        [EnumDescription("所有类型")]
        None,
        [EnumDescription("在线支付")]
        ONLINE = 1,
        [EnumDescription("货到付款")]
        COD
    }
}
