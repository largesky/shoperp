using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public enum WuliuPrintTemplateSourceType
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("自研")]
        SELF = 1,

        [EnumDescription("菜鸟")]
        CAINIAO = 2,

        [EnumDescription("拼多多")]
        PINDUODUO = 3,
    }
}
