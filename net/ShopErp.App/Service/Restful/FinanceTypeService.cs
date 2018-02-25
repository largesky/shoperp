using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Restful
{
    class FinanceTypeService : ServiceBase<FinanceType>
    {
        public List<FinanceType> GetByAll()
        {
            return DoPost<DataCollectionResponse<FinanceType>>(null).Datas;
        }
    }
}
