using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class GoodsMapDao : NHibernateDaoBase<GoodsMap>
    {

        public DataCollectionResponse<GoodsMap> GetByAll(string vendor, string number, string targetNumber)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQueryLike("Vendor", vendor, objs);
            hsql += this.MakeQuery("Number", number, objs);
            hsql += this.MakeQuery("TargetNumber", targetNumber, objs);

            return this.GetPage(hsql, 0, 0, objs.ToArray());
        }

        public GoodsMap GetByVendorAndNumber(string vendor, string number)
        {
            string hsql = "from " + this.GetEntiyName() + " where Number=? and (Vendor like ? or VendorPingYing Like ?)";
            List<object> objs = new List<object>();

            objs.Add(number);
            objs.Add('%' + vendor + '%');
            objs.Add('%' + vendor + '%');
            var ret = this.GetPage(hsql, 0, 0, objs.ToArray());
            if (ret.Datas.Count < 1)
            {
                return null;
            }

            return ret.Datas[0];
        }

        public DataCollectionResponse<GoodsMap> GetByAll(string vendor, string number, long targetGoodsId, int pageIndex, int pageSize)
        {
            List<object> para = new List<object>();
            string hsql = "from " + this.GetEntiyName() + " GM0,Vendor V0 where GM0.VendorId=V0.Id and ";

            if (string.IsNullOrWhiteSpace(vendor) == false)
            {
                hsql += " (V0.Name like ? or V0.PingyingName like ?) and ";
                para.Add('%' + vendor + '%');
                para.Add('%' + vendor + '%');
            }
            hsql += this.MakeQueryLike("GM0.Number", number, para);
            if (targetGoodsId > 0)
            {
                hsql += this.MakeQuery("GM0.TargetGoodsId", targetGoodsId);
            }
            return this.GetPageEx(this.TrimHSql("select GM0 " + hsql) + " order by GM0.Id desc", this.TrimHSql("select count(GM0.Id) " + hsql), pageIndex, pageSize, para.ToArray());

        }
    }
}
