using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TaobaoQueryGoodsListResponseDataTableDataSource
    {
        public string itemId;

        public string catId;

        public int soldQuantity_m;

        public TaobaoQueryGoodsListResponseDataTableDataSourceItemDesc itemDesc;

        public TaobaoQueryGoodsListResponseDataTableDataSourceManagerPrice managerPrice;

        public TaobaoQueryGoodsListResponseDataTableDataSourceUpdateInfo upShelfDate_m;

    }
}
