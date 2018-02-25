namespace ShopErp.Domain.Pop
{
    public class PopGoodsSku
    {
        public string Id { get; set; }

        public string Code { get; set; }

        public string Value { get; set; }

        public string Price { get; set; }

        public string PropId { get; set; }

        /// <summary>
        /// SKU数量
        /// </summary>
        public string Stock { get; set; }

        public string Status { get; set; }
    }
}
