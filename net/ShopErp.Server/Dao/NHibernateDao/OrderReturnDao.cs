using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderReturnDao : NHibernateDaoBase<OrderReturn>
    {
        static readonly string[] TIME_TYPES = new string[] { "CreateTime", "ProcessTime", };

        public DataCollectionResponse<OrderReturn> GetByAll(long id, long orderId, string vendor, string number, string deliveryNumber, OrderReturnState state, OrderReturnType type, int timeType, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQuery("Id", id);
            hsql += this.MakeQuery("OrderId", orderId);
            hsql += this.MakeQueryLike("GoodsInfo", vendor, objs);
            hsql += this.MakeQueryLike("GoodsInfo", number, objs);
            hsql += this.MakeQuery("DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery(TIME_TYPES[timeType], start, true);
            hsql += this.MakeQuery(TIME_TYPES[timeType], end, false);
            hsql += this.MakeQuery("State", (int)state, (int)OrderReturnState.NONE);
            hsql += this.MakeQuery("Type", (int)type, (int)OrderReturnType.NONE);
            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }


        public DataCollectionResponse<OrderReturn> GetByOrderIdAndOrderGoodsId(long orderId, long orderGoodsId)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            hsql += this.MakeQuery("OrderId", orderId);
            hsql += this.MakeQuery("OrderGoodsId", orderGoodsId);

            return this.GetPage(hsql, 0, 0);
        }
    }
}
