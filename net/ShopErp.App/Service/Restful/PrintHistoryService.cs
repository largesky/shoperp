using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;

namespace ShopErp.App.Service.Restful
{
    public class PrintHistoryService : ServiceBase<PrintHistory>
    {
        public DataCollectionResponse<PrintHistory> GetByAll(long orderId, string deliveryCompany, string deliveryNumber, WuliuPrintTemplateSourceType deliverySourceType, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            System.Collections.Generic.Dictionary<string, object> para = new System.Collections.Generic.Dictionary<string, object>();
            para["orderId"] = orderId;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["deliverySourceType"] = deliverySourceType;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<PrintHistory>>(para);
        }

        public ResponseBase Upload(PrintHistory ph)
        {
            System.Collections.Generic.Dictionary<string, object> para =
                new System.Collections.Generic.Dictionary<string, object>();
            para["ph"] = ph;
            return DoPost<DataCollectionResponse<ResponseBase>>(para);
        }

    }
}