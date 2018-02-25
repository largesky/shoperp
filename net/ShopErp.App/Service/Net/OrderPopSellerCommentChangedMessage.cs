using System;
using ShopErp.Domain;

namespace ShopErp.App.Service.Net
{
    [Serializable]
    public class OrderPopSellerCommentChangedMessage : Message
    {
        public long OrderId { get; set; }

        public string SellerComment { get; set; }

        public ColorFlag Flag { get; set; }
    }
}