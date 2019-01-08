using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OrderUpdateDao : NHibernateDaoBase<OrderUpdate>
    {
        public DataCollectionResponse<OrderUpdate> GetByAll(long[] shopIds, string popOrderId, DateTime popPayTimeStart, DateTime popPayTimeEnd, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where PopPayTime>='" + this.FormatDateTime(popPayTimeStart) + "' and PopPayTime<='" + this.FormatDateTime(popPayTimeEnd) + "' and ";
            if (shopIds != null && shopIds.Length > 0)
            {
                hsql += " ShopId in (" + string.Join(",", shopIds) + " ) and ";
            }
            if (string.IsNullOrWhiteSpace(popOrderId) == false)
            {
                hsql += " PopOrderId='" + popOrderId + "'";
            }
            else
            {
                hsql += " PopOrderId !=''";
            }
            return this.GetPage(this.TrimHSql(hsql), pageIndex, pageSize);
        }


        public void UpdateOrderGoodsStateByOrderId(long orderId, OrderState state)
        {
            var s = this.OpenSession();
            try
            {
                var query = s.CreateSQLQuery(string.Format("update OrderGoods set State={0} where OrderId={1}", (int)state, orderId));
                int ret = query.ExecuteUpdate();
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }
        }

        public void UpdateEx(OrderUpdate ou, bool updateState)
        {
            if (ou == null)
            {
                throw new Exception("要更新的订单不能为空");
            }

            string sql = string.Format("update `Order` set PopCodSevFee={0},PopOrderTotalMoney={1},PopState='{2}',PopCodNumber='{3}',PopPayTime='{4}'",
                ou.PopCodSevFee.ToString("F2"), ou.PopOrderTotalMoney.ToString("F2"), ou.PopState, ou.PopCodNumber, this.FormatDateTime(ou.PopPayTime));
            if (updateState)
                sql += ",State=" + (int)ou.State;
            sql += " where Id=" + ou.Id;
            ExcuteSqlUpdate(sql);
        }

        public void UpdateOrderGoodsState(long orderGoodsId, OrderState state)
        {
            var s = this.OpenSession();
            try
            {
                var query = s.CreateSQLQuery(string.Format("update OrderGoods set State={0} where Id=", orderGoodsId, (int)state));
                int ret = query.ExecuteUpdate();
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
