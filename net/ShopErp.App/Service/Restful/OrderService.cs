using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Order[] MarkDelivery(string deliveryNumber, int goodsCount, bool chkPopState, bool chkLocalState)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["deliveryNumber"] = deliveryNumber;
            para["goodsCount"] = goodsCount;
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
        /// <param name="timeType">时间类型：0 PopPayTime 平台付款时间，1 PopDeliveryTime 平台发货时间，2 CreateTime 本地创建时间，3 PrintTime 本地打印时间，4 DeliveryTime 本地发货时间，5 CloseTime 本地关闭时间</param>
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
        public DataCollectionResponse<Order> GetByAll(string popBuyerId, string receiverMobile,
            string receiverName, string receiverAddress, DateTime startTime, DateTime endTime, string deliveryCompany, string deliveryNumber,
            OrderState state, PopPayType payType, string vendorName, string number, string size,
            ColorFlag[] ofs, int parseResult, string comment, long shopId, OrderCreateType createType, OrderType type, string shipper,
            int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["popBuyerId"] = popBuyerId;
            para["receiverMobile"] = receiverMobile;
            para["receiverName"] = receiverName;
            para["receiverAddress"] = receiverAddress;

            para["startTime"] = startTime;
            para["endTime"] = endTime;
            para["deliveryCompany"] = deliveryCompany;
            para["deliveryNumber"] = deliveryNumber;
            para["state"] = state;
            para["payType"] = payType;

            para["vendorName"] = vendorName;
            para["number"] = number;
            para["size"] = size;
            para["ofs"] = ofs;
            para["parseResult"] = parseResult;
            para["comment"] = comment;
            para["shopId"] = shopId;

            para["createType"] = createType;
            para["type"] = type;
            para["shipper"] = shipper;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;

            return DoPost<DataCollectionResponse<Order>>(para);
        }

        public DataCollectionResponse<Order> GetPayedAndPrintedOrders(long[] shopId, OrderCreateType createType, PopPayType payType, string shipper, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shopId"] = shopId;
            para["createType"] = createType;
            para["payType"] = payType;
            para["shipper"] = shipper;
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

        public ResponseBase ModifyOrderGoodsPrice(long orderGoodsId, float price)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderGoodsId"] = orderGoodsId;
            para["price"] = price;
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

        public OrderDownloadCollectionResponse GetPopWaitSendOrders(Shop shop, PopPayType payType, DateTime dateTime, int pageIndex, int pageSize)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["payType"] = payType;
            para["dateTime"] = dateTime;
            para["pageIndex"] = pageIndex;
            para["pageSize"] = pageSize;
            return DoPost<OrderDownloadCollectionResponse>(para);
        }

        public DataCollectionResponse<PopOrderState> GetPopOrderState(Shop shop, string popOrderId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["popOrderId"] = popOrderId;
            return DoPost<DataCollectionResponse<PopOrderState>>(para);
        }

        public OrderDownloadCollectionResponse SaveOrUpdateOrdersByPopOrderId(Shop shop, List<OrderDownload> orders)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["shop"] = shop;
            para["orders"] = orders;
            return DoPost<OrderDownloadCollectionResponse>(para);
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

        public ResponseBase UpdateOrderGoodsStateToGeted(long orderId)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["orderId"] = orderId;
            return DoPost<ResponseBase>(para);
        }

        public DataOneResponse<OrderState> UpdateOrderState(string popOrderid, OrderState onlineOrderState, OrderUpdate orderInDb, Shop shop)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["popOrderid"] = popOrderid;
            para["onlineOrderState"] = onlineOrderState;
            para["orderInDb"] = orderInDb;
            para["shop"] = shop;
            return DoPost<DataOneResponse<OrderState>>(para);
        }

        /// <summary>
        /// 是否可以合并
        /// </summary>
        /// <param name="o1"></param>
        /// <param name="o2"></param>
        /// <returns></returns>
        public static bool CanbeMerge(Order o1, Order o2)
        {
            if (o1 == null || o2 == null)
            {
                throw new ArgumentNullException("CanbeMerge");
            }

            return o1.ShopId == o2.ShopId &&
                   o1.PopBuyerId.Trim() == o2.PopBuyerId.Trim() &&
                   o1.ReceiverName.Trim() == o2.ReceiverName.Trim() &&
                   o1.ReceiverPhone.Trim() == o2.ReceiverPhone.Trim() &&
                   o1.ReceiverMobile.Trim() == o2.ReceiverMobile.Trim() &&
                   o1.ReceiverAddress.Trim() == o2.ReceiverAddress.Trim();
        }

        /// <summary>
        /// 复制生成新订单，然后合并
        /// </summary>
        /// <param name="orders"></param>
        /// <returns></returns>
        public static Order[] MergeOrders(Order[] orders)
        {
            if (orders == null)
            {
                throw new ArgumentNullException("orders");
            }

            List<Order> mergedOrders = new List<Order>();
            Order[] nOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<Order[]>(Newtonsoft.Json.JsonConvert.SerializeObject(orders));

            foreach (var or in nOrders)
            {
                var first = mergedOrders.FirstOrDefault(obj => CanbeMerge(or, obj));
                if (first == null)
                {
                    mergedOrders.Add(or);
                    continue;
                }
                if (or.OrderGoodss == null || or.OrderGoodss.Count < 1)
                {
                    continue;
                }
                first.OrderGoodss = first.OrderGoodss ?? new List<OrderGoods>();
                //合并过的，不再合并
                foreach (var og in or.OrderGoodss)
                {
                    if (first.OrderGoodss.Any(obj => obj.Id == og.Id) == false)
                    {
                        first.OrderGoodss.Add(og);
                    }
                }
            }
            return mergedOrders.ToArray();
        }

        public static OrderGoods[] FilterOrderGoodsWithStateOk(Order order, bool onlnyNormal)
        {
            if (order == null || order.OrderGoodss == null || order.OrderGoodss.Count < 1 || (onlnyNormal && order.Type != OrderType.NORMAL))
            {
                return new OrderGoods[0];
            }
            return order.OrderGoodss.Where(obj => (int)OrderState.PAYED <= (int)obj.State && (int)obj.State <= (int)OrderState.SUCCESS).ToArray();
        }


        /// <summary>
        /// 格式化订单下载可以发货的商品信息，不包包含关闭，这些的
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public static string FormatGoodsInfoWithStateOk(Order order, bool onlnyNormal, bool usVendorPingying)
        {
            var orderGoods = FilterOrderGoodsWithStateOk(order, onlnyNormal);
            StringBuilder sb = new StringBuilder();
            foreach (var goods in orderGoods)
            {
                sb.AppendLine((usVendorPingying ? ServiceContainer.GetService<VendorService>().GetVendorPingyingName(goods.Vendor).ToUpper() : VendorService.FormatVendorName(goods.Vendor)) + " " + goods.Number + goods.Edtion + goods.Color + goods.Size + " (" + goods.Count + ") ");
            }
            return sb.ToString().Trim();
        }

        public static int CountGoodsWithStateOk(Order order, bool onlnyNormal)
        {
            return FilterOrderGoodsWithStateOk(order, onlnyNormal).Select(obj => obj.Count).Sum();
        }
    }
}