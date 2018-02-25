using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ShopErp.App.Service.Restful
{
    public class OrderGoodsService : ServiceBase<OrderGoods>
    {
        public DataCollectionResponse<GoodsCount> GetGoodsCount(ColorFlag[] flags, DateTime startTime, DateTime endTime,
            int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["flags"] = flags;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;

            return DoPost<DataCollectionResponse<GoodsCount>>(para);
        }

        public DataCollectionResponse<SaleCount> GetSaleCount(long shopId, int timeType, DateTime startTime, DateTime endTime,
            string popNumberId, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopId"] = shopId;
            para["timeType"] = timeType;
            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["popNumberId"] = popNumberId;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<SaleCount>>(para);
        }
    }
}