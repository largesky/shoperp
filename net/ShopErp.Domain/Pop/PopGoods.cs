using System.Collections.Generic;

namespace ShopErp.Domain.Pop
{
    public class PopGoods
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Code { get; set; }

        public string AddTime { get; set; }

        public string UpdateTime { get; set; }

        public int SaleNum { get; set; }

        public string Image { get; set; }

        public string CatId { get; set; }

        public PopGoodsState State { get; set; }

        public List<PopGoodsSku> Skus { get;  set; }

        public PopGoods()
        {
            this.Skus = new List<PopGoodsSku>();
        }
    }
}
