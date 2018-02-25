using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Web;
using ShopErp.Domain.Pop;
using ShopErp.Server.Service.Pop;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OrderUpdateService : ServiceBase<OrderUpdate, OrderUpdateDao>
    {
        private readonly PopService popService = new PopService();

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<OrderUpdate> GetByAll(long[] shopIds, string popOrderId, DateTime popPayTimeStart, DateTime popPayTimeEnd, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(shopIds, popOrderId, popPayTimeStart,popPayTimeEnd, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public StringResponse Update(OrderUpdate orderUpdate)
        {
            try
            {
                var shop = ServiceContainer.GetService<ShopService>().GetById(orderUpdate.ShopId).First;
                var functionType = popService.GetOrderGetFunction(shop.PopType);
                string error = "";
                if (functionType == PopOrderGetFunction.PAYED)
                {
                    var nor = popService.GetOrderState(shop, orderUpdate.PopOrderId);
                    error = ServiceContainer.GetService<OrderService>().UpdateOrderState(nor, orderUpdate, shop).data;
                }
                else
                {
                    var nor = popService.GetOrder(shop, orderUpdate.PopOrderId);
                    error = ServiceContainer.GetService<OrderService>().UpdateOrderStateWithGoods(nor.Order, orderUpdate, shop).data;
                }
                return new StringResponse(error);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }

        public void UpdateOrderGoodsState(long orderId, string popInfo, int count, OrderState state)
        {
            this.dao.UpdateOrderGoodsState(orderId, popInfo, count, state);
        }

        public void UpdateOrderGoodsStateByOrderId(long orderId, OrderState state)
        {
            this.dao.UpdateOrderGoodsStateByOrderId(orderId, state);
        }

        public void UpdateEx(OrderUpdate ou, bool updateState)
        {
            this.dao.UpdateEx(ou, updateState);
        }
    }
}
