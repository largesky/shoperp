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
        /// ���TIME����Ϊ�գ��򽫵���ƽ̨�ӿڣ���Ƿ���������ֻ����ƽ̨����ʱ��
        /// </summary>
        /// <param name="id"></param>
        /// <param name="time">���TIME����Ϊ�գ��򽫵���ƽ̨�ӿڣ���Ƿ���������ֻ����ƽ̨����ʱ��</param>
        public void MarkPopDelivery(long id, string time)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["time"] = time;
            DoPost<ResponseBase>(para);
        }

        /// <summary>
        /// ��ѯ����
        /// </summary>
        /// <param name="popBuyerId">����ǳƣ�ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="receiverPhone">���������ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="receiverMobile">����ֻ���ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="receiverName">���������ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="receiverAddress">��ҵ�ַ��ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="timeType">ʱ�����ͣ�0 PopPayTime ƽ̨����ʱ�䣬1 PopDeliveryTime ƽ̨����ʱ�䣬2 CreateTime ���ش���ʱ�䣬3 PrintTime ���ش�ӡʱ�䣬4 DeliveryTime ���ط���ʱ�䣬5 CloseTime ���عر�ʱ��</param>
        /// <param name="startTime">��ʼʱ�䣬���Ϊ1970-01-01 ��ʾ��ʹ��</param>
        /// <param name="endTime">����ʱ�䣬���Ϊ1970-01-01 ��ʾ��ʹ��</param>
        /// <param name="deliveryCompany">��ݹ�˾����ȷƥ��</param>
        /// <param name="deliveryNumber">��ݵ��ţ���ȷƥ��</param>
        /// <param name="state">����״̬��NONE��ʾ����ѯ</param>
        /// <param name="payType">�������ͣ�NONE��ʾ����ѯ</param>
        /// <param name="vendorName">�������ƣ�ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="number">���ţ�ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="ofs">��ɫ���ģ����Ϊ��NULL���߿����飬��ʾ����ѯ</param>
        /// <param name="parseResult">���������-1��ʾ����ѯ</param>
        /// <param name="comment">���ұ�ע��ģ��ƥ�䣬Ϊ�ջ���NULL��ʾ����ѯ</param>
        /// <param name="shopId">���̱�ţ�0��ʾ��ѯ</param>
        /// <param name="createType">�������ͣ�NONE��ʾ����ѯ</param>
        /// <param name="type">�������ͣ� NONE��ʾ����ѯ</param>
        /// <param name="pageIndex">ҳ�±꣬��0��ʼ</param>
        /// <param name="pageSize">ÿҳ���ݴ�С��0��ʾ����ҳ</param>
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
        /// �Ƿ���Ժϲ�
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
        /// ���������¶�����Ȼ��ϲ�
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
                //�ϲ����ģ����ٺϲ�
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
        /// ��ʽ���������ؿ��Է�������Ʒ��Ϣ�����������رգ���Щ��
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