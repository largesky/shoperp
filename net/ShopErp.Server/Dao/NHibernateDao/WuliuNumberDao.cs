using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Dao.NHibernateDao
{
    class WuliuNumberDao : NHibernateDaoBase<WuliuNumber>
    {
        public DataCollectionResponse<WuliuNumber> GetByAll(string wuliuIds, string deliveryCompany, string deliveryNumber, string packageId, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            string hsql = " from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQueryLike("WuliuIds", wuliuIds, objs);
            hsql += this.MakeQuery("DeliveryCompany", deliveryCompany, objs);
            hsql += this.MakeQuery("DeliveryNumber", deliveryNumber, objs);
            hsql += this.MakeQuery("PackageId", packageId, objs);
            hsql += this.MakeQuery("CreateTime", start, true);
            hsql += this.MakeQuery("CreateTime", end, false);

            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }
    }
}
