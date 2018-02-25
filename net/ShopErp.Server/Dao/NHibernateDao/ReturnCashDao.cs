using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class ReturnCashDao : NHibernateDaoBase<ReturnCash>
    {
        readonly string[] TIME_TYPES = { "CreateTime", "ProcessTime" };

        public DataCollectionResponse<ReturnCash> GetByAll(long shopId, string popOrderId, string type, string accountInfo, int timeType, DateTime startTime, DateTime endTime, ReturnCashState state, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();
            hsql += this.MakeQuery("ShopId", shopId);
            hsql += this.MakeQuery("PopOrderId", popOrderId, objs);
            hsql += this.MakeQuery("AccountInfo", accountInfo, objs);
            hsql += this.MakeQuery("Type", type, objs);
            hsql += this.MakeQuery(TIME_TYPES[timeType], startTime, true);
            hsql += this.MakeQuery(TIME_TYPES[timeType], endTime, false);
            hsql += this.MakeQuery("State", (int)state, (int)ReturnCashState.NONE);
            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }
    }
}
