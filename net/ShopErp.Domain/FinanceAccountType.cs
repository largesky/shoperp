using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public enum FinanceAccountType
    {
        [EnumDescription("所有")]
        None = 0,

        [EnumDescription("现金")]
        CASH,

        [EnumDescription("银行卡")]
        BANK,

        [EnumDescription("支付宝")]
        ALIPAY,

        [EnumDescription("微信")]
        WEIXIN,
    }
}
