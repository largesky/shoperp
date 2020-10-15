using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderUpdateDao : NHibernateDaoBase<OrderUpdate>
    {
        public DataCollectionResponse<OrderUpdate> GetByAll(long[] shopIds, string popOrderId, OrderType orderType, DateTime popPayTimeStart, DateTime popPayTimeEnd, int pageIndex, int pageSize)
        {
            List<Object> objs = new List<object>();
            string hsql = "from " + this.GetEntiyName() + " where ";
            if (shopIds != null && shopIds.Length > 0)
            {
                hsql += " ShopId in (" + string.Join(",", shopIds) + " ) and ";
            }
            hsql += this.MakeQuery("PopOrderId", popOrderId, objs);
            hsql += this.MakeQuery("Type", (int)orderType, (int)OrderType.NONE);
            hsql += this.MakeQuery("PopPayTime", popPayTimeStart, true);
            hsql += this.MakeQuery("PopPayTime", popPayTimeEnd, false);

            return this.GetPage(this.TrimHSql(hsql), pageIndex, pageSize,objs.ToArray());
        }
    }
}
