namespace ShopErp.Domain
{
    public enum PopType
    {
        [EnumDescription("所有")]
        None,

        [EnumDescription("淘宝")]
        TAOBAO = 1,

        [EnumDescription("蘑菇街")]
        MOGUJIE = 3,

        [EnumDescription("美丽说")]
        MEILISHUO = 4,

        [EnumDescription("京东")]
        JINGDONG = 5,

        [EnumDescription("天猫")]
        TMALL = 6,

        [EnumDescription("阿里巴巴")]
        ALIBABA = 7,

        [EnumDescription("微店")]
        WEIDIAN = 8,

        [EnumDescription("天猫分销")]
        TFENGXIAO = 9,

        [EnumDescription("楚楚街")]
        CHUCHUJIE = 10,

        [EnumDescription("拼多多")]
        PINGDUODUO = 11,

        [EnumDescription("楚楚街拼划算")]
        CHUCHUJIE_PING = 12,

        [EnumDescription("楚楚街预售")]
        CHUCHUJIE_YUSHOU = 13,

        [EnumDescription("其它")]
        OTHER = 99999
    }
}
