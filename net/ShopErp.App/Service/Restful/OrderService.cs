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

        public ResponseBase UpdateDelivery(long id, long deliveryTemplateId, string deliveryCompany, string deliveryNumber, DateTime printTime)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["deliveryTemplateId"] = deliveryTemplateId;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["printTime"] = printTime;
            return DoPost<ResponseBase>(para);
        }

        public Order[] MarkDelivery(string deliveryNumber, float weight, bool chkWeight, bool chkPopState, bool chkLocalState)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            para["weight"] = weight;
            para["chkWeight"] = chkWeight;
            para["chkPopState"] = chkPopState;
            para["chkLocalState"] = chkLocalState;
            return DoPost<DataCollectionResponse<Order>>(para).Datas.ToArray();
        }

        /// <summary>
        /// 如果TIME参数为空，则将调用平台接口，标记发货。否则只更新平台发货时间
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time">如果TIME参数为空，则将调用平台接口，标记发货。否则只更新平台发货时间</param>
        public void MarkPopDelivery(long id, string time)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["time"] = time;
            DoPost<ResponseBase>(para);
        }

        /// <summary>
        /// 查询订单
        /// </summary>
        /// <param name="popBuyerId">买家昵称，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="receiverPhone">买家座机，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="receiverMobile">买家手机，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="receiverName">买家姓名，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="receiverAddress">买家地址，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="timeType">时间类型：0 PopCreateTime 平台创建时间，1 PopPayTime 平台付款时间，2 PopDeliveryTime 平台发货时间，3 CreateTime 本地创建时间，4 PrintTime 本地打印时间，5 DeliveryTime 本地发货时间，6 CloseTime 本地关闭时间</param>
        /// <param name="startTime">开始时间，如果为1970-01-01 表示不使用</param>
        /// <param name="endTime">结束时间，如果为1970-01-01 表示不使用</param>
        /// <param name="deliveryCompany">快递公司，精确匹配</param>
        /// <param name="deliveryNumber">快递单号，精确匹配</param>
        /// <param name="state">订单状态，NONE表示不查询</param>
        /// <param name="payType">付款类型，NONE表示不查询</param>
        /// <param name="vendorName">厂家名称，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="number">货号，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="ofs">颜色旗帜，如果为空NULL或者空数组，表示不查询</param>
        /// <param name="parseResult">解析结果，-1表示不查询</param>
        /// <param name="comment">卖家备注，模糊匹配，为空或者NULL表示不查询</param>
        /// <param name="shopId">店铺编号，0表示查询</param>
        /// <param name="createType">创建类型，NONE表示不查询</param>
        /// <param name="type">订单类型， NONE表示不查询</param>
        /// <param name="pageIndex">页下标，从0开始</param>
        /// <param name="pageSize">每页数据大小，0表示不分页</param>
        /// <returns></returns>
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

        public ResponseBase CloseOrder(long orderId, long orderGoodsId, int count)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            para["count"] = count;
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

        public OrderDownloadCollectionResponse GetPopWaitSendOrders(Shop shop, PopPayType payType, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["payType"] = payType;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<OrderDownloadCollectionResponse>(para);
        }

        public OrderDownloadCollectionResponse SaveOrUpdateOrdersByPopOrderId(Shop shop, List<OrderDownload> orders)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["orders"] = orders;
            return DoPost<OrderDownloadCollectionResponse>(para);
        }

        public StringResponse UpdateOrderStateWithGoods(Order orderOnline, OrderUpdate orderInDb, Shop shop)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderOnline"] = orderOnline;
            para["orderInDb"] = orderInDb;
            para["shop"] = shop;
            return DoPost<StringResponse>(para);
        }

        public ResponseBase UpdateOrderGoodsState(long orderId, long orderGoodsId, OrderState state, string stockComment)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            para["orderGoodsId"] = orderGoodsId;
            para["state"] = state;
            para["stockComment"] = stockComment;
            return DoPost<ResponseBase>(para);
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