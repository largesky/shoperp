using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class FinanceDao : NHibernateDaoBase<Finance>
    {
        public DataCollectionResponse<Finance> GetByAll(string type, long accountId,string comment,DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQueryLike("Type", type, objs);
            hsql += this.MakeQueryLike("Comment", comment, objs);
            if (accountId > 0)
            {
                hsql += this.MakeQuery("FinaceAccountId", accountId, 0);
            }

            hsql += this.MakeQuery("CreateTime", startTime, true);
            hsql += this.MakeQuery("CreateTime", endTime, false);
            return this.GetPageEx(this.TrimHSql(hsql) + " order by CreateTime desc ", "select count(id) " + this.TrimHSql(hsql), pageIndex, pageSize, objs.ToArray());
        }
    }
}
