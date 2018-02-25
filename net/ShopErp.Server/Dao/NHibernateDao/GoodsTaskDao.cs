using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class GoodsTaskDao : NHibernateDaoBase<GoodsTask>
    {
        public DataCollectionResponse<GoodsTask> GetByAll(long shopId, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            hsql += this.MakeQuery("ShopId", shopId);
            var ret = this.GetPage(hsql, pageIndex, pageSize);
            return ret;
        }
    }
}
