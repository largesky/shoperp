using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddReqSku
    {
        public string thumb_url;

        public string spec_id_list;

        public string out_sku_sn;

        public long multi_price;

        public long price;

        public long limit_quantity = 999;

        public int is_onsale = 1;

        public long weight = 0;

        public long quantity = 50;
    }
}
