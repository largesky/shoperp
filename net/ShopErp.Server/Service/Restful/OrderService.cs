using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ShopErp.Server.Service.Pop;
using ShopErp.Domain;
using ShopErp.Server.Dao.NHibernateDao;
using System.ServiceModel;
using System.ServiceModel.Web;
using ShopErp.Domain.Pop;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Log;
using ShopErp.Server.Service.Net;
using System.IO;
using NHibernate.Util;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OrderService : ServiceBase<Order, OrderDao>
    {
        public const string UPDATE_RET_NOEXIST = "本地订单中不存在";
        public const string UPDATE_RET_NOUPDATED = "未更新订单";
        public const string UPDATE_RET_UPDATED = "已更新订单";

        protected static readonly char[] SPILTE_CHAR_COMMENT_L = new char[] { '(', '（' };
        protected static readonly char[] SPILTE_CHAR_COMMENT_R = new char[] { ')', '）' };
        protected static readonly char[] SPILTE_CHAR_EDTION_L = new char[] { '[', '【' };
        protected static readonly char[] SPILTE_CHAR_EDTION_R = new char[] { ']', '】' };

        private PopService ps = new PopService();

        private OrderGoodsDao ogDao = new OrderGoodsDao();

        private Order GetByIdWithException(long id)
        {
            Order or = this.dao.GetById(id);
            if (or == null)
            {
                throw new Exception("订单不存在");
            }
            return or;
        }

        /// <summary>
        /// 去除颜色，尺码中以()，[]包围的内容
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected static string RemoveByChar(string value, char[] lcs, char[] rcs)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            int indexL = value.IndexOfAny(lcs);
            int indexR = value.IndexOfAny(rcs);
            if (indexL >= 0 && indexR > indexL)
            {
                value = value.Substring(0, indexL);
            }
            return value;
        }

        protected void ParseColorSizeEditon(string iColor, string iSize, out string oColor, out string oEdtion, out string oSize)
        {
            oSize = RemoveByChar(iSize, SPILTE_CHAR_COMMENT_L, SPILTE_CHAR_COMMENT_R);
            oSize = RemoveByChar(oSize, SPILTE_CHAR_EDTION_L, SPILTE_CHAR_EDTION_R);

            oColor = RemoveByChar(iColor, SPILTE_CHAR_COMMENT_L, SPILTE_CHAR_COMMENT_R);

            if (oColor.IndexOfAny(SPILTE_CHAR_EDTION_L) >= 0)
            {
                oEdtion = oColor.Substring(oColor.IndexOfAny(SPILTE_CHAR_EDTION_L));
                oEdtion = oEdtion.Replace("(", "").Replace("（", "").Replace(")", "").Replace("）", "").Replace("[", "").Replace("【", "").Replace("]", "").Replace("】", "").Replace(" ", "");
                oColor = oColor.Substring(0, oColor.IndexOfAny(SPILTE_CHAR_EDTION_L));
            }
            else
            {
                oColor = iColor;
                oEdtion = "";
            }
            if (string.IsNullOrWhiteSpace(oEdtion) == false)
            {
                oEdtion = oEdtion.Replace("版", "");
                oEdtion = oEdtion.Replace("跟高", "");
            }
        }

        private void FillEmptyAndParseGoods(Order order)
        {
            if (order.State == OrderState.NONE)
            {
                throw new Exception("订单State不能为NONE");
            }
            if (order.ShopId < 1)
            {
                throw new Exception("订单ShopId小于1");
            }
            if (order.CreateType == OrderCreateType.NONE)
            {
                throw new Exception("订单CreateType不能为NONE");
            }
            if (order.Type == OrderType.NONE)
            {
                throw new Exception("订单Type不能为NONE");
            }
            if (order.PopType == PopType.None)
            {
                throw new Exception("订单PopType不能为NONE");
            }
            if (order.PopPayType == PopPayType.None)
            {
                throw new Exception("订单PopPayType不能为NONE");
            }
            if (order.PopFlag == ColorFlag.None)
            {
                throw new Exception("订单PopFlag不能为NONE");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverName))
            {
                throw new Exception("订单ReceiverName不能为空");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverAddress))
            {
                throw new Exception("订单ReceiverAddress不能为空");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverPhone) && string.IsNullOrWhiteSpace(order.ReceiverMobile))
            {
                throw new Exception("订单ReceiverPhone,与ReceiverMobile 不能同时为空");
            }
            order.PopOrderId = order.PopOrderId ?? string.Empty;
            order.PopBuyerId = order.PopBuyerId ?? string.Empty;
            order.PopCodSevFee = order.PopCodSevFee < 0 ? 0 : order.PopCodSevFee;
            order.PopCodNumber = order.PopCodNumber ?? string.Empty;
            order.PopSellerComment = order.PopSellerComment ?? string.Empty;
            order.PopBuyerComment = order.PopBuyerComment ?? string.Empty;
            order.PopState = order.PopState ?? String.Empty;
            order.DeliveryCompany = order.DeliveryCompany ?? string.Empty;
            order.DeliveryNumber = order.DeliveryNumber ?? string.Empty;
            order.PopPayTime = this.IsDbMinTime(order.PopPayTime) ? DateTime.Now : order.PopPayTime;
            order.PopDeliveryTime = this.IsDbMinTime(order.PopDeliveryTime) ? this.GetDbMinTime() : order.PopDeliveryTime;
            order.CreateTime = this.IsDbMinTime(order.CreateTime) ? DateTime.Now : order.CreateTime;
            order.PrintTime = this.IsDbMinTime(order.PrintTime) ? this.GetDbMinTime() : order.PrintTime;
            order.DeliveryTime = this.IsDbMinTime(order.DeliveryTime) ? this.GetDbMinTime() : order.DeliveryTime;
            order.CloseTime = this.IsDbMinTime(order.CloseTime) ? this.GetDbMinTime() : order.CloseTime;
            order.CreateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number;
            order.PrintOperator = order.PrintOperator ?? string.Empty;
            order.DeliveryOperator = order.DeliveryOperator ?? string.Empty;
            order.CloseOperator = order.CloseOperator ?? string.Empty;
            order.ParseResult = true;
            if (order.OrderGoodss != null && order.OrderGoodss.Count > 0)
            {
                foreach (var item in order.OrderGoodss)
                {
                    if (item.State == OrderState.NONE)
                    {
                        throw new Exception("订单商品状态不能为NONE：" + item.Vendor + " " + item.Number);
                    }
                    item.GoodsId = 0;
                    item.Vendor = item.Vendor ?? string.Empty;
                    item.Number = item.Number ?? string.Empty;
                    item.Edtion = item.Edtion ?? string.Empty;
                    item.Color = item.Color ?? string.Empty;
                    item.Size = item.Size ?? string.Empty;
                    item.PopUrl = item.PopUrl ?? String.Empty;
                    item.PopInfo = item.PopInfo ?? string.Empty;
                    item.PopOrderSubId = item.PopOrderSubId ?? string.Empty;
                    item.CloseTime = this.IsDbMinTime(item.CloseTime) ? this.GetDbMinTime() : item.CloseTime;
                    item.CloseOperator = item.CloseOperator ?? string.Empty;
                    item.Comment = item.Comment ?? string.Empty;
                    item.StockTime = this.IsDbMinTime(item.StockTime) ? this.GetDbMinTime() : item.StockTime;
                    item.StockOperator = item.StockOperator ?? string.Empty;
                    item.Image = item.Image ?? string.Empty;
                    string color = null, edtion = null, size = null;
                    ParseColorSizeEditon(item.Color, item.Size, out color, out edtion, out size);
                    item.Color = color ?? item.Color;
                    item.Edtion = (edtion ?? item.Edtion);
                    item.Size = size ?? item.Size;
                    Goods g = ServiceContainer.GetService<GoodsService>().ParsePopOrderGoodsNumber(item);
                    if (g != null)
                    {
                        item.Shipper = g.Shipper;
                        item.Edtion = g.IgnoreEdtion ? string.Empty : item.Edtion;
                    }
                    else
                    {
                        item.Shipper = "";
                    }
                }
                order.ParseResult = order.OrderGoodss.All(o => o.GoodsId > 0);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<Order> GetById(string id)
        {
            try
            {
                id = id.Trim();
                if (id.All(c => Char.IsDigit(c)) && id.Length < 14)
                {
                    var item = this.dao.GetById(long.Parse(id));
                    if (item != null)
                        return new DataCollectionResponse<Order>(item);
                }
                var items = this.dao.GetAllByField("PopOrderId", id, 0, 0).Datas;
                return new DataCollectionResponse<Order>(items);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbypoporderid.html")]
        public DataCollectionResponse<Order> GetByPopOrderId(string popOrderId)
        {
            try
            {
                var item = this.dao.GetAllByField("PopOrderId", popOrderId, 0, 0).Datas;
                return new DataCollectionResponse<Order>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbydeliverynumber.html")]
        public DataCollectionResponse<Order> GetByDeliveryNumber(string deliveryNumber)
        {
            try
            {
                var item = this.dao.GetAllByField("DeliveryNumber", deliveryNumber, 0, 0).Datas;
                return new DataCollectionResponse<Order>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(Order value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.PopOrderId) == false)
                {
                    //检测数据库是否存在
                    var count = this.dao.GetColumnValueBySqlQuery<long>("select count(Id) from `Order` where PopOrderId='" + value.PopOrderId + "'").First();
                    if (count > 0)
                    {
                        throw new Exception("订单编号：" + value.PopOrderId + "已经存在");
                    }
                }
                FillEmptyAndParseGoods(value);
                this.dao.Save(value);
                if (value.OrderGoodss != null && value.OrderGoodss.Count > 0)
                {
                    foreach (var v in value.OrderGoodss)
                    {
                        v.OrderId = value.Id;
                    }
                    this.dao.Save(value.OrderGoodss.ToArray());
                }
                return new LongResponse(value.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public ResponseBase Update(Order value)
        {
            try
            {
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }

                FillEmptyAndParseGoods(value);
                this.dao.Update(value);

                //删除以前的，现在订单中没有商品
                var ogs = ogDao.GetAllByField("OrderId", value.Id, 0, 0).Datas;
                var toDelete = value.OrderGoodss == null ? ogs.ToArray() : ogs.Where(obj => value.OrderGoodss.FirstOrDefault(o => o.Id == obj.Id) == null).ToArray();
                if (toDelete.Length > 0)
                {
                    ogDao.Delete(toDelete);
                }

                if (value.OrderGoodss != null && value.OrderGoodss.Count > 0)
                {
                    foreach (var v in value.OrderGoodss)
                    {
                        v.OrderId = value.Id;
                    }
                    this.dao.SaveOrUpdateById(value.OrderGoodss.ToArray());
                }
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/delete.html")]
        public ResponseBase Delete(long id)
        {
            try
            {
                this.dao.DeleteByLongId(id);
                this.dao.ExcuteSqlUpdate("delete from OrderGoods where OrderId=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/updatedelivery.html")]
        public ResponseBase UpdateDelivery(long id, long deliveryTemplateId, string deliveryCompany, string deliveryNumber, DateTime printTime)
        {
            try
            {
                var or = this.GetByIdWithException(id);
                or.DeliveryCompany = deliveryCompany;
                or.DeliveryNumber = deliveryNumber;
                or.DeliveryTemplateId = deliveryTemplateId;
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                //快递单号为空，表示需要重置打印
                if (string.IsNullOrWhiteSpace(deliveryNumber))
                {
                    foreach (var og in or.OrderGoodss)
                    {
                        if (og.State == OrderState.PRINTED)
                        {
                            og.State = OrderState.PAYED;
                            this.dao.Update(og);
                        }
                    }
                    if ((int)or.State >= (int)OrderState.PRINTED && (int)or.State < (int)OrderState.SHIPPED)
                    {
                        or.State = OrderState.PAYED;
                    }
                    or.PrintTime = this.GetDbMinTime();
                    or.PrintOperator = op;
                    this.dao.Update(or);
                }
                else
                {
                    foreach (var og in or.OrderGoodss)
                    {
                        if (og.State == OrderState.PAYED || (this.IsDbMinTime(or.DeliveryTime) && or.State == OrderState.RETURNING))
                        {
                            og.State = OrderState.PRINTED;
                            this.dao.Update(og);
                        }
                    }
                    if (or.State == OrderState.PAYED || (this.IsDbMinTime(or.DeliveryTime) && or.State == OrderState.RETURNING))
                    {
                        or.State = OrderState.PRINTED;
                    }
                    or.PrintOperator = op;
                    or.PrintTime = printTime;
                    this.dao.Update(or);
                }
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/markdelivery.html")]
        public DataCollectionResponse<Order> MarkDelivery(string deliveryNumber, int goodsCount, bool chkPopState, bool chkLocalState)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                var allOrders = this.GetByDeliveryNumber(deliveryNumber).Datas;

                if (allOrders == null || allOrders.Count < 1)
                {
                    throw new Exception("快递单号未找到订单");
                }

                //如果要检查状态，则过滤状态不正确的订单。因为有时候一个人同时几双，其中某些可能不要了。
                if (chkLocalState)
                {
                    allOrders = allOrders.Where(obj => (int)obj.State >= (int)OrderState.PRINTED && (int)obj.State <= (int)OrderState.SHIPPED).ToList();

                    //如果所有订单都已经过滤，则说明所有订单状态都不正确。
                    if (allOrders.Count < 1)
                    {
                        throw new Exception("订单状态不正确");
                    }
                }

                //正常订单
                var normalOrders = allOrders.Where(obj => obj.Type != OrderType.SHUA).ToArray();

                //订单基本信息检查
                if (normalOrders.Select(obj => obj.ShopId).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且店铺不一样");
                }

                if (normalOrders.Select(obj => obj.ReceiverName).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且收货人姓名不一样");
                }

                if (normalOrders.Select(obj => obj.ReceiverPhone).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且收货人电话不一样");
                }

                if (normalOrders.Select(obj => obj.ReceiverMobile).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且收货人手机不一样");
                }

                if (normalOrders.Select(obj => obj.ReceiverAddress.Trim()).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且收货人地址不一样");
                }

                if (normalOrders.Select(obj => obj.PopPayType).Distinct().Count() > 1)
                {
                    throw new Exception("多个订单且付款类型不一样");
                }

                //合并所有效订单中商品
                var normalOgs = new List<OrderGoods>();
                foreach (var or in normalOrders)
                {
                    if (chkLocalState)
                    {
                        normalOgs.AddRange(or.OrderGoodss.Where(obj => (int)obj.State >= (int)OrderState.PAYED && (int)obj.State <= (int)OrderState.SUCCESS));
                    }
                    else
                    {
                        normalOgs.AddRange(or.OrderGoodss.Where(obj => obj.State != OrderState.SPILTED));
                    }
                }
                if (goodsCount != normalOgs.Where(obj => obj.IsPeijian == false).Select(obj => obj.Count).Sum())
                {
                    throw new Exception("商品数量不匹配");
                }
                foreach (var og in normalOgs)
                {
                    if (normalOgs.Any(obj => obj.Shipper.Length > og.Shipper.Length ? obj.Shipper.IndexOf(og.Shipper, StringComparison.OrdinalIgnoreCase) < 0 : og.Shipper.IndexOf(obj.Shipper, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        throw new Exception("不同仓库不能用一个单号发货");
                    }
                }

                //第一个正常的订单，如果都不是，则默认为第一个订单
                var mainOrder = normalOrders.Length > 0 ? normalOrders[0] : allOrders[0];
                //计算快递费用
                double deliveryMoney = ServiceContainer.GetService<DeliveryTemplateService>().ComputeDeliveryMoneyImplByCount(mainOrder.DeliveryCompany, mainOrder.ReceiverAddress, mainOrder.Type != OrderType.NORMAL, mainOrder.PopPayType, goodsCount);

                //更新订单状态，运费金额信息
                List<object> objsToUpdate = new List<object>(normalOgs);
                foreach (OrderGoods og in normalOgs)
                {
                    og.State = OrderState.SHIPPED;
                }

                string comment = string.Format("【发货{0}】", DateTime.Now.ToString("MM-dd HH:mm"));
                foreach (var order in allOrders)
                {
                    order.DeliveryMoney = (float)Math.Round(deliveryMoney / (normalOrders.Length > 0 ? normalOrders.Length : allOrders.Count), 2);
                    order.Weight = 0;
                    order.DeliveryTime = DateTime.Now;
                    order.DeliveryOperator = op;
                    order.State = (order.State == OrderState.SUCCESS) ? OrderState.SUCCESS : OrderState.SHIPPED;
                    //检查当前是否有标记发货信息
                    int startIndex = order.PopSellerComment.IndexOf("【发货");
                    int endIndex = order.PopSellerComment.IndexOf('】', startIndex < 0 ? 0 : startIndex);
                    if (startIndex >= 0 && endIndex > startIndex)
                    {
                        order.PopSellerComment = order.PopSellerComment.Replace(order.PopSellerComment.Substring(startIndex, endIndex - startIndex + 1), comment);
                    }
                    else
                    {
                        order.PopSellerComment = order.PopSellerComment + comment;
                    }
                    objsToUpdate.Add(order);
                    if (order.ShopId < 1 || string.IsNullOrWhiteSpace(order.PopOrderId))
                    {
                        continue;
                    }
                    //标记平台发货
                    try
                    {
                        Shop s = ServiceContainer.GetService<ShopService>().GetById(order.ShopId).First;
                        if (s == null)
                        {
                            throw new Exception("订单:" + order.Id + "店铺信息不存在");
                        }

                        if (s.AppEnabled == false)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(s.AppKey) || string.IsNullOrWhiteSpace(s.AppSecret) || string.IsNullOrWhiteSpace(s.AppAccessToken))
                        {
                            throw new Exception("店铺授权信息不完整");
                        }

                        this.ps.MarkDelivery(s, order.PopOrderId, order.PopPayType, order.DeliveryCompany, order.DeliveryNumber);
                        if (order.PopPayType == PopPayType.ONLINE && order.PopDeliveryTime <= GetDbMinTime())//在线支付才更新
                        {
                            order.PopDeliveryTime = DateTime.Now;
                        }
                    }
                    catch
                    {
                        if (chkPopState)
                        {
                            throw;
                        }
                    }
                }
                //删除以前相同的发货信息
                ServiceContainer.GetService<DeliveryOutService>().DeleteOrderDeliveryOut(deliveryNumber);
                //生成发货信息
                var deliveryOut = new DeliveryOut
                {
                    CreateTime = DateTime.Now,
                    DeliveryCompany = allOrders[0].DeliveryCompany,
                    DeliveryNumber = allOrders[0].DeliveryNumber,
                    Operator = op,
                    OrderId = string.Join(",", allOrders.Select(obj => obj.Id.ToString())),
                    ERPDeliveryMoney = (float)deliveryMoney,
                    ERPGoodsMoney = normalOgs.Select(obj => obj.Price * obj.Count).Sum(),
                    PopGoodsMoney = normalOrders.Select(obj => obj.PopSellerGetMoney).Sum(),
                    PopDeliveryMoney = 0,
                    PopPayType = mainOrder.PopPayType,
                    ReceiverAddress = mainOrder.ReceiverAddress,
                    ShopId = mainOrder.ShopId,
                    Weight = goodsCount,
                    PopCodSevFee = normalOrders.Select(obj => obj.PopCodSevFee).Sum(),
                    GoodsInfo = string.Join(",", normalOgs.Select(obj => VendorService.FormatVendorName(obj.Vendor) + " " + obj.Number + " " + obj.Edtion + " " + obj.Color + " " + obj.Size + " " + obj.Count)),
                    Shipper = normalOgs.Count > 0 ? normalOgs[0].Shipper : "",
                };
                if (deliveryOut.GoodsInfo.Length > 1000)
                {
                    deliveryOut.GoodsInfo = deliveryOut.GoodsInfo.Substring(0, 990);
                }
                this.dao.Update(objsToUpdate.ToArray());
                this.dao.Save(deliveryOut);
                return new DataCollectionResponse<Order>(allOrders);
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/markpopdelivery.html")]
        public ResponseBase MarkPopDelivery(long id, string time)
        {
            try
            {
                var or = this.GetByIdWithException(id);
                if (string.IsNullOrWhiteSpace(or.DeliveryCompany) || string.IsNullOrWhiteSpace(or.DeliveryNumber))
                {
                    throw new Exception("本地订单物流信息为空");
                }

                if (string.IsNullOrWhiteSpace(or.PopOrderId))
                {
                    throw new Exception("订单平台编号为空");
                }

                if ((int)or.State >= (int)OrderState.PRINTED && (int)or.State <= (int)OrderState.SHIPPED)
                {
                    int ret = 0;
                    if (string.IsNullOrWhiteSpace(time))
                    {
                        Shop s = ServiceContainer.GetService<ShopService>().GetById(or.ShopId).First;
                        if (s == null)
                        {
                            throw new Exception("订单店铺信息不存在");
                        }

                        if (s.AppEnabled == false)
                        {
                            throw new Exception("店铺接口已禁用，无法调用相应接口操作");
                        }

                        if (string.IsNullOrWhiteSpace(s.AppKey) || string.IsNullOrWhiteSpace(s.AppSecret) || string.IsNullOrWhiteSpace(s.AppAccessToken))
                        {
                            throw new Exception("订单店铺授权信息为空");
                        }

                        this.ps.MarkDelivery(s, or.PopOrderId, or.PopPayType, or.DeliveryCompany, or.DeliveryNumber);
                        if (this.dao.IsLessDBMinDate(or.PopDeliveryTime))
                        {
                            ret = this.dao.ExcuteSqlUpdate("update `Order` set PopDeliveryTime='" + this.FormatTime(DateTime.Now) + "' where Id=" + id);
                        }
                    }
                    else
                    {
                        ret = this.dao.ExcuteSqlUpdate("update `Order` set PopDeliveryTime='" + time + "' where Id=" + id);
                    }
                }
                else
                {
                    throw new Exception("本地订单状态不对");
                }
                return ResponseBase.SUCCESS;
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<Order> GetByAll(string popBuyerId, string receiverMobile,
            string receiverName, string receiverAddress, DateTime startTime, DateTime endTime, string deliveryCompany, string deliveryNumber,
            OrderState state, PopPayType payType, string vendorName, string number, string size,
            ColorFlag[] ofs, int parseResult, string comment, long shopId, OrderCreateType createType, OrderType type, string shipper,
            int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(popBuyerId, receiverMobile, receiverName, receiverAddress, startTime, endTime, deliveryCompany, deliveryNumber, state, payType, vendorName, number, size, ofs, parseResult, comment, shopId, createType, type, shipper, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getpayedandprintedorders.html")]
        public DataCollectionResponse<Order> GetPayedAndPrintedOrders(long[] shopId, OrderCreateType createType, PopPayType payType, string shipper, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetPayedAndPrintedOrders(shopId, createType, payType, shipper, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getordersbyinfoidnotequal.html")]
        public DataCollectionResponse<Order> GetOrdersByInfoIdNotEqual(string popBuyerId, string receiverPhone, string receiverMobile, string receiverAddress, long id)
        {
            try
            {
                return this.dao.GetOrdersByInfoIDNotEqual(popBuyerId, receiverPhone, receiverMobile, receiverAddress, id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/closeorder.html")]
        public ResponseBase CloseOrder(long orderId, long orderGoodsId, int count)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                Order or = this.GetByIdWithException(orderId);
                if (or == null)
                {
                    throw new Exception("指定的订单不存在");
                }

                if (or.State == OrderState.SHIPPED)
                {
                    if (DateTime.Now.Subtract(or.DeliveryTime).TotalHours >= 24)
                    {
                        throw new Exception("发货24时小时后的订单不能关闭");
                    }
                }

                if ((int)or.State > (int)OrderState.SHIPPED)
                {
                    throw new Exception("该状态下不能关闭订单");
                }

                var ogs = or.OrderGoodss.Where(obj => obj.State != OrderState.SPILTED && obj.State != OrderState.CLOSED && obj.State != OrderState.CANCLED).ToList();
                var og = ogs.FirstOrDefault(obj => obj.Id == orderGoodsId);

                if (orderGoodsId > 0 && og == null)
                {
                    throw new Exception("订单中的商品不存在");
                }

                //关闭单个商品
                if (orderGoodsId > 0 && (ogs.Count > 1 || count < og.Count))
                {
                    if (count > og.Count)
                    {
                        throw new Exception("要关闭的数量不能大于商品数量");
                    }
                    if (count >= og.Count)
                    {
                        //单个商品全关
                        og.State = OrderState.CLOSED;
                        og.CloseTime = DateTime.Now;
                        og.CloseOperator = op;
                    }
                    else
                    {
                        //关闭部分
                        og.Count -= count;
                        og.GetedCount = og.GetedCount > og.Count ? og.Count : og.GetedCount;
                    }
                    og.StockOperator = op;
                    og.StockTime = DateTime.Now;
                    this.dao.Update(og, or);
                }
                else
                {
                    foreach (var ogg in ogs)
                    {
                        ogg.State = OrderState.CLOSED;
                        ogg.CloseTime = DateTime.Now;
                        ogg.CloseOperator = op;
                    }
                    this.dao.Update(ogs.ToArray());
                    or.State = OrderState.CLOSED;
                    or.CloseTime = DateTime.Now;
                    or.CloseOperator = op;
                    if (this.IsDbMinTime(or.DeliveryTime) == false)
                    {
                        //删除发货记录
                        ServiceContainer.GetService<DeliveryOutService>().DeleteOrderDeliveryOut(or.DeliveryNumber);
                    }
                    or.DeliveryTime = this.GetDbMinTime();
                    or.DeliveryOperator = "";
                    this.dao.Update(or);
                }
                return ResponseBase.SUCCESS;
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/spilteordergoods.html")]
        public ResponseBase SpilteOrderGoods(long orderId, OrderSpilteInfo[] infos)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                Order or = this.GetByIdWithException(orderId);
                // 检查信息合法性
                var ogs = or.OrderGoodss.Where(obj => (int)obj.State < (int)OrderState.SHIPPED).ToList();
                if (ogs.Count < 1)
                {
                    throw new Exception("没有商品可以拆分");
                }

                foreach (var spilteInfo in infos)
                {
                    OrderGoods og = ogs.FirstOrDefault(obj => obj.Id == spilteInfo.OrderGoodsId);
                    if (og == null)
                    {
                        throw new Exception("订单商品不存在:" + spilteInfo.OrderGoodsId);
                    }

                    if ((int)og.State < (int)OrderState.PAYED || (int)og.State > (int)OrderState.SHIPPED)
                    {
                        throw new Exception("订单商品状态不能被拆分:" + og.Id);
                    }

                    if (og.Count < spilteInfo.Count)
                    {
                        throw new Exception("要拆分的商品实际数量大:" + og.Id);
                    }
                }
                // 生成新订单
                Order nor = new Order
                {
                    PopBuyerComment = or.PopBuyerComment,
                    CloseOperator = "",
                    CloseTime = this.GetDbMinTime(),
                    State = OrderState.PAYED,
                    PrintTime = this.GetDbMinTime(),
                    ParseResult = true,
                    CreateTime = DateTime.Now,
                    DeliveryCompany = "",
                    DeliveryNumber = "",
                    DeliveryOperator = "",
                    DeliveryTime = this.GetDbMinTime(),
                    DeliveryMoney = 0,
                    PopDeliveryTime = or.PopDeliveryTime,
                    PopPayTime = or.PopPayTime,
                    OrderGoodss = new List<OrderGoods>(),
                    PopBuyerId = or.PopBuyerId,
                    PopCodNumber = or.PopCodNumber,
                    PopFlag = or.PopFlag,
                    PopOrderId = "",
                    PopOrderTotalMoney = 0,
                    PopPayType = or.PopPayType,
                    PopSellerComment = or.PopSellerComment + " 原订单:" + or.Id,
                    PopState = or.PopState,
                    PopType = or.PopType,
                    PrintOperator = "",
                    ReceiverAddress = or.ReceiverAddress,
                    ReceiverMobile = or.ReceiverMobile,
                    ReceiverName = or.ReceiverName,
                    ReceiverPhone = or.ReceiverPhone,
                    ShopId = or.ShopId,
                    Weight = 0,
                    CreateOperator = op,
                    PopCodSevFee = 0,
                    CreateType = OrderCreateType.MANUAL,
                    Type = or.Type,
                };

                List<Object> objsUpdate = new List<Object>();
                foreach (OrderSpilteInfo cuInfo in infos)
                {
                    OrderGoods og = ogs.FirstOrDefault(obj => obj.Id == cuInfo.OrderGoodsId);
                    OrderGoods nog = new OrderGoods
                    {
                        OrderId = 0,
                        Id = 0,
                        Count = cuInfo.Count,
                        State = OrderState.PAYED,
                        GetedCount = 0,
                        Price = og.Price,
                        CloseOperator = "",
                        CloseTime = this.GetDbMinTime(),
                        StockOperator = og.StockOperator,
                        StockTime = og.StockTime,
                        Comment = og.Comment,
                        Color = og.Color,
                        Edtion = og.Edtion,
                        Image = og.Image,
                        Number = og.Number,
                        GoodsId = og.GoodsId,
                        PopInfo = og.PopInfo,
                        PopOrderSubId = og.PopOrderSubId,
                        PopPrice = og.PopPrice,
                        PopUrl = og.PopUrl,
                        Size = og.Size,
                        Vendor = og.Vendor,
                        Weight = og.Weight,
                        Shipper = "",
                    };
                    nor.OrderGoodss.Add(nog);

                    if (og.Count <= cuInfo.Count)
                    {
                        og.State = OrderState.SPILTED;
                    }
                    else
                    {
                        og.Count -= cuInfo.Count;
                    }
                    og.CloseOperator = op;
                    og.CloseTime = DateTime.Now;
                }

                //新订单商品总额
                nor.Weight = nor.OrderGoodss.Select(obj => obj.Weight * obj.Count).Sum();
                nor.ParseResult = nor.OrderGoodss.Count(obj => obj.GoodsId <= 0) > 0 ? false : true;

                //旧订单
                ogs = or.OrderGoodss.Where(obj => obj.State != OrderState.SPILTED).ToList();
                or.Weight = ogs.Select(obj => obj.Weight * obj.Count).Sum();
                or.ParseResult = ogs.Count(obj => obj.GoodsId <= 0) > 0 ? false : true;

                // 保存数据
                try
                {
                    //更新老订单
                    List<object> objs = new List<object>();
                    objs.Add(or);
                    objs.AddRange(or.OrderGoodss.ToArray());
                    this.dao.Update(objs.ToArray());

                    //保存新订单
                    this.Save(nor);
                }
                catch
                {
                    if (nor.Id > 0)
                    {
                        this.dao.Delete(nor);
                    }
                    throw;
                }
                return ResponseBase.SUCCESS;
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/resetprintstate.html")]
        public ResponseBase ResetPrintState(long orderId)
        {
            var or = this.GetByIdWithException(orderId);
            this.UpdateDelivery(orderId, -1, "", "", this.GetDbMinTime());
            return ResponseBase.SUCCESS;
        }

        /// <summary>
        /// 修改备注和颜色旗帜
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="flag"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/modifypopsellercomment.html")]
        public ResponseBase ModifyPopSellerComment(long orderId, ColorFlag flag, string comment)
        {
            try
            {
                Order os = this.dao.GetById(orderId);
                if (os == null)
                {
                    throw new Exception("订单不存在");
                }
                Shop s = ServiceContainer.GetService<ShopService>().GetById(os.ShopId).First;
                if (s == null)
                {
                    throw new Exception("店铺信息不存在");
                }
                if ((s.PopType == PopType.TMALL || s.PopType == PopType.TAOBAO) && string.IsNullOrWhiteSpace(s.AppAccessToken) == false && string.IsNullOrWhiteSpace(os.PopOrderId) == false && s.AppEnabled)
                {
                    this.ps.ModifyComment(s, os.PopOrderId, comment, flag);
                }
                os.PopFlag = flag;
                os.PopSellerComment = comment;
                this.dao.Update(os);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 下载平台上的待发货订单
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="payType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/getpopwaitsendorders.html")]
        public OrderDownloadCollectionResponse GetPopWaitSendOrders(Shop shop, PopPayType payType, int pageIndex, int pageSize)
        {
            try
            {
                var ret = this.ps.GetOrders(shop, payType == PopPayType.COD ? PopService.QUERY_STATE_WAITSHIP_COD : PopService.QUERY_STATE_WAITSHIP, pageIndex, pageSize);
                var ret1 = SaveOrUpdateOrdersByPopOrderId(shop, ret.Datas);
                ret1.Total = ret.Total;
                ret1.IsTotalValid = ret.IsTotalValid;
                return ret1;
            }
            catch (WebFaultException<ResponseBase>)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 保存或者更新下载下来的订单
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="orders"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/saveorupdateordersbypoporderid.html")]
        public OrderDownloadCollectionResponse SaveOrUpdateOrdersByPopOrderId(Shop shop, List<OrderDownload> orders)
        {
            try
            {
                if (orders == null)
                {
                    throw new Exception("参数orders 为空");
                }

                foreach (var or in orders)
                {
                    if (or == null)
                    {
                        throw new Exception("OrderDownload有空参数");
                    }

                    if (or.Error != null)
                    {
                        continue;
                    }

                    if (or.Order == null)
                    {
                        or.Error = new OrderDownloadError(shop.Id, "", "", "保存或者更新订单错误:Order与Error均为空", "");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(or.Order.PopOrderId))
                    {
                        or.Error = new OrderDownloadError(shop.Id, "", "", "保存或者更新订单错误:PopOrderId为空", "");
                        continue;
                    }

                    try
                    {
                        //检查订单是否存在存
                        var count = GetColumnValueBySqlQuery<long>("select count(Id) from `Order` where PopOrderId='" + or.Order.PopOrderId + "'").First();
                        if (count > 1)
                        {
                            or.Error = new OrderDownloadError(shop.Id, or.Order.PopOrderId, or.Order.ReceiverName, "系统中存在2个及以上相同订单", "");
                            or.Order = null;
                        }
                        else if (count < 1)
                        {
                            Save(or.Order);
                        }
                        else
                        {
                            //本地已经有的需要更新，像退款的这些订单，也可能不需要更新但本地已经关闭，所以需要读取本地的
                            string upRet = UpdateOrderStateWithGoods(or.Order, null, shop).data;
                            or.Order = GetByPopOrderId(or.Order.PopOrderId).First;
                        }
                    }
                    catch (WebFaultException<ResponseBase> we)
                    {
                        or.Error = new OrderDownloadError(shop.Id, or.Order.PopOrderId, or.Order.ReceiverName, we.Detail.error, we.StackTrace);
                        or.Order = null;
                    }
                    catch (Exception ex)
                    {
                        or.Error = new OrderDownloadError(shop.Id, or.Order.PopOrderId, or.Order.ReceiverName, ex.Message, ex.StackTrace);
                        or.Order = null;
                    }
                }
                OrderDownloadCollectionResponse resp = new OrderDownloadCollectionResponse(orders);
                return resp;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 更新订单商品状态
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="orderGoodsId"></param>
        /// <param name="state"></param>
        /// <param name="stockComment"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/updateordergoodsstate.html")]
        public ResponseBase UpdateOrderGoodsState(long orderId, long orderGoodsId, OrderState state, string stockComment)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                Order order = this.GetByIdWithException(orderId);

                if (state != OrderState.GETED && state != OrderState.CHECKFAIL)
                {
                    throw new Exception("订单商品只能修改成已拿货或者检查未过");
                }

                if ((int)order.State < (int)OrderState.PAYED)
                {
                    throw new Exception("未付款订单不能更改");
                }

                if ((int)order.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("已经发货，不能更改");
                }

                OrderGoods og = order.OrderGoodss.FirstOrDefault(obj => obj.Id == orderGoodsId);
                if (og == null)
                {
                    throw new Exception("订单商品不存在");
                }
                if ((int)og.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单商品状态不允许修改");
                }
                og.State = state;
                og.Comment = stockComment;
                og.StockTime = DateTime.Now;
                og.StockOperator = op;

                if (state == OrderState.GETED)
                {
                    int val = int.Parse(stockComment.Replace("已拿", "").Replace("双", ""));
                    if (val > og.Count)
                    {
                        throw new Exception("拿货数量不能比总数量大");
                    }
                    og.GetedCount = val;
                }
                else
                {
                    og.GetedCount = 0;
                }
                this.dao.Update(og);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 将订单更新成已拿货
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/updateordertogeted.html")]
        public ResponseBase UpdateOrderToGeted(long orderId)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                var or = this.GetByIdWithException(orderId);
                if ((int)or.State < (int)OrderState.PRINTED || (int)or.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单状态不正确");
                }
                if (or.OrderGoodss == null || or.OrderGoodss.Count < 1)
                {
                    throw new Exception("订单没有商品不需要标记");
                }
                List<object> objs = new List<object>();
                or.State = OrderState.GETED;
                foreach (var og in or.OrderGoodss)
                {
                    if (((int)or.State < (int)OrderState.PAYED || (int)or.State > (int)OrderState.SUCCESS))
                    {
                        continue;
                    }
                    og.State = OrderState.GETED;
                    og.GetedCount = og.Count;
                    og.Comment = "已拿" + og.Count + "双";
                    og.StockOperator = op;
                    og.StockTime = DateTime.Now;
                    objs.Add(og);
                }
                objs.Add(or);
                this.dao.Update(objs.ToArray());
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// 更新订单状态
        /// </summary>
        /// <param name="orderOnline">网络上下载的最新订单</param>
        /// <param name="orderInDb">本地订单,如果传入空值，则从数据库读取</param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/updateorderstatewithgoods.html")]
        public StringResponse UpdateOrderStateWithGoods(Order orderOnline, OrderUpdate orderInDb, Shop shop)
        {
            try
            {
                OrderUpdateService ous = ServiceContainer.GetService<OrderUpdateService>();
                if (orderInDb == null)
                {
                    //检测数据库是否存在
                    var ret = ous.GetByAll(null, orderOnline.PopOrderId, DateTime.MinValue, DateTime.Now.AddDays(1), 0, 0);
                    if (ret == null || ret.Datas.Count < 1)
                    {
                        return new StringResponse(UPDATE_RET_NOEXIST);
                    }
                    orderInDb = ret.Datas[0];
                }

                //本地订单已经关闭，则不允许更新
                if (orderInDb.State == OrderState.CLOSED || orderInDb.State == OrderState.CANCLED)
                {
                    return new StringResponse(UPDATE_RET_NOUPDATED);
                }

                //(orderOnline.OrderGoodss == null || orderOnline.OrderGoodss.Count == 1) 表示订单有商品表不为1，多个有可能在退款中，则必须要更新
                if (orderOnline.State == orderInDb.State && orderOnline.PopState == orderInDb.PopState && (orderOnline.OrderGoodss == null || orderOnline.OrderGoodss.Count == 1) &&
                    Math.Abs(orderOnline.PopOrderTotalMoney - orderInDb.PopOrderTotalMoney) < 0.001F &&
                    Math.Abs(orderOnline.PopCodSevFee - orderInDb.PopCodSevFee) < 0.001F)
                {
                    return new StringResponse(UPDATE_RET_NOUPDATED);
                }

                orderInDb.PopState = orderOnline.PopState;
                orderInDb.PopCodNumber = orderOnline.PopCodNumber;
                orderInDb.PopPayTime = orderOnline.PopPayTime;
                orderInDb.PopOrderTotalMoney = orderOnline.PopOrderTotalMoney;
                orderInDb.PopCodSevFee = orderOnline.PopCodSevFee;
                var onlineState = orderOnline.State;
                OrderState targetState = orderInDb.State, dbState = orderInDb.State;

                if (onlineState == OrderState.WAITPAY)
                {
                    //待付款，不需要更新状态
                }
                else if (onlineState == OrderState.PAYED)
                {
                    if (orderInDb.State == OrderState.WAITPAY)
                    {
                        targetState = OrderState.PAYED;
                    }
                    else if ((orderInDb.State == OrderState.RETURNING) && ous.IsDbMinTime(orderInDb.DeliveryTime))
                    {
                        if (ous.IsDbMinTime(orderInDb.PrintTime) == false)
                        {
                            targetState = OrderState.PRINTED;
                        }
                        else
                        {
                            targetState = onlineState;
                        }
                    }
                }
                else if (onlineState == OrderState.SHIPPED)
                {
                    //如果在退款中，则标记为已发货
                    if (orderInDb.State == OrderState.RETURNING)
                    {
                        if (IsDbMinTime(orderInDb.DeliveryTime))
                        {
                            targetState = OrderState.PRINTED;
                        }
                        else
                        {
                            targetState = onlineState;
                        }
                    }

                    //已发货,且系统中的打印时间，则说明该订单不是系统打印的，需要更新状态，且同步物流
                    if (ous.IsDbMinTime(orderInDb.PrintTime))
                    {
                        targetState = onlineState;
                    }
                }
                else if (onlineState == OrderState.SUCCESS)
                {
                    //非本地打印
                    if (ous.IsDbMinTime(orderInDb.PrintTime))
                    {
                        targetState = onlineState;
                    }
                    else
                    {
                        //本地已经发货，则标记为，已完成，防止用户误确认收到，导致系统无法统计发货
                        if (ous.IsDbMinTime(orderInDb.DeliveryTime) == false)
                        {
                            targetState = onlineState;
                        }
                    }
                }
                else if (onlineState == OrderState.RETURNING || onlineState == OrderState.CANCLED || onlineState == OrderState.CLOSED)
                {
                    targetState = onlineState;
                }
                else
                {
                    Logger.Log("更新订单失败未知状态[" + onlineState + "]");
                    return new StringResponse("更新订单失败未知状态[" + onlineState + "]");
                }

                if (targetState == OrderState.NONE)
                {
                    throw new Exception("要更新成的订单状态不能为:" + targetState);
                }

                //由于多数订单中只有一个商品，所以采取不同试更新，理提供更新速度
                //多个订单
                if (orderOnline.OrderGoodss != null && orderOnline.OrderGoodss.Count > 1)
                {
                    var ogsInDb = ServiceContainer.GetService<OrderGoodsService>().GetByOrderId(orderInDb.Id).Datas.Where(obj => obj.State != OrderState.CANCLED && obj.State != OrderState.CLOSED && obj.State != OrderState.SPILTED).ToArray();
                    foreach (var og in ogsInDb)
                    {
                        var ogNewState = og.State;
                        if (string.IsNullOrWhiteSpace(og.PopOrderSubId))
                        {
                            //没有子订单编号，则更新订单状态
                            if (targetState != OrderState.PAYED && targetState != OrderState.PRINTED)
                            {
                                ogNewState = targetState;
                            }
                        }
                        else
                        {
                            var ogsOnLine = orderOnline.OrderGoodss.FirstOrDefault(obj => obj.PopOrderSubId == og.PopOrderSubId);
                            if (ogsOnLine == null)
                            {
                                throw new Exception(string.Format("更新失败：系统中子订单未在平台上找到,订单编号;{0},子订单编号;{1}", orderOnline.PopOrderId, og.PopOrderSubId));
                            }
                            if ((ogsOnLine.State != OrderState.PAYED && ogsOnLine.State != OrderState.PRINTED) || og.State == OrderState.RETURNING)
                            {
                                ogNewState = ogsOnLine.State;
                            }
                        }
                        if (ogNewState != og.State)
                            ous.UpdateOrderGoodsState(og.Id, ogNewState);
                    }
                }
                else
                {
                    if (targetState != orderInDb.State)
                    {
                        ous.UpdateOrderGoodsStateByOrderId(orderInDb.Id, targetState);
                    }
                }
                orderInDb.State = targetState;
                ous.UpdateEx(orderInDb, true);
                return new StringResponse(UPDATE_RET_UPDATED);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/updateorderstate.html")]
        public StringResponse UpdateOrderState(PopOrderState orderStateOnline, OrderUpdate orderInDb, Shop shop)
        {
            try
            {
                var ous = ServiceContainer.GetService<OrderUpdateService>();
                if (orderInDb == null)
                {
                    //检测数据库是否存在
                    var ret = ous.GetByAll(null, orderStateOnline.PopOrderId, DateTime.MinValue, DateTime.Now.AddDays(1), 0, 0);
                    if (ret == null || ret.Datas == null || ret.Datas.Count < 1)
                    {
                        return new StringResponse(UPDATE_RET_NOEXIST);
                    }
                    orderInDb = ret.Datas[0];
                }

                if (orderStateOnline.State == orderInDb.State && orderStateOnline.PopOrderStateValue == orderInDb.PopState)
                {
                    return new StringResponse(UPDATE_RET_NOUPDATED);
                }

                orderInDb.PopState = orderStateOnline.PopOrderStateValue;
                var onlineState = orderStateOnline.State;
                OrderState targetState = orderInDb.State, dbState = orderInDb.State;

                if (onlineState == OrderState.WAITPAY)
                {
                    //待付款，不需要更新状态
                }
                else if (onlineState == OrderState.PAYED)
                {
                    if (orderInDb.State == OrderState.WAITPAY)
                    {
                        targetState = OrderState.PAYED;
                    }
                    else if ((orderInDb.State == OrderState.RETURNING) && ous.IsDbMinTime(orderInDb.DeliveryTime))
                    {
                        if (ous.IsDbMinTime(orderInDb.PrintTime) == false)
                        {
                            targetState = OrderState.PRINTED;
                        }
                        else
                        {
                            targetState = onlineState;
                        }
                    }
                }
                else if (onlineState == OrderState.SHIPPED)
                {
                    //如果在退款中，则标记为已发货
                    if (orderInDb.State == OrderState.RETURNING)
                    {
                        if (IsDbMinTime(orderInDb.DeliveryTime))
                        {
                            targetState = OrderState.PRINTED;
                        }
                        else
                        {
                            targetState = onlineState;
                        }
                    }

                    //已发货,且系统中的打印时间，则说明该订单不是系统打印的，需要更新状态，且同步物流
                    if (ous.IsDbMinTime(orderInDb.PrintTime))
                    {
                        targetState = onlineState;
                    }
                }
                else if (onlineState == OrderState.SUCCESS)
                {
                    //非本地打印
                    if (ous.IsDbMinTime(orderInDb.PrintTime))
                    {
                        targetState = onlineState;
                    }
                    else
                    {
                        //本地已经发货，则标记为，已完成，防止用户误确认收到，导致系统无法统计发货
                        //如果打印时间超过15天没有发货一般不可能的，有可能是没有扫描到发货，这种也可以更新
                        //春节怎么办，时间上是超过15天的或者20天的？
                        if (ous.IsDbMinTime(orderInDb.DeliveryTime) == false || DateTime.Now.Subtract(orderInDb.PopPayTime).TotalDays >= 30)
                        {
                            targetState = onlineState;
                        }
                    }
                }
                else if ((int)onlineState > (int)OrderState.SHIPPED)
                {
                    targetState = onlineState;
                }
                else
                {
                    Logger.Log("更新订单失败未知状态[" + onlineState + "]");
                    return new StringResponse("更新订单失败未知状态[" + onlineState + "]");
                }

                if (targetState == OrderState.NONE)
                {
                    throw new Exception("要更新成的订单状态不能为:" + targetState);
                }

                //本地已经关闭的订单则不允许更新状态
                if (orderInDb.State != OrderState.CLOSED && orderInDb.State != OrderState.CANCLED)
                {
                    if (targetState != orderInDb.State)
                    {
                        ous.UpdateOrderGoodsStateByOrderId(orderInDb.Id, targetState);
                    }
                    orderInDb.State = targetState;
                }
                ous.UpdateEx(orderInDb, true);
                return new StringResponse(UPDATE_RET_UPDATED);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }
    }
}