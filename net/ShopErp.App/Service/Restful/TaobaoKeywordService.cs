using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class TaobaoKeywordService : ServiceBase<TaobaoKeyword>
    {
        public DataCollectionResponse<TaobaoKeyword> GetByAll()
        {
            return DoPost<DataCollectionResponse<TaobaoKeyword>>(null);
        }

        public ResponseBase UpdateStartAndEndTime()
        {
            return DoPost<ResponseBase>(null);
        }
    }
}
