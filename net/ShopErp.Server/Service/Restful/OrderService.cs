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
using ShopErp.Server.Utils;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OrderService : ServiceBase<Order, OrderDao>
    {
        protected static readonly char[] SPILTE_CHAR_COMMENT_L = new char[] { '(', '��' };
        protected static readonly char[] SPILTE_CHAR_COMMENT_R = new char[] { ')', '��' };
        protected static readonly char[] SPILTE_CHAR_EDTION_L = new char[] { '[', '��' };
        protected static readonly char[] SPILTE_CHAR_EDTION_R = new char[] { ']', '��' };

        private PopService ps = new PopService();

        private OrderGoodsDao ogDao = new OrderGoodsDao();

        private Order GetByIdWithException(long id)
        {
            Order or = this.dao.GetById(id);
            if (or == null)
            {
                throw new Exception("����������");
            }
            return or;
        }

        /// <summary>
        /// ȥ����ɫ����������()��[]��Χ������
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
                oEdtion = oEdtion.Replace("(", "").Replace("��", "").Replace(")", "").Replace("��", "").Replace("[", "").Replace("��", "").Replace("]", "").Replace("��", "").Replace(" ", "");
                oColor = oColor.Substring(0, oColor.IndexOfAny(SPILTE_CHAR_EDTION_L));
            }
            else
            {
                oColor = iColor;
                oEdtion = "";
            }
            if (string.IsNullOrWhiteSpace(oEdtion) == false)
            {
                oEdtion = oEdtion.Replace("��", "");
                oEdtion = oEdtion.Replace("����", "");
            }
        }

        private void FillEmptyAndParseGoods(Order order)
        {
            if (order.State == OrderState.NONE)
            {
                throw new Exception("����State����ΪNONE");
            }
            if (order.ShopId < 1)
            {
                throw new Exception("����ShopIdС��1");
            }
            if (order.CreateType == OrderCreateType.NONE)
            {
                throw new Exception("����CreateType����ΪNONE");
            }
            if (order.Type == OrderType.NONE)
            {
                throw new Exception("����Type����ΪNONE");
            }
            if (order.PopType == PopType.None)
            {
                throw new Exception("����PopType����ΪNONE");
            }
            if (order.PopPayType == PopPayType.None)
            {
                throw new Exception("����PopPayType����ΪNONE");
            }
            if (order.PopFlag == ColorFlag.None)
            {
                throw new Exception("����PopFlag����ΪNONE");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverName))
            {
                throw new Exception("����ReceiverName����Ϊ��");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverAddress))
            {
                throw new Exception("����ReceiverAddress����Ϊ��");
            }
            if (string.IsNullOrWhiteSpace(order.ReceiverPhone) && string.IsNullOrWhiteSpace(order.ReceiverMobile))
            {
                throw new Exception("����ReceiverPhone,��ReceiverMobile ����ͬʱΪ��");
            }
            order.PopOrderId = order.PopOrderId ?? string.Empty;
            order.PopBuyerId = order.PopBuyerId ?? string.Empty;
            order.PopCodSevFee = order.PopCodSevFee < 0 ? 0 : order.PopCodSevFee;
            order.PopCodNumber = order.PopCodNumber ?? string.Empty;
            order.PopSellerComment = StringUtils.FilterUnReadableChar(order.PopSellerComment);
            order.PopBuyerComment = StringUtils.FilterUnReadableChar(order.PopBuyerComment);
            order.PopState = order.PopState ?? String.Empty;
            order.DeliveryCompany = order.DeliveryCompany ?? string.Empty;
            order.DeliveryNumber = order.DeliveryNumber ?? string.Empty;
            order.PopPayTime = Utils.DateTimeUtil.IsDbMinTime(order.PopPayTime) ? DateTime.Now : order.PopPayTime;
            order.PopDeliveryTime = Utils.DateTimeUtil.IsDbMinTime(order.PopDeliveryTime) ? Utils.DateTimeUtil.DbMinTime : order.PopDeliveryTime;
            order.CreateTime = Utils.DateTimeUtil.IsDbMinTime(order.CreateTime) ? DateTime.Now : order.CreateTime;
            order.PrintTime = Utils.DateTimeUtil.IsDbMinTime(order.PrintTime) ? Utils.DateTimeUtil.DbMinTime : order.PrintTime;
            order.DeliveryTime = Utils.DateTimeUtil.IsDbMinTime(order.DeliveryTime) ? Utils.DateTimeUtil.DbMinTime : order.DeliveryTime;
            order.CloseTime = Utils.DateTimeUtil.IsDbMinTime(order.CloseTime) ? Utils.DateTimeUtil.DbMinTime : order.CloseTime;
            order.CreateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number;
            order.PrintOperator = order.PrintOperator ?? string.Empty;
            order.DeliveryOperator = order.DeliveryOperator ?? string.Empty;
            order.CloseOperator = order.CloseOperator ?? string.Empty;
            order.ParseResult = true;
            order.ReceiverAddress = StringUtils.FilterUnReadableChar(new string(order.ReceiverAddress.Where(c => c != '\r' && c != '\n').ToArray()));
            order.ReceiverName = StringUtils.FilterUnReadableChar(order.ReceiverName);
            if (order.OrderGoodss != null && order.OrderGoodss.Count > 0)
            {
                foreach (var item in order.OrderGoodss)
                {
                    if (item.State == OrderState.NONE)
                    {
                        throw new Exception("������Ʒ״̬����ΪNONE��" + item.Vendor + " " + item.Number);
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
                    item.CloseTime = Utils.DateTimeUtil.IsDbMinTime(item.CloseTime) ? Utils.DateTimeUtil.DbMinTime : item.CloseTime;
                    item.CloseOperator = item.CloseOperator ?? string.Empty;
                    item.Comment = item.Comment ?? string.Empty;
                    item.StockTime = Utils.DateTimeUtil.IsDbMinTime(item.StockTime) ? Utils.DateTimeUtil.DbMinTime : item.StockTime;
                    item.StockOperator = item.StockOperator ?? string.Empty;
                    item.Image = item.Image ?? string.Empty;
                    string color = null, edtion = null, size = null;
                    ParseColorSizeEditon(item.Color, item.Size, out color, out edtion, out size);
                    item.Color = String.IsNullOrWhiteSpace(color) ? item.Color : color;
                    item.Edtion = string.IsNullOrWhiteSpace(edtion) ? item.Edtion : edtion;
                    item.Size = string.IsNullOrWhiteSpace(size) ? item.Size : size;
                    ServiceContainer.GetService<GoodsService>().ParsePopOrderGoodsNumber(item);
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
                    //������ݿ��Ƿ����
                    var count = this.dao.GetColumnValueBySqlQuery<long>("select count(Id) from `Order` where PopOrderId='" + value.PopOrderId + "'").First();
                    if (count > 0)
                    {
                        throw new Exception("������ţ�" + value.PopOrderId + "�Ѿ�����");
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
                    throw new Exception("����δ�����������ֱ�Ӹ���");
                }

                FillEmptyAndParseGoods(value);
                this.dao.Update(value);

                //ɾ����ǰ�ģ����ڶ�����û����Ʒ
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
                //��ݵ���Ϊ�գ���ʾ��Ҫ���ô�ӡ
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
                    or.PrintTime = Utils.DateTimeUtil.DbMinTime;
                    or.PrintOperator = op;
                    this.dao.Update(or);
                }
                else
                {
                    foreach (var og in or.OrderGoodss)
                    {
                        if (og.State == OrderState.PAYED || (Utils.DateTimeUtil.IsDbMinTime(or.DeliveryTime) && or.State == OrderState.RETURNING))
                        {
                            og.State = OrderState.PRINTED;
                            this.dao.Update(og);
                        }
                    }
                    if (or.State == OrderState.PAYED || (Utils.DateTimeUtil.IsDbMinTime(or.DeliveryTime) && or.State == OrderState.RETURNING))
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
                    throw new Exception("��ݵ���δ�ҵ�����");
                }

                //���Ҫ���״̬�������״̬����ȷ�Ķ�������Ϊ��ʱ��һ����ͬʱ��˫������ĳЩ���ܲ�Ҫ�ˡ�
                if (chkLocalState)
                {
                    allOrders = allOrders.Where(obj => (int)obj.State >= (int)OrderState.PRINTED && (int)obj.State <= (int)OrderState.SHIPPED).ToList();

                    //������ж������Ѿ����ˣ���˵�����ж���״̬������ȷ��
                    if (allOrders.Count < 1)
                    {
                        throw new Exception("����״̬����ȷ");
                    }
                }

                //��������
                var normalOrders = allOrders.Where(obj => obj.Type != OrderType.SHUA).ToArray();

                //����������Ϣ���
                if (normalOrders.Select(obj => obj.ShopId).Distinct().Count() > 1)
                {
                    throw new Exception("��������ҵ��̲�һ��");
                }

                if (normalOrders.Select(obj => obj.ReceiverName).Distinct().Count() > 1)
                {
                    throw new Exception("����������ջ���������һ��");
                }

                if (normalOrders.Select(obj => obj.ReceiverPhone).Distinct().Count() > 1)
                {
                    throw new Exception("����������ջ��˵绰��һ��");
                }

                if (normalOrders.Select(obj => obj.ReceiverMobile).Distinct().Count() > 1)
                {
                    throw new Exception("����������ջ����ֻ���һ��");
                }

                if (normalOrders.Select(obj => obj.ReceiverAddress.Trim()).Distinct().Count() > 1)
                {
                    throw new Exception("����������ջ��˵�ַ��һ��");
                }

                if (normalOrders.Select(obj => obj.PopPayType).Distinct().Count() > 1)
                {
                    throw new Exception("��������Ҹ������Ͳ�һ��");
                }

                //�ϲ�����Ч��������Ʒ
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
                    throw new Exception("��Ʒ������ƥ��");
                }
                foreach (var og in normalOgs)
                {
                    if (normalOgs.Any(obj => obj.Shipper.Length > og.Shipper.Length ? obj.Shipper.IndexOf(og.Shipper, StringComparison.OrdinalIgnoreCase) < 0 : og.Shipper.IndexOf(obj.Shipper, StringComparison.OrdinalIgnoreCase) < 0))
                    {
                        throw new Exception("��ͬ�ֿⲻ����һ�����ŷ���");
                    }
                }

                //��һ�������Ķ�������������ǣ���Ĭ��Ϊ��һ������
                var mainOrder = normalOrders.Length > 0 ? normalOrders[0] : allOrders[0];
                //�����ݷ���
                double deliveryMoney = ServiceContainer.GetService<DeliveryTemplateService>().ComputeDeliveryMoneyImplByCount(mainOrder.DeliveryCompany, mainOrder.ReceiverAddress, mainOrder.Type != OrderType.NORMAL, mainOrder.PopPayType, goodsCount);

                //���¶���״̬���˷ѽ����Ϣ
                List<object> objsToUpdate = new List<object>(normalOgs);
                foreach (OrderGoods og in normalOgs)
                {
                    og.State = OrderState.SHIPPED;
                }

                string comment = string.Format("������{0}��", DateTime.Now.ToString("MM-dd HH:mm"));
                foreach (var order in allOrders)
                {
                    order.DeliveryMoney = (float)Math.Round(deliveryMoney / (normalOrders.Length > 0 ? normalOrders.Length : allOrders.Count), 2);
                    order.Weight = 0;
                    order.DeliveryTime = DateTime.Now;
                    order.DeliveryOperator = op;
                    order.State = (order.State == OrderState.SUCCESS) ? OrderState.SUCCESS : OrderState.SHIPPED;
                    //��鵱ǰ�Ƿ��б�Ƿ�����Ϣ
                    int startIndex = order.PopSellerComment.IndexOf("������");
                    int endIndex = order.PopSellerComment.IndexOf('��', startIndex < 0 ? 0 : startIndex);
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
                    //���ƽ̨����
                    try
                    {
                        Shop s = ServiceContainer.GetService<ShopService>().GetById(order.ShopId).First;
                        if (s == null)
                        {
                            throw new Exception("����:" + order.Id + "������Ϣ������");
                        }

                        if (s.AppEnabled == false)
                        {
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(s.AppKey) || string.IsNullOrWhiteSpace(s.AppSecret) || string.IsNullOrWhiteSpace(s.AppAccessToken))
                        {
                            throw new Exception("������Ȩ��Ϣ������");
                        }

                        this.ps.MarkDelivery(s, order.PopOrderId, order.PopPayType, order.DeliveryCompany, order.DeliveryNumber);
                        if (order.PopPayType == PopPayType.ONLINE && order.PopDeliveryTime <= Utils.DateTimeUtil.DbMinTime)//����֧���Ÿ���
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
                //ɾ����ǰ��ͬ�ķ�����Ϣ
                ServiceContainer.GetService<DeliveryOutService>().DeleteOrderDeliveryOut(deliveryNumber);
                //���ɷ�����Ϣ
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
                    throw new Exception("���ض���������ϢΪ��");
                }

                if (string.IsNullOrWhiteSpace(or.PopOrderId))
                {
                    throw new Exception("����ƽ̨���Ϊ��");
                }

                if ((int)or.State >= (int)OrderState.PRINTED && (int)or.State <= (int)OrderState.SHIPPED)
                {
                    int ret = 0;
                    if (string.IsNullOrWhiteSpace(time))
                    {
                        Shop s = ServiceContainer.GetService<ShopService>().GetById(or.ShopId).First;
                        if (s == null)
                        {
                            throw new Exception("����������Ϣ������");
                        }

                        if (s.AppEnabled == false)
                        {
                            throw new Exception("���̽ӿ��ѽ��ã��޷�������Ӧ�ӿڲ���");
                        }

                        if (string.IsNullOrWhiteSpace(s.AppKey) || string.IsNullOrWhiteSpace(s.AppSecret) || string.IsNullOrWhiteSpace(s.AppAccessToken))
                        {
                            throw new Exception("����������Ȩ��ϢΪ��");
                        }

                        this.ps.MarkDelivery(s, or.PopOrderId, or.PopPayType, or.DeliveryCompany, or.DeliveryNumber);
                        if (Utils.DateTimeUtil.IsDbMinTime(or.PopDeliveryTime))
                        {
                            ret = this.dao.ExcuteSqlUpdate("update `Order` set PopDeliveryTime='" + Utils.DateTimeUtil.FormatDateTime(DateTime.Now) + "' where Id=" + id);
                        }
                    }
                    else
                    {
                        ret = this.dao.ExcuteSqlUpdate("update `Order` set PopDeliveryTime='" + time + "' where Id=" + id);
                    }
                }
                else
                {
                    throw new Exception("���ض���״̬����");
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
                    throw new Exception("ָ���Ķ���������");
                }

                if (or.State == OrderState.SHIPPED)
                {
                    if (DateTime.Now.Subtract(or.DeliveryTime).TotalHours >= 24)
                    {
                        throw new Exception("����24ʱСʱ��Ķ������ܹر�");
                    }
                }

                if ((int)or.State > (int)OrderState.SHIPPED)
                {
                    throw new Exception("��״̬�²��ܹرն���");
                }

                var ogs = or.OrderGoodss.Where(obj => obj.State != OrderState.SPILTED && obj.State != OrderState.CLOSED).ToList();
                var og = ogs.FirstOrDefault(obj => obj.Id == orderGoodsId);

                if (orderGoodsId > 0 && og == null)
                {
                    throw new Exception("�����е���Ʒ������");
                }

                //�رյ�����Ʒ
                if (orderGoodsId > 0 && (ogs.Count > 1 || count < og.Count))
                {
                    if (count > og.Count)
                    {
                        throw new Exception("Ҫ�رյ��������ܴ�����Ʒ����");
                    }
                    if (count >= og.Count)
                    {
                        //������Ʒȫ��
                        og.State = OrderState.CLOSED;
                        og.CloseTime = DateTime.Now;
                        og.CloseOperator = op;
                    }
                    else
                    {
                        //�رղ���
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
                    if (Utils.DateTimeUtil.IsDbMinTime(or.DeliveryTime) == false)
                    {
                        //ɾ��������¼
                        ServiceContainer.GetService<DeliveryOutService>().DeleteOrderDeliveryOut(or.DeliveryNumber);
                    }
                    or.DeliveryTime = Utils.DateTimeUtil.DbMinTime;
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
                // �����Ϣ�Ϸ���
                var ogs = or.OrderGoodss.Where(obj => (int)obj.State < (int)OrderState.SHIPPED).ToList();
                if (ogs.Count < 1)
                {
                    throw new Exception("û����Ʒ���Բ��");
                }

                foreach (var spilteInfo in infos)
                {
                    OrderGoods og = ogs.FirstOrDefault(obj => obj.Id == spilteInfo.OrderGoodsId);
                    if (og == null)
                    {
                        throw new Exception("������Ʒ������:" + spilteInfo.OrderGoodsId);
                    }

                    if ((int)og.State < (int)OrderState.PAYED || (int)og.State > (int)OrderState.SHIPPED)
                    {
                        throw new Exception("������Ʒ״̬���ܱ����:" + og.Id);
                    }

                    if (og.Count < spilteInfo.Count)
                    {
                        throw new Exception("Ҫ��ֵ���Ʒʵ��������:" + og.Id);
                    }
                }
                // �����¶���
                Order nor = new Order
                {
                    PopBuyerComment = or.PopBuyerComment,
                    CloseOperator = "",
                    CloseTime = Utils.DateTimeUtil.DbMinTime,
                    State = OrderState.PAYED,
                    PrintTime = Utils.DateTimeUtil.DbMinTime,
                    ParseResult = true,
                    CreateTime = DateTime.Now,
                    DeliveryCompany = "",
                    DeliveryNumber = "",
                    DeliveryOperator = "",
                    DeliveryTime = Utils.DateTimeUtil.DbMinTime,
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
                    PopSellerComment = or.PopSellerComment + " ԭ����:" + or.Id,
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
                        CloseTime = Utils.DateTimeUtil.DbMinTime,
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

                //�¶�����Ʒ�ܶ�
                nor.Weight = nor.OrderGoodss.Select(obj => obj.Weight * obj.Count).Sum();
                nor.ParseResult = nor.OrderGoodss.Count(obj => obj.GoodsId <= 0) > 0 ? false : true;

                //�ɶ���
                ogs = or.OrderGoodss.Where(obj => obj.State != OrderState.SPILTED).ToList();
                or.Weight = ogs.Select(obj => obj.Weight * obj.Count).Sum();
                or.ParseResult = ogs.Count(obj => obj.GoodsId <= 0) > 0 ? false : true;

                // ��������
                try
                {
                    //�����϶���
                    List<object> objs = new List<object>();
                    objs.Add(or);
                    objs.AddRange(or.OrderGoodss.ToArray());
                    this.dao.Update(objs.ToArray());

                    //�����¶���
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
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/modifyordergoodsprice.html")]
        public ResponseBase ModifyOrderGoodsPrice(long orderGoodsId, float price)
        {
            try
            {
                var og = this.ogDao.GetById(orderGoodsId);
                if (og == null)
                {
                    throw new Exception("������Ʒ��ţ�" + orderGoodsId + " δ�ҵ�");
                }
                og.Price = price;
                this.ogDao.Update(og);
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
            this.UpdateDelivery(orderId, -1, "", "", Utils.DateTimeUtil.DbMinTime);
            return ResponseBase.SUCCESS;
        }

        /// <summary>
        /// �޸ı�ע����ɫ����
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
                    throw new Exception("����������");
                }
                Shop s = ServiceContainer.GetService<ShopService>().GetById(os.ShopId).First;
                if (s == null)
                {
                    throw new Exception("������Ϣ������");
                }
                if (string.IsNullOrWhiteSpace(s.AppAccessToken) == false && string.IsNullOrWhiteSpace(os.PopOrderId) == false && s.AppEnabled)
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
        /// ����ƽ̨�ϵĴ���������
        /// </summary>
        /// <param name="shop"></param>
        /// <param name="payType"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/getpopwaitsendorders.html")]
        public OrderDownloadCollectionResponse GetPopWaitSendOrders(Shop shop, PopPayType payType, DateTime dateTime, int pageIndex, int pageSize)
        {
            try
            {
                var ret = this.ps.GetOrders(shop, payType == PopPayType.COD ? PopService.QUERY_STATE_WAITSHIP_COD : PopService.QUERY_STATE_WAITSHIP, dateTime, pageIndex, pageSize);
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

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/getpoporderstate.html")]
        public DataCollectionResponse<PopOrderState> GetPopOrderState(Shop shop, string popOrderId)
        {
            try
            {
                if (shop.AppEnabled == false)
                {
                    throw new Exception("��������û�п����ӿڹ����޷���ȡ����״̬");
                }
                var nor = ps.GetOrderState(shop, popOrderId);
                return new DataCollectionResponse<PopOrderState>(nor);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// ������߸������������Ķ���
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
                    throw new Exception("����orders Ϊ��");
                }

                foreach (var or in orders)
                {
                    if (or == null)
                    {
                        throw new Exception("OrderDownload�пղ���");
                    }

                    if (or.Error != null)
                    {
                        continue;
                    }

                    if (or.Order == null)
                    {
                        or.Error = new OrderDownloadError(shop.Id, "", "", "������߸��¶�������:Order��Error��Ϊ��", "");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(or.Order.PopOrderId))
                    {
                        or.Error = new OrderDownloadError(shop.Id, "", "", "������߸��¶�������:PopOrderIdΪ��", "");
                        continue;
                    }

                    try
                    {
                        //��鶩���Ƿ���ڴ�
                        var count = GetColumnValueBySqlQuery<long>("select count(Id) from `Order` where PopOrderId='" + or.Order.PopOrderId + "'").First();
                        if (count > 1)
                        {
                            or.Error = new OrderDownloadError(shop.Id, or.Order.PopOrderId, or.Order.ReceiverName, "ϵͳ�д���2����������ͬ����", "");
                            or.Order = null;
                        }
                        else if (count < 1)
                        {
                            Save(or.Order);
                        }
                        else
                        {
                            //�����Ѿ��е���Ҫ���£����˿����Щ������Ҳ���ܲ���Ҫ���µ������Ѿ��رգ�������Ҫ��ȡ���ص�
                            UpdateOrderState(or.Order.PopOrderId, or.Order.State, null, shop);
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
        /// ���¶�����Ʒ״̬
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
                Order order = this.GetByIdWithException(orderId);
                if ((int)order.State < (int)OrderState.PAYED || (int)order.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("δ��������ѷ����Ķ��������޸�״̬");
                }
                OrderGoods og = order.OrderGoodss.FirstOrDefault(obj => obj.Id == orderGoodsId);
                if (og == null)
                {
                    throw new Exception("������Ʒ������");
                }
                og.State = state;
                og.Comment = stockComment;
                og.StockTime = DateTime.Now;
                og.StockOperator = ServiceContainer.GetCurrentLoginInfo().op.Number;
                og.GetedCount = state == OrderState.GETED ? int.Parse(stockComment.Replace("����", "").Replace("˫", "")) : 0;
                this.dao.Update(og);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        /// <summary>
        /// ���������³����û�
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/updateordergoodstogeted.html")]
        public ResponseBase UpdateOrderGoodsStateToGeted(long orderId)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                var or = this.GetByIdWithException(orderId);
                if ((int)or.State < (int)OrderState.PRINTED || (int)or.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("����״̬����ȷ");
                }
                if (or.OrderGoodss == null || or.OrderGoodss.Count < 1)
                {
                    throw new Exception("����û����Ʒ����Ҫ���");
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
                    og.Comment = "����" + og.Count + "˫";
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

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, UriTemplate = "/updateorderstate.html")]
        public DataOneResponse<OrderState> UpdateOrderState(string popOrderid, OrderState onlineOrderState, OrderUpdate orderInDb, Shop shop)
        {
            try
            {
                var ous = ServiceContainer.GetService<OrderUpdateService>();
                if (orderInDb == null)
                {
                    //������ݿ��Ƿ����
                    var ret = ous.GetByAll(null, popOrderid, OrderType.NONE, Utils.DateTimeUtil.DbMinTime, DateTime.Now.AddDays(1), 0, 0);
                    if (ret == null || ret.Datas == null || ret.Datas.Count < 1)
                    {
                        return new DataOneResponse<OrderState>(OrderState.NONE);
                    }
                    orderInDb = ret.Datas[0];
                }
                if (orderInDb.State == OrderState.CLOSED || onlineOrderState == orderInDb.State)
                {
                    return new DataOneResponse<OrderState>(orderInDb.State);
                }

                OrderState targetState = orderInDb.State, dbState = orderInDb.State;

                if (onlineOrderState == OrderState.PAYED)
                {
                    //������������˿�ȡ���ˡ�
                    if ((orderInDb.State == OrderState.RETURNING) && Utils.DateTimeUtil.IsDbMinTime(orderInDb.DeliveryTime))
                    {
                        targetState = Utils.DateTimeUtil.IsDbMinTime(orderInDb.PrintTime) ? OrderState.PAYED : OrderState.PRINTED;
                    }
                }
                else if (onlineOrderState == OrderState.SHIPPED)
                {
                    //������˿��У�����Ϊ�ѷ������˿���ȡ��
                    if (orderInDb.State == OrderState.RETURNING)
                    {
                        targetState = Utils.DateTimeUtil.IsDbMinTime(orderInDb.DeliveryTime) ? OrderState.PRINTED : OrderState.SHIPPED;
                    }
                    else
                    {
                        //�ѷ���,�ö�������ϵͳ��ӡ�ģ���ͨ������;�������ģ���Ҫ����״̬
                        if (Utils.DateTimeUtil.IsDbMinTime(orderInDb.PrintTime))
                        {
                            targetState = OrderState.SHIPPED;
                        }
                    }
                }
                else if (onlineOrderState == OrderState.SUCCESS)
                {
                    //�Ǳ��ش�ӡ
                    if (Utils.DateTimeUtil.IsDbMinTime(orderInDb.PrintTime))
                    {
                        targetState = onlineOrderState;
                    }
                    else
                    {
                        if (Utils.DateTimeUtil.IsDbMinTime(orderInDb.DeliveryTime) == false)
                        {
                            targetState = onlineOrderState;
                        }
                    }
                }
                else if (onlineOrderState == OrderState.RETURNING || onlineOrderState == OrderState.CLOSED)
                {
                    targetState = onlineOrderState;
                }
                else
                {
                    throw new Exception("����״̬���ԣ�" + onlineOrderState);
                }

                if (targetState == OrderState.NONE)
                {
                    throw new Exception("Ҫ���³ɵĶ���״̬����Ϊ:" + targetState);
                }

                if (targetState == orderInDb.State)
                {
                    return new DataOneResponse<OrderState>(orderInDb.State);
                }
                this.dao.UpdateOrderState(orderInDb.Id, targetState);
                return new DataOneResponse<OrderState>(targetState);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }
    }
}