using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class FinanceAccountService : ServiceBase<FinanceAccount>
    {
        public DataCollectionResponse<FinanceAccount> GetByAll()
        {
            return DoPost<DataCollectionResponse<FinanceAccount>>(null);
        }
    }
}
