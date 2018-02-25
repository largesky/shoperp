using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.App.Service.Restful
{
    public class GoodsShopService : ServiceBase<GoodsShop>
    {
        public DataCollectionResponse<GoodsShop> GetByAll(long goodsId, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["goodsId"] = goodsId;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<GoodsShop>>(para);
        }
    }
}