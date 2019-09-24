using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Order
{
    class TaobaoQueryOrderDetailResponseOrderInfoLineContentNameValue
    {
        public string name;

        [JsonConverter(typeof(TaobaoQueryOrderDetailResponseOperationsGuideLineTypeValueConvert))]
        public string value;
    }
}
