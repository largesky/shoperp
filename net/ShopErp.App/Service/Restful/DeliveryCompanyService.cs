using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.App.Service.Restful
{
    public class DeliveryCompanyService : ServiceBase<DeliveryCompany>
    {
        public DataCollectionResponse<DeliveryCompany> GetByAll()
        {
            return DoPost<DataCollectionResponse<DeliveryCompany>>(null);
        }

        public DeliveryCompany GetDeliveryCompany(string name)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["name"] = name;
            return DoPost<DataCollectionResponse<DeliveryCompany>>(para).First;
        }

        public static List<string> GetDeliveryCompaniyNames()
        {
            return ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name)
                .ToList();
        }
    }
}