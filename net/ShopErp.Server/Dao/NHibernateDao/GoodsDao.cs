using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao

{
    public class GoodsDao : NHibernateDaoBase<Goods>
    {
        static readonly string[] TIME_TYPES = { "CreateTime", "ProcessImageTime", "UploadTime" };

        public DataCollectionResponse<Goods> GetByAll(long shopId, GoodsState state, int timeType, DateTime start, DateTime end, string vendor, string number, GoodsType type, string comment, ColorFlag flag, GoodsVideoType videoType, string order, int pageIndex, int pageSize)
        {
            List<object> para = new List<object>();
            string dataHsqlHeader = " select distinct  GU  from " + this.GetEntiyName() + " as  GU left join fetch GU.Vendor as V left join  GU.Shops as GUS ";
            //对于count统计sql语句left join 不能使用 fetch，不然会产生错误的sql语句，！！！！！
            string countHsqlHeader = " select count( distinct GU.Id)  from " + this.GetEntiyName() + " as  GU left join GU.Vendor as V left join GU.Shops as GUS  ";
            string where = "";

            //店铺查询条件
            if (shopId <= -1)
            {
                where += "  GU.Id not in (select distinct GoodsId from GUS group by GoodsId having count(GoodsId) >=1 ) and ";
            }
            else if (shopId > 0)
            {
                where += " GUS.ShopId=" + shopId + " and ";
            }
            where += this.MakeQuery("GUS.State", (int)state, (int)GoodsState.NONE);
            where += this.MakeQuery("GU.Type", (int)type, (int)GoodsType.GOODS_SHOES_NONE);
            if (timeType <= 0)
            {
                where += this.MakeQuery("GU." + TIME_TYPES[timeType], start, true);
                where += this.MakeQuery("GU." + TIME_TYPES[timeType], end, false);
            }
            else
            {
                where += this.MakeQuery("GUS." + TIME_TYPES[timeType], start, true);
                where += this.MakeQuery("GUS." + TIME_TYPES[timeType], end, false);
            }

            if (string.IsNullOrWhiteSpace(vendor) == false)
            {
                where += " (V.Name like ? or V.PingyingName like ?) and ";
                para.Add('%' + vendor + '%');
                para.Add('%' + vendor + '%');
            }
            where += this.MakeQueryLike("GU.Number", number, para);
            where += this.MakeQueryLike("GU.Comment", comment, para);
            where += this.MakeQuery("GU.Flag", (int)flag, (int)ColorFlag.None);
            where += this.MakeQuery("GU.VideoType", (int)videoType, (int)GoodsVideoType.NONE);
            string dataHsql = dataHsqlHeader + this.TrimHSql("where " + where) + " order by ";
            if (string.IsNullOrWhiteSpace(order))
            {
                dataHsql += " GU.Id desc ";
            }
            else if (order.Contains("State"))
            {
                dataHsql += " GUS.State " + (order.Contains("asc") ? "asc " : "desc");
            }
            else if (order.Contains("UploadTime"))
            {
                dataHsql += " GUS.UploadTime " + (order.Contains("asc") ? "asc " : "desc");
            }
            else if (order.Contains("Vendor"))
            {
                dataHsql += order.Contains("Vendor") ? (" V.Name " + (order.Contains("asc") ? "asc " : "desc")) : ("GU." + order);
            }
            else
            {
                dataHsql += "GU." + order;
            }
            return this.GetPageEx(dataHsql, this.TrimHSql(countHsqlHeader + " where " + where), pageIndex, pageSize, para.ToArray());
        }

        public DataCollectionResponse<Goods> GetByNumberAndVendorNameLike(string number, string vendorNameOrPingName, int pageIndex, int pageSize)
        {
            List<object> para = new List<object>();
            string hsql = "from " + this.GetEntiyName() + " GU0,Vendor V0 where GU0.VendorId=V0.Id and ";

            if (string.IsNullOrWhiteSpace(vendorNameOrPingName) == false)
            {
                hsql += " (V0.Name like ? or V0.PingyingName like ?) and ";
                para.Add('%' + vendorNameOrPingName + '%');
                para.Add('%' + vendorNameOrPingName + '%');
            }
            hsql += this.MakeQuery("GU0.Number", number, para);
            return this.GetPageEx(this.TrimHSql("select GU0 " + hsql) + " order by GU0.Id desc", this.TrimHSql("select count(GU0.Id) " + hsql), pageIndex, pageSize, para.ToArray());

        }

        public void UpdateWeight(long id, float weight)
        {
            this.ExcuteSqlUpdate(string.Format("update `Goods`  set Weight={0} where Id={1}", weight.ToString("F2"), id));
        }

        public void UpdateLastSellTime(long id, DateTime time)
        {
            this.ExcuteSqlUpdate(string.Format("update `Goods` set LastSellTime='{0}' where Id={1}", this.FormatDateTime(time), id));
        }

    }
}
