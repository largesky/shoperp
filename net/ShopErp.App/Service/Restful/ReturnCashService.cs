using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;

namespace ShopErp.App.Service.Restful
{
    public class ReturnCashService : ServiceBase<ReturnCash>
    {
        public DataCollectionResponse<ReturnCash> GetByAll(long shopId, string popOrderId, string type, string accountInfo,
            int timeType, DateTime startTime, DateTime endTime, ReturnCashState state, int pageIndex, int pageSize)
        {
            System.Collections.Generic.Dictionary<string, object> para =
                new System.Collections.Generic.Dictionary<string, object>();
            para["shopId"] = shopId;
            para["popOrderId"] = popOrderId;
            para["type"] = type;
            para["accountInfo"] = accountInfo;
            para["timeType"] = timeType;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["state"] = state;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<ReturnCash>>(para);
        }
    }
}