using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain
{
    public enum GoodsVideoType
    {
        [EnumDescription("所有")]
        NONE = 0,

        [EnumDescription("无")]
        NOT = 1,

        [EnumDescription("视频")]
        VIDEO = 2,

        [EnumDescription("图片连播")]
        PICTURE = 3,
    }
}
