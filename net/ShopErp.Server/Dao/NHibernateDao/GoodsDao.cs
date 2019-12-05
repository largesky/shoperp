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
            string dataHsqlHeader = " select distinct  G  from " + this.GetEntiyName() + " as  G left join fetch G.Vendor as V left join  G.Shops as GS ";
            //对于count统计sql语句left join 不能使用 fetch，不然会产生错误的sql语句，！！！！！
            string countHsqlHeader = " select count( distinct G.Id)  from " + this.GetEntiyName() + " as  G left join G.Vendor as V left join G.Shops as GS  ";
            string where = "";

            //店铺查询条件
            if (shopId <= -1)
            {
                where += "  G.Id not in (select distinct GoodsId from GS group by GoodsId having count(GoodsId) >=1 ) and ";
            }
            else if (shopId > 0)
            {
                where += " GS.ShopId=" + shopId + " and ";
            }
            where += this.MakeQuery("GS.State", (int)state, (int)GoodsState.NONE);
            where += this.MakeQuery("G.Type", (int)type, (int)GoodsType.GOODS_SHOES_NONE);
            if (timeType <= 0)
            {
                where += this.MakeQuery("G." + TIME_TYPES[timeType], start, true);
                where += this.MakeQuery("G." + TIME_TYPES[timeType], end, false);
            }
            else
            {
                where += this.MakeQuery("GS." + TIME_TYPES[timeType], start, true);
                where += this.MakeQuery("GS." + TIME_TYPES[timeType], end, false);
            }

            if (string.IsNullOrWhiteSpace(vendor) == false)
            {
                where += " (V.Name like ? or V.PingyingName like ?) and ";
                para.Add('%' + vendor + '%');
                para.Add('%' + vendor + '%');
            }
            where += this.MakeQueryLike("G.Number", number, para);
            where += this.MakeQueryLike("G.Comment", comment, para);
            where += this.MakeQuery("G.Flag", (int)flag, (int)ColorFlag.None);
            where += this.MakeQuery("G.VideoType", (int)videoType, (int)GoodsVideoType.NONE);
            string dataHsql = dataHsqlHeader + this.TrimHSql("where " + where) + " order by ";
            if (string.IsNullOrWhiteSpace(order))
            {
                dataHsql += " G.Id desc ";
            }
            else if (order.Contains("State"))
            {
                dataHsql += " GS.State " + (order.Contains("asc") ? "asc " : "desc");
            }
            else if (order.Contains("UploadTime"))
            {
                dataHsql += " GS.UploadTime " + (order.Contains("asc") ? "asc " : "desc");
            }
            else if (order.Contains("Vendor"))
            {
                dataHsql += order.Contains("Vendor") ? (" V.Name " + (order.Contains("asc") ? "asc " : "desc")) : ("G." + order);
            }
            else
            {
                dataHsql += "G." + order;
            }
            return this.GetPageEx(dataHsql, this.TrimHSql(countHsqlHeader + " where " + where), pageIndex, pageSize, para.ToArray());
        }

        public DataCollectionResponse<Goods> GetByNumberAndVendorNameLike(string number, string vendorNameOrPingName, int pageIndex, int pageSize)
        {
            List<object> para = new List<object>();
            string hsql = "from " + this.GetEntiyName() + " G0,Vendor V0 where G0.VendorId=V0.Id and ";

            if (string.IsNullOrWhiteSpace(vendorNameOrPingName) == false)
            {
                hsql += " (V0.Name like ? or V0.PingyingName like ?) and ";
                para.Add('%' + vendorNameOrPingName + '%');
                para.Add('%' + vendorNameOrPingName + '%');
            }
            hsql += this.MakeQuery("G0.Number", number, para);
            return this.GetPageEx(this.TrimHSql("select G0 " + hsql) + " order by G0.Id desc", this.TrimHSql("select count(G0.Id) " + hsql), pageIndex, pageSize, para.ToArray());

        }
 
    }
}
