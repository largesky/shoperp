using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.App.Service.Restful
{
    public class SystemCleanService
    {
        public LongResponse GetTableCountAll(string table)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["table"] = table;
            return ServiceBase<object>.DoPostWithUrl<LongResponse>("/systemclean/gettablecountall.html", para);
        }

        public LongResponse GetTableCount(string table, DateTime start)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["table"] = table;
            para["start"] = start;
            return ServiceBase<object>.DoPostWithUrl<LongResponse>("/systemclean/gettablecount.html", para);
        }

        public LongResponse DeleteTableData(string table, DateTime start)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["table"] = table;
            para["start"] = start;
            return ServiceBase<object>.DoPostWithUrl<LongResponse>("/systemclean/deletetabledata.html", para);
        }
    }
}
