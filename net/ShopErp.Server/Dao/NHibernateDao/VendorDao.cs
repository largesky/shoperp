using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class VendorDao : NHibernateDaoBase<Vendor>
    {
        public DataCollectionResponse<Vendor> GetByAll(string name, string pingYingName, string homePage, string marketAddress, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<Object> objs = new List<object>();

            hsql += this.MakeQueryLike("Name", name, objs);
            hsql += this.MakeQueryLike("PingYingName", pingYingName, objs);
            hsql += this.MakeQueryLike("HomePage", homePage, objs);
            hsql += this.MakeQueryLike("MarketAddress", marketAddress, objs);

            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }
    }
}
