using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.App.Service.Restful
{
    public class GoodsMapService : ServiceBase<GoodsMap>
    {
        public DataCollectionResponse<GoodsMap> GetByAll(string vendor, string number, long targetNumberId, int pageIndex,
            int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["vendor"] = vendor;
            para["number"] = number;
            para["targetNumberId"] = targetNumberId;
            para["pageIndex"] = pageSize;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<GoodsMap>>(para);
        }
    }
}