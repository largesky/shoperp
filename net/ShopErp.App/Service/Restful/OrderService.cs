using System;
using System.Collections.Generic;
using System.Linq;
using ShopErp.Domain;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.App.Service.Restful
{
    public class OrderService : ServiceBase<Order>
    {
        public DataCollectionResponse<Order> GetById(string id)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public DataCollectionResponse<Order> GetByPopOrderId(string popOrderId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["popOrderId"] = popOrderId;
            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public DataCollectionResponse<Order> GetByDeliveryNumber(string deliveryNumber)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public ResponseBase UpdateDelivery(long id, long deliveryTemplateId, string deliveryCompany, string deliveryNumber, PaperType paperyType, DateTime printTime)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["deliveryTemplateId"] = deliveryTemplateId;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["paperyType"] = paperyType;
            para["printTime"] = printTime;
            return DoPost<ResponseBase>(para);
        }

        public Order[] MarkDelivery(string deliveryNumber, float weight, bool ingorePopError, bool ingoreWeightDetect, bool ingoreStateCheck)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            para["weight"] = weight;
            para["ingorePopError"] = ingorePopError;
            para["ingoreWeightDetect"] = ingoreWeightDetect;
            para["ingoreStateCheck"] = ingoreStateCheck;
            return DoPost<DataCollectionResponse<Order>>(para).Datas.ToArray();
        }

        public void MarkPopDelivery(long id)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            DoPost<ResponseBase>(para);
        }

        public DataCollectionResponse<Order> GetByAll(string popBuyerId, string receiverPhone, string receiverMobile,
            string receiverName, string receiverAddress,
            int timeType, DateTime startTime, DateTime endTime, string deliveryCompany, string deliveryNumber,
            OrderState state, PopPayType payType, string vendorName, string number,
            ColorFlag[] ofs, int parseResult, string comment, long shopId, OrderCreateType createType, OrderType type,
            int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["popBuyerId"] = popBuyerId;
            para["receiverPhone"] = receiverPhone;
            para["receiverMobile"] = receiverMobile;
            para["ReceiverName"] = receiverName;
            para["receiverAddress"] = receiverAddress;
            para["timeType"] = timeType;

            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["state"] = state;
            para["payType"] = payType;

            para["vendorName"] = vendorName;
            para["number"] = number;
            para["ofs"] = ofs;
            para["parseResult"] = parseResult;
            para["comment"] = comment;
            para["shopId"] = shopId;

            para["createType"] = createType;
            para["type"] = type;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;

            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public DataCollectionResponse<Order> GetPayedAndPrintedOrders(long[] shopId, OrderCreateType createType, PopPayType payType, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopId"] = shopId;
            para["createType"] = createType;
            para["payType"] = payType;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public DataCollectionResponse<Order> GetOrdersByInfoIdNotEqual(string popBuyerId, string receiverPhone, string receiverMobile, string receiverAddress, long id)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["popBuyerId"] = popBuyerId;
            para["receiverPhone"] = receiverPhone;
            para["receiverMobile"] = receiverMobile;
            para["receiverAddress"] = receiverAddress;
            para["id"] = id;

            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public StringResponse GetOrderPopCodNumber(long id)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            return DoPost<StringResponse>(para);
        }

        public ResponseBase CloseOrder(long orderId, long orderGoodsId, int count)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            para["count"] = count;
            return DoPost<ResponseBase>(para);
        }

        public ResponseBase MarkOrderGoodsState(long orderId, long orderGoodsId, OrderState state, string stockComment)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            para["state"] = state;
            para["stockComment"] = stockComment;
            return DoPost<ResponseBase>(para);
        }

        public ResponseBase SpilteOrderGoods(long orderId, OrderSpilteInfo[] infos)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["infos"] = infos;
            return DoPost<ResponseBase>(para);
        }

        public ResponseBase ResetPrintState(long orderId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            return DoPost<ResponseBase>(para);
        }

        public ResponseBase ModifyPopSellerComment(long orderId, ColorFlag flag, string comment)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["flag"] = flag;
            para["comment"] = comment;
            return DoPost<ResponseBase>(para);
        }


        public OrderDownloadCollectionResponse GetPopWaitSendOrders(Shop shop, PopPayType payType, int pageIndex,int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["payType"] = payType;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<OrderDownloadCollectionResponse>(para);
        }

        public StringResponse UpdateOrderState(PopOrderState orderStateOnline, OrderUpdate orderInDb, Shop shop)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderStateOnline"] = orderStateOnline;
            para["orderInDb"] = orderInDb;
            para["shop"] = shop;
            return DoPost<StringResponse>(para);
        }

        public ResponseBase UpdateOrderToGeted(long orderId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            return DoPost<ResponseBase>(para);
        }
    }
}