using System;

namespace ShopErp.Domain
{
    public class Shop
    {
        public long Id { get; set; }

        public PopType PopType { get; set; }

        public string PopSellerId { get; set; }

        public string PopSellerNumberId { get; set; }

        public string PopTalkId { get; set; }

        public string AppKey { get; set; }

        public string AppSecret { get; set; }

        public string AppAccessToken { get; set; }

        public string AppRefreshToken { get; set; }

        public string AppCallbackUrl { get; set; }

        public string Mark { get; set; }

        public float CommissionPer { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public string LastUpdateOperator { get; set; }

        public bool Enabled { get; set; }

        public int ShippingHours { get; set; }

        public int FirstDeliveryHours { get; set; }

        public int SecondDeliveryHours { get; set; }

        public bool AppEnabled { get; set; }

        public bool WuliuEnabled { get; set; }
    }
}
