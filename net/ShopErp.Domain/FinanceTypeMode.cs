using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public enum FinanceTypeMode
    {
        [EnumDescription("所有")]
        None = 0,

        [EnumDescription("收入")]
        INPUT = 1,

        [EnumDescription("支出")]
        OUTPUT = 2,
    }
}
