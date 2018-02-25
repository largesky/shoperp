using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ShopErp.App.Service.Restful
{
    public class DeliveryInService : ServiceBase<DeliveryIn>
    {
        public DataCollectionResponse<DeliveryIn> GetByAll(string deliveryCompany, string deliveryNumber, DateTime startTime,
            DateTime endTime, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<DeliveryIn>>(para);
        }
    }
}