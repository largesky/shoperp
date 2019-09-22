using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;


namespace ShopErp.App.Service.Restful
{
    public class GoodsTaskService : ServiceBase<GoodsTask>
    {

        public DataCollectionResponse<long> SaveBatch(GoodsTask[] values)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["values"] = values;
            return DoPost<DataCollectionResponse<long>>(para);
        }


        public DataCollectionResponse<GoodsTask> GetByAll(long shopId, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopId"] = shopId;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<GoodsTask>>(para);
        }
    }
}