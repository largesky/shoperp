using ShopErp.Domain;
using System;
using System.Collections.Generic;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.App.Service.Restful
{
    public class OrderUpdateService : ServiceBase<OrderUpdate>
    {
        public DataCollectionResponse<OrderUpdate> GetByAll(long[] shopIds, string popOrderId, OrderType orderType, DateTime popPayTimeStart, DateTime popPayTimeEnd, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopIds"] = shopIds;
            para["popOrderId"] = popOrderId;
            para["orderType"] = orderType;
            para["popPayTimeStart"] = popPayTimeStart;
            para["popPayTimeEnd"] = popPayTimeEnd;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;

            return DoPost<DataCollectionResponse<OrderUpdate>>(para);
        }

        public new string Update(OrderUpdate orderUpdate)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderUpdate"] = orderUpdate;
            return DoPost<StringResponse>(para).data;
        }
    }
}