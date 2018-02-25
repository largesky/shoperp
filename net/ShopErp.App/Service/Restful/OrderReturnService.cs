using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ShopErp.App.Service.Restful
{
    public class OrderReturnService : ServiceBase<OrderReturn>
    {
        public DataCollectionResponse<OrderReturn> GetByAll(long id, long orderId, string vendor, string number,
            string deliveryNumber, OrderReturnState state, OrderReturnType type, int timeType, DateTime start,
            DateTime end, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["orderId"] = orderId;
            para["vendor"] = vendor;
            para["number"] = number;
            para["deliveryNumber"] = deliveryNumber;
            para["state"] = state;
            para["type"] = type;
            para["timeType"] = timeType;
            para["start"] = start;
            para["end"] = end;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<OrderReturn>>(para);
        }

        public DataCollectionResponse<OrderReturn> GetByOrderIdAndOrderGoodsId(long orderId, long orderGoodsId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            return DoPost<DataCollectionResponse<OrderReturn>>(para);
        }

        public LongResponse Create(long orderId, long orderGoodsId, string deliveryCompany,
            string deliveryNumber, OrderReturnType type, OrderReturnReason reason, int count)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["type"] = type;
            para["reason"] = reason;
            para["count"] = count;
            return DoPost<LongResponse>(para);
        }

        public LongResponse CreateWithoutOrder(string deliveryCompany, string deliveryNumber,
            string goodsInfo, float goodsMoney, int count)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["goodsInfo"] = goodsInfo;
            para["goodsMoney"] = goodsMoney;
            para["count"] = count;
            return DoPost<LongResponse>(para);
        }
    }
}