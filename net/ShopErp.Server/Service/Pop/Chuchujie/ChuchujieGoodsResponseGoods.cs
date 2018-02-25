using System.Collections.Generic;

namespace ShopErp.Server.Service.Pop.Chuchujie
{
    public class ChuchujieGoodsResponseGoods
    {
        public string goods_id;
        public string goods_title;
        public string version;
        public string goods_code;
        public string add_time;
        public string update_time;
        public int sale_num;
        public string goods_url;
        public string goods_img;
        public string goods_status;
        public List<ChuchujieGoodsResponseGoodsSku> sku;
    }
}
