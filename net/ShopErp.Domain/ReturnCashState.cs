namespace ShopErp.Domain
{
    public enum ReturnCashState
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("待处理")]
        WAIT_PROCESS = 1,

        [EnumDescription("处理失败")]
        PROCESS_FAIL = 2,

        [EnumDescription("已完成")]
        COMPLETED = 3,
    }
}
