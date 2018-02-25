namespace ShopErp.Domain
{
    public enum OrderCreateType
    {
        [EnumDescription("所有")]
        NONE=0,

        [EnumDescription("下载")]
        DOWNLOAD = 1,

        [EnumDescription("手动")]
        MANUAL = 2,
    }
}
