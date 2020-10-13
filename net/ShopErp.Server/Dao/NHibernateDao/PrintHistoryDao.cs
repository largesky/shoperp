using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class PrintHistoryDao : NHibernateDaoBase<PrintHistory>
    {
        public DataCollectionResponse<PrintHistory> GetByAll(long orderId, string deliveryCompany, string deliveryNumber, WuliuPrintTemplateSourceType deliverySourceType, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQuery("OrderId", orderId);
            hsql += this.MakeQuery("DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("DeliverySourceType", (int)deliverySourceType, (int)(WuliuPrintTemplateSourceType.NONE));
            hsql += this.MakeQuery("CreateTime", startTime, true);
            hsql += this.MakeQuery("CreateTime", endTime, false);

            string query = this.TrimHSql(hsql) + " order by id asc";

            return this.GetPageEx(query, hsql, pageIndex, pageSize, objs.ToArray());
        }
    }
}
