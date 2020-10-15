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
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<OrderUpdate> GetByAll(long[] shopIds, string popOrderId, OrderType orderType, DateTime popPayTimeStart, DateTime popPayTimeEnd, int pageIndex, int pageSize)
        {
            try
            {
                return this.dao.GetByAll(shopIds, popOrderId, orderType, popPayTimeStart, popPayTimeEnd, pageIndex, pageSize);
            }
            catch (Exception e)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(e.Message), HttpStatusCode.OK);
            }
        }
    }
}
