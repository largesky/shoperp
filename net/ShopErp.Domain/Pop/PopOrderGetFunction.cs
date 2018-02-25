namespace ShopErp.Domain.Pop
{
    public enum PopOrderGetFunction
    {
        /// <summary>
        /// 总是可以根据订单编号获取订单所有信息，
        /// 如果淘宝
        /// </summary>
        ALWAYS = 1,

        /// <summary>
        /// 只有在已付款主，待发货情况可以根据订单编号获取订单所有信息，
        /// 如果拼多多 
        /// </summary>
        PAYED = 2,
    }
}
