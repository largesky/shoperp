using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    class TaobaoQueryOrderDetailResponseOperationsGuideLineTypeValue
    {
        public string type;

        [JsonConverter(typeof(TaobaoQueryOrderDetailResponseOperationsGuideLineTypeValueConvert))]
        public string value;
    }
}
