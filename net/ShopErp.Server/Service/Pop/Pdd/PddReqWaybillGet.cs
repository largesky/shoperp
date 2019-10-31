using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddReqWaybillGet
    {
        public bool need_encrypt = true;

        public string wp_code;

        public PddReqWaybillGetSender sender;

        public PddReqWaybillGetTradeOrderInfoDto[] trade_order_info_dtos;
    }
}
