using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Dao.NHibernateDao
{
    class TaobaoKeywordDetailDao : NHibernateDaoBase<TaobaoKeywordDetail>
    {
        public DataCollectionResponse<TaobaoKeywordDetail> GetByAll(string number, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQuery("Number", number, objs);
            hsql += this.MakeQuery("CreateTime", start, true);
            hsql += this.MakeQuery("CreateTime", end, false);

            return this.GetPage(hsql, pageIndex, pageSize, objs.ToArray());
        }


    }
}
