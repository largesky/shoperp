using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NHibernate;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderGoodsDao : NHibernateDaoBase<OrderGoods>
    {
        public DataCollectionResponse<SaleCount> GetSaleCount(long shopId, OrderType type, int timeType, DateTime startTime, DateTime endTime, string popNumberId, int pageIndex, int pageSize)
        {
            ISession session = null;
            try
            {
                String contenthsql = "select order.Id,orderGoods.Id,orderGoods.Image,orderGoods.Count,orderGoods.PopPrice,order.PopSellerGetMoney, orderGoods.Price,order.DeliveryMoney,orderGoods.Vendor,orderGoods.Number,orderGoods.NumberId,order.PopPayTime,order.DeliveryTime,order.State,order.ShopId,orderGoods.Color,orderGoods.Size,orderGoods.Edtion ";
                String hsqlWhere = String.Format(" from Order order,OrderGoods orderGoods where order.Id=orderGoods.OrderId and order.CreateType=1 ");
                hsqlWhere += string.Format(" and order.{0} >='{1}' and order.{0} <='{2}'", (timeType == 0) ? "PopPayTime" : "DeliveryTime", this.FormatDateTime(startTime), this.FormatDateTime(endTime));

                if (shopId > 0)
                {
                    hsqlWhere += " and order.ShopId=" + shopId;
                }
                if (string.IsNullOrWhiteSpace(popNumberId) == false)
                {
                    hsqlWhere += " and orderGoods.PopUrl='" + popNumberId + "'";
                }
                if (type != OrderType.NONE)
                {
                    hsqlWhere += " and order.Type=" + (int)type;
                }

                string hsqlData = contenthsql + hsqlWhere;
                string hsqlCount = "select count(orderGoods.id) " + hsqlWhere;
                session = OpenSession();
                var query = session.CreateQuery(hsqlData);
                if (pageSize > 0)
                {
                    query.SetFirstResult(pageIndex * pageSize);
                    query.SetMaxResults(pageSize);
                }
                var rs = query.List<object>();
                List<SaleCount> counts = new List<SaleCount>();
                foreach (Object list in rs)
                {
                    Object[] l = (Object[])list;
                    SaleCount gc = new SaleCount();
                    gc.OrderId = (long)l[0];
                    gc.OrderGoodsId = (long)l[1];
                    gc.Image = (string)l[2];
                    gc.Count = (int)l[3];
                    gc.PopPrice = (float)l[4];
                    gc.PopSellerGetMoney = (float)l[5];
                    gc.ERPOrderGoodsMoney = (float)l[6];
                    gc.ERPOrderDeliveryMoney = (float)l[7];
                    gc.Vendor = (string)l[8];
                    gc.Number = (string)l[9];
                    gc.NumberId = (long)l[10];
                    gc.PopPayTime = (DateTime)l[11];
                    gc.DeliveryTime = (DateTime)l[12];
                    gc.State = (OrderState)l[13];
                    gc.ShopId = (long)l[14];
                    gc.Color = (string)l[15];
                    gc.Size = (string)l[16];
                    gc.Edtion = (string)l[17];
                    counts.Add(gc);
                }
                var countQuery = session.CreateQuery(hsqlCount);
                long count = (long)(countQuery.UniqueResult());
                DataCollectionResponse<SaleCount> ret = new DataCollectionResponse<SaleCount>(counts, (int)count);
                return ret;
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }


        }

        public DataCollectionResponse<GoodsCount> GetOrderGoodsCount(ColorFlag[] flags, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            ISession session = null;
            try
            {
                string f = string.Join(" or ", flags.Select(obj => "PopFlag=" + ((int)obj).ToString()));
                if (string.IsNullOrWhiteSpace(f) == false)
                {
                    f = " and " + "(" + f + ")";
                }

                String hsqlWhere = String.Format("from Order order,OrderGoods orderGoods where order.Id=orderGoods.OrderId and order.Type<>2 and order.PopPayTime>='{0}' and order.PopPayTime<='{1}' and orderGoods.GetedCount<orderGoods.Count and (PopPayType=1 or (PopPayType=2 {2})) and order.State>={3} and order.State<{4}", this.FormatDateTime(startTime), this.FormatDateTime(endTime), f, (int)OrderState.PAYED, (int)OrderState.SHIPPED);
                String contenthsql = "select order.Id,orderGoods.Vendor,orderGoods.Number,orderGoods.Edtion,orderGoods.Color,orderGoods.Size,orderGoods.Count,orderGoods.GetedCount,orderGoods.Price,order.PopPayTime,orderGoods.State, orderGoods.NumberId,order.PopType,order.DeliveryCompany ";
                string hsqlData = contenthsql + hsqlWhere;
                string hsqlCount = "select count(orderGoods.id) " + hsqlWhere;

                Debug.WriteLine("HsqlWhere:" + hsqlWhere);
                Debug.WriteLine("HsqlData:" + hsqlData);
                Debug.WriteLine("HsqlCount:" + hsqlCount);

                session = OpenSession();
                var query = session.CreateQuery(hsqlData);
                if (pageSize > 0)
                {
                    query.SetFirstResult(pageIndex * pageSize);
                    query.SetMaxResults(pageSize);
                }
                var rs = query.List<object>();
                List<GoodsCount> counts = new List<GoodsCount>();
                foreach (Object list in rs)
                {
                    Object[] l = (Object[])list;
                    GoodsCount gc = new GoodsCount();
                    gc.OrderId = l[0].ToString();
                    gc.Vendor = l[1].ToString();
                    gc.Number = l[2].ToString();
                    gc.Edtion = l[3] == null ? "" : l[3].ToString();
                    gc.Color = l[4].ToString();
                    gc.Size = l[5].ToString();
                    gc.Count = (OrderState)l[10] == OrderState.GETED ? (int)l[6] - (int)l[7] : (int)l[6];
                    gc.Money = (float)l[8];
                    gc.FirstPayTime = (DateTime)l[9];
                    gc.State = (OrderState)l[10];
                    gc.NumberId = (long)l[11];
                    gc.PopType = (PopType)l[12];
                    if ((OrderState)l[10] != OrderState.PAYED)
                    {
                        gc.DeliveryCompany = l[13].ToString();
                    }
                    else
                    {
                        gc.DeliveryCompany = "";
                    }

                    if ((int)gc.State < (int)OrderState.PAYED || (int)gc.State >= (int)OrderState.SHIPPED)
                        continue;
                    counts.Add(gc);
                }
                var countQuery = session.CreateQuery(hsqlCount);
                long count = (long)(countQuery.UniqueResult());
                DataCollectionResponse<GoodsCount> ret = new DataCollectionResponse<GoodsCount>(counts, (int)count);
                return ret;
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
