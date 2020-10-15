using Microsoft.Win32;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderDao : NHibernateDaoBase<Order>
    {

        public DataCollectionResponse<Order> GetByAll(string popBuyerId, string receiverMobile,
            string receiverName, string receiverAddress, DateTime startTime, DateTime endTime, string deliveryCompany, string deliveryNumber,
            OrderState state, PopPayType payType, string vendorName, string number, string size,
            ColorFlag[] ofs, int parseResult, string comment, long shopId, OrderCreateType createType, OrderType type, string shipper,
            int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " as O0 left join O0.OrderGoodss as OG0 where ";
            List<object> objs = new List<object>();

            if (Utils.DateTimeUtil.IsDbMinTime(startTime))
            {
                startTime = DateTime.Now.AddDays(-30);
            }

            hsql += this.MakeQueryLike("O0.PopBuyerId", popBuyerId, objs);
            hsql += this.MakeQueryLike("O0.ReceiverMobile", receiverMobile, objs);
            hsql += this.MakeQuery("O0.ReceiverName", receiverName, objs);
            hsql += this.MakeQueryLike("O0.ReceiverAddress", receiverAddress, objs);
            hsql += this.MakeQuery("O0.PopPayTime", startTime, true);
            hsql += this.MakeQuery("O0.PopPayTime", endTime, false);
            hsql += this.MakeQuery("O0.DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("O0.DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("O0.State", (int)state);
            hsql += this.MakeQuery("O0.PopPayType", (int)payType);
            hsql += this.MakeQueryLike("Vendor", vendorName, objs);
            hsql += this.MakeQueryLike("Number", number, objs);
            hsql += this.MakeQueryLike("Size", size, objs);
            hsql += this.MakeQueryLike("Shipper", shipper, objs);
            if (ofs != null && ofs.Length > 0)
            {
                hsql += " O0.PopFlag in (" + string.Join(",", ofs.Select(obj => (int)obj)) + ")  and ";
            }
            hsql += this.MakeQuery("O0.ParseResult", parseResult, -1);
            hsql += this.MakeQueryLike("O0.PopSellerComment", comment, objs);
            hsql += this.MakeQuery("O0.ShopId", shopId);
            hsql += this.MakeQuery("O0.CreateType", (int)createType, (int)OrderCreateType.NONE);
            hsql += this.MakeQuery("O0.Type", (int)type, (int)OrderType.NONE);
            hsql = this.TrimHSql(hsql);
            return this.GetPageEx("select distinct O0 " + hsql + " order by O0.PopPayTime desc", "select count( distinct O0.Id) " + hsql, pageIndex, pageSize, objs.ToArray());
        }

        public DataCollectionResponse<Order> GetPayedAndPrintedOrders(long[] shopId, OrderCreateType createType, PopPayType payType, string shipper, int pageIndex, int pageSize)
        {
            List<object> objs = new List<object>();
            string hsql = "from " + this.GetEntiyName() + " as O0 left join O0.OrderGoodss as OG0 where (O0.State = " + (int)OrderState.PAYED + " or O0.State = " + (int)OrderState.PRINTED + ")  and ";
            if (shopId != null && shopId.Length > 0)
            {
                hsql += " ShopId In (" + string.Join(",", shopId.Select(o => o.ToString())) + ") and ";
            }
            hsql += this.MakeQuery("CreateType", (int)createType, (int)OrderCreateType.NONE);
            hsql += this.MakeQuery("PopPayType", (int)payType, (int)PopPayType.None);
            hsql += this.MakeQuery("Shipper", shipper, objs);
            hsql = this.TrimHSql(hsql);
            return this.GetPageEx("select distinct O0 " + hsql + " order by O0.PopPayTime desc", "select count( distinct O0.Id) " + hsql, pageIndex, pageSize, objs.ToArray());
        }

        public DataCollectionResponse<Order> GetOrdersByInfoIDNotEqual(string popBuyerId, string receiverPhone, string receiverMobile, string receiverAddress, long id)
        {
            string hsql = "from " + this.GetEntiyName() + " where Id<>" + id + " and (";
            List<object> objs = new List<object>();

            if (string.IsNullOrWhiteSpace(popBuyerId) == false)
            {
                hsql += "PopBuyerId=? or ";
                objs.Add(popBuyerId);
            }

            if (string.IsNullOrWhiteSpace(receiverPhone) == false)
            {
                hsql += "ReceiverPhone=? or ";
                objs.Add(receiverPhone);
            }

            if (string.IsNullOrWhiteSpace(receiverMobile) == false)
            {
                hsql += "ReceiverMobile=? or ";
                objs.Add(receiverMobile);
            }

            if (string.IsNullOrWhiteSpace(receiverAddress) == false)
            {
                hsql += "ReceiverAddress=? or ";
                objs.Add(receiverAddress);
            }
            hsql = hsql.Substring(0, hsql.Length - 3);
            hsql += ")";

            var ret = this.GetPage(hsql, 0, 0, objs.ToArray());

            return ret;
        }

        public void UpdateOrderState(long orderId, OrderState state)
        {
            var s = this.OpenSession();
            try
            {
                var query = s.CreateSQLQuery(string.Format("update OrderGoods set State={0} where OrderId={1} and  State<{2}", (int)state, orderId, (int)OrderState.CLOSED));
                int ret = query.ExecuteUpdate();
                query = s.CreateSQLQuery((string.Format("update `Order` set State={0} where Id={1} ", (int)state, orderId)));
                ret = query.ExecuteUpdate();
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }
        }
    }
}
