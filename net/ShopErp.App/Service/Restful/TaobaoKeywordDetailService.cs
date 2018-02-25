using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class TaobaoKeywordDetailService : ServiceBase<TaobaoKeywordDetail>
    {
        public DataCollectionResponse<TaobaoKeywordDetail> GetByAll(string number, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();

            para["number"] = number;
            para["start"] = start;
            para["end"] = end;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<TaobaoKeywordDetail>>(para);
        }

        public void SaveMulti(TaobaoKeywordDetail[] values)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["values"] = values;
            DoPost<ResponseBase>(para);
        }
    }
}
