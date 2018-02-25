namespace ShopErp.Domain
{
    public enum GoodsState
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("待处图")]
        WAITPROCESSIMAGE = 1,

        [EnumDescription("待审核")]
        WAITREVIEW = 2,

        [EnumDescription("待上货")]
        WAITUPLOADED = 3,

        [EnumDescription("已上传")]
        UPLOADED = 4,

        [EnumDescription("已下架")]
        NOTSALE = 5
    }
}
