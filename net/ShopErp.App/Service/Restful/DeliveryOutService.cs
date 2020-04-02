using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;

namespace ShopErp.App.Service.Restful
{
    public class DeliveryOutService : ServiceBase<DeliveryOut>
    {
        public DataCollectionResponse<DeliveryOut> GetByAll(PopPayType payType, long shopId, string deliveryCompany,
            string deliveryNumber, string vendor, string number, string shipper, DateTime startTime, DateTime endTime, int pageIndex,
            int pageSize)
        {
            System.Collections.Generic.Dictionary<string, object> para = new System.Collections.Generic.Dictionary<string, object>();
            para["payType"] = payType;
            para["shopId"] = shopId;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["vendor"] = vendor;
            para["number"] = number;
            para["shipper"] = shipper;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<DeliveryOut>>(para);
        }

        public ResponseBase DeleteOrderDeliveryOut(string deliveryNumber)
        {
            System.Collections.Generic.Dictionary<string, object> para =
                new System.Collections.Generic.Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            return DoPost<ResponseBase>(para);
        }
    }
}