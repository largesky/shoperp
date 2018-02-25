using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class DeliveryInDao : NHibernateDaoBase<DeliveryIn>
    {
        public DataCollectionResponse<DeliveryIn> GetByAll(string deliveryCompany, string deliveryNumber, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<Object> objs = new List<object>();

            hsql += this.MakeQuery("DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("CreateTime", startTime, true);
            hsql += this.MakeQuery("CreateTime", endTime, false);

            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }
    }
}
