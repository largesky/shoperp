using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddReqWaybillGetTradeOrderInfoDto
    {
        public string object_id;

        public string user_id;

        public string template_url;

        public PddReqWaybillGetTradeOrderInfoDtoOrderInfo order_info;

        public PddReqWaybillGetTradeOrderInfoDtoPackageInfo package_info;

        public PddReqWaybillGetTradeOrderInfoDtoRecipient recipient;
    }
}
