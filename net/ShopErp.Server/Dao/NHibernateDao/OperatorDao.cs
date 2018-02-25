using ShopErp.Domain;
using System.Collections.Generic;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class OperatorDao : NHibernateDaoBase<Operator>
    {
        public Operator GetByNumberAndPassword(string number, string password)
        {
            string hsql = "from " + this.GetEntiyName() + " where ";
            List<object> objs = new List<object>();

            hsql += this.MakeQuery("Number", number, objs);
            hsql += this.MakeQuery("Password", password, objs);

            var ret = this.GetPage(hsql, 0, 0, objs.ToArray());

            if (ret.Total < 1)
            {
                return null;
            }

            return ret.Datas[0];
        }
    }
}
