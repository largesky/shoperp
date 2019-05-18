using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderDao : NHibernateDaoBase<Order>
    {
        static readonly string[] TIME_TYPES = { "PopCreateTime", "PopPayTime", "PopDeliveryTime", "CreateTime", "PrintTime", "DeliveryTime", "CloseTime" };

        private string GetTimeType(int timeType)
        {
            if (timeType > TIME_TYPES.Length)
            {
                throw new Exception("未知的时间类型");
            }
            return TIME_TYPES[timeType];
        }

        public DataCollectionResponse<Order> GetByAll(string popBuyerId, string receiverPhone, string receiverMobile, string receiverName, string receiverAddress,
            int timeType, DateTime startTime, DateTime endTime, string deliveryCompany, string deliveryNumber,
            OrderState state, PopPayType payType, string vendorName, string number,
            ColorFlag[] ofs, int parseResult, string comment, long shopId, OrderCreateType createType, OrderType type, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " as O0 left join O0.OrderGoodss as OG0 where ";
            List<object> objs = new List<object>();

            if (this.IsLessDBMinDate(startTime))
            {
                startTime = DateTime.Now.AddDays(-45);
            }

            hsql += this.MakeQueryLike("O0.PopBuyerId", popBuyerId, objs);
            hsql += this.MakeQueryLike("O0.ReceiverPhone", receiverPhone, objs);
            hsql += this.MakeQueryLike("O0.ReceiverMobile", receiverMobile, objs);
            hsql += this.MakeQuery("O0.ReceiverName", receiverName, objs);
            hsql += this.MakeQueryLike("O0.ReceiverAddress", receiverAddress, objs);
            hsql += this.MakeQuery("O0." + this.GetTimeType(timeType), startTime, true);
            hsql += this.MakeQuery("O0." + this.GetTimeType(timeType), endTime, false);
            hsql += this.MakeQuery("O0.DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("O0.DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("O0.State", (int)state);
            hsql += this.MakeQuery("O0.PopPayType", (int)payType);
            hsql += this.MakeQueryLike("OG0.Vendor", vendorName, objs);
            hsql += this.MakeQueryLike("OG0.Number", number, objs);
            if (ofs != null && ofs.Length > 0)
            {
                hsql += "(";
                hsql += string.Join(" or ", (ofs).Select(obj => "O0.PopFlag=" + (int)obj));
                hsql += ") and ";
            }
            hsql += this.MakeQuery("O0.ParseResult", parseResult, -1);
            hsql += this.MakeQueryLike("O0.PopSellerComment", comment, objs);
            hsql += this.MakeQuery("O0.ShopId", shopId);
            hsql += this.MakeQuery("O0.CreateType", (int)createType, (int)OrderCreateType.NONE);
            hsql += this.MakeQuery("O0.Type", (int)type, (int)OrderType.NONE);
            return this.GetPageEx("select distinct O0 " + this.TrimHSql(hsql) + " order by O0.PopPayTime desc", "select count( distinct O0.Id) " + this.TrimHSql(hsql).Replace("fetch", ""), pageIndex, pageSize, objs.ToArray());
        }

        public DataCollectionResponse<Order> GetPayedAndPrintedOrders(long[] shopId, OrderCreateType createType, PopPayType payType, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where  (State=" + (int)OrderState.PAYED + " or State=" + (int)OrderState.PRINTED + ")  and ";
            if (shopId != null && shopId.Length > 0)
            {
                hsql += " ShopId In (" + string.Join(",", shopId.Select(o => o.ToString())) + ") and ";
            }
            hsql += this.MakeQuery("CreateType", (int)createType, (int)OrderCreateType.NONE);
            hsql += this.MakeQuery("PopPayType", (int)payType, (int)PopPayType.None);
            return this.GetPage(hsql, pageIndex, pageSize);
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
    }
}
