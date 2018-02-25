namespace ShopErp.Server.Service.Pop.Chuchujie
{
    public class ChuchujieGoodsResponseGoodsSku
    {
        public string sku_id;
        public string sku_code;
        public string value;
        public string sku_price;
        public string prop_id;
        public string sku_stock;
        public string sku_status;

        public override string ToString()
        {
            return sku_code + " " + value;
        }
    }
}
