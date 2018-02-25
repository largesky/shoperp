using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OrderReturnService : ServiceBase<OrderReturn, OrderReturnDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<OrderReturn> GetById(long id)
        {
            try
            {
                var item = this.dao.GetById(id);
                return new DataCollectionResponse<OrderReturn>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(OrderReturn value)
        {
            try
            {
                value.CreateTime = DateTime.Now;
                this.dao.Save(value);
                return new LongResponse(value.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public ResponseBase Update(OrderReturn value)
        {
            try
            {
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }
                this.dao.Update(value);
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
                this.dao.ExcuteSqlUpdate("delete from OrderReturn where Id=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<OrderReturn> GetByAll(long id, long orderId, string vendor, string number, string deliveryNumber, OrderReturnState state, OrderReturnType type, int timeType, DateTime start, DateTime end, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(id, orderId, vendor, number, deliveryNumber, state, type, timeType, start, end, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyorderidandordergoodsid.html")]
        public DataCollectionResponse<OrderReturn> GetByOrderIdAndOrderGoodsId(long orderId, long orderGoodsId)
        {
            try
            {
                return new DataCollectionResponse<OrderReturn>(this.dao.GetByOrderIdAndOrderGoodsId(orderId, orderGoodsId).Datas);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/create.html")]
        public LongResponse Create(long orderId, long orderGoodsId, string deliveryCompany, string deliveryNumber, OrderReturnType type, OrderReturnReason reason, int count)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                var ors = this.GetByOrderIdAndOrderGoodsId(orderId, orderGoodsId);
                OrderReturn or = null;
                DateTime minTime = this.GetDbMinTime();

                if (ors != null && ors.Datas.Count > 0)
                {
                    return new LongResponse(ors.Datas[0].Id);
                }

                var o = ServiceContainer.GetService<OrderService>().GetById(orderId.ToString()).First;

                if (o == null)
                {
                    throw new Exception("订单不存在");
                }

                if ((int)o.State < (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单未发货不能创建退货");
                }

                var og = o.OrderGoodss.FirstOrDefault(obj => obj.Id == orderGoodsId);

                if (og == null)
                {
                    throw new Exception("订单商品不存在");
                }

                if ((int)og.State < (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单未发货不能创建退货");
                }

                or = new OrderReturn
                {
                    Comment = "",
                    Count = count,
                    CreateOperator = op,
                    CreateTime = DateTime.Now,
                    DeliveryCompany = deliveryCompany,
                    DeliveryNumber = deliveryNumber,
                    State = OrderReturnState.WAITPROCESS,
                    GoodsInfo = og.Vendor + "," + og.Number + " " + og.Edtion + " " + og.Color + " " + og.Size,
                    OrderGoodsId = orderGoodsId,
                    OrderId = orderId,
                    ProcessOperator = "",
                    ProcessTime = minTime,
                    Reason = reason,
                    Type = type,
                    GoodsMoney = og.Price * og.Count,
                    Id = 0,
                    NewOrderId = 0,
                };
                this.dao.Save(or);
                return new LongResponse(or.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/createwithoutorder.html")]
        public LongResponse CreateWithoutOrder(string deliveryCompany, string deliveryNumber, string goodsInfo, float goodsMoney, int count)
        {
            try
            {
                string op = ServiceContainer.GetCurrentLoginInfo().op.Number;
                DateTime minTime = this.GetDbMinTime();
                var or = new OrderReturn
                {
                    Comment = "",
                    Count = count,
                    CreateOperator = op,
                    CreateTime = DateTime.Now,
                    DeliveryCompany = deliveryCompany,
                    DeliveryNumber = deliveryNumber,
                    State = OrderReturnState.WAITPROCESS,
                    GoodsInfo = goodsInfo,
                    OrderGoodsId = 0,
                    OrderId = 0,
                    ProcessOperator = "",
                    ProcessTime = minTime,
                    Reason = OrderReturnReason.DAY7,
                    Type = OrderReturnType.NONEORDER,
                    GoodsMoney = goodsMoney,
                };
                this.dao.Save(or);
                return new LongResponse(or.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }
    }
}
