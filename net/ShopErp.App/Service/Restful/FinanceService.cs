using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;


namespace ShopErp.App.Service.Restful
{
    public class FinanceService : ServiceBase<Finance>
    {
        public DataCollectionResponse<Finance> GetByAll(string type, long accountId, string comment, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["type"] = type;
            para["accountId"] = accountId;
            para["comment"] = comment;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;

            return DoPost<DataCollectionResponse<Finance>>(para);
        }

        public void Create(string type, DateTime time, float money, long account, long account2, string comment,string opposite)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["type"] = type;
            para["time"] = time;
            para["money"] = money;
            para["account"] = account;
            para["account2"] = account2;
            para["comment"] = comment;
            para["opposite"] = opposite;
            DoPost<StringResponse>(para);
        }

    }
}