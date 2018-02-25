using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShopErp.App.Service.Restful
{
    public class ShopService : ServiceBase<Shop>
    {
        public DataCollectionResponse<Shop> GetByAll()
        {
            return DoPost<DataCollectionResponse<Shop>>(null);
        }

        public StringResponse GetShopOauthUrl(long shopId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();

            para["shopId"] = shopId;

            return DoPost<StringResponse>(para);
        }
    }
}