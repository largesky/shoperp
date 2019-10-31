using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop.Pdd
{
    class PddRspWaybillSearchDeliveryCompanyAccount
    {

        public string branch_code;

        public string branch_name;

        public long quantity;

        public PddRspWaybillSearchDeliveryCompanyAccountAddress[] shipp_address_cols;
    }
}
