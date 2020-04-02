using System;
using System.Collections.Generic;
using NHibernate;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class DeliveryOutDao : NHibernateDaoBase<DeliveryOut>
    {
        public DataCollectionResponse<DeliveryOut> GetByAll(PopPayType payType, int shopId, string deliveryCompany, string deliveryNumber, string vendor, string number, string shipper, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<Object> objs = new List<object>();

            hsql += this.MakeQuery("PopPayType", (int)payType, (int)PopPayType.None);
            hsql += this.MakeQuery("ShopId", shopId);
            hsql += this.MakeQuery("DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("CreateTime", startTime, true);
            hsql += this.MakeQuery("CreateTime", endTime, false);
            hsql += this.MakeQueryLike("GoodsInfo", vendor, objs);
            hsql += this.MakeQueryLike("GoodsInfo", number, objs);
            hsql += this.MakeQueryLike("Shipper", shipper, objs);
            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }

        public void DeleteOrderDeliveryOut(string deliveryNumber)
        {
            string hsql = "delete from " + this.GetEntiyName() + " where DeliveryNumber='" + deliveryNumber + "'";
            ISession session = this.OpenSession();
            try
            {
                var query = session.CreateQuery(hsql);
                query.ExecuteUpdate();
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }
    }
}
