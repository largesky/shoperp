using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddReqWaybillUpdate
    {
        public string waybill_code;

        public string wp_code;

        public PddReqWaybillGetTradeOrderInfoDtoRecipient recipient;
    }
}
