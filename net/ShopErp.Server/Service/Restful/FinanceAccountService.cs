using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain;
using ShopErp.Server.Dao.NHibernateDao;
using System.ServiceModel.Web;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class FinanceAccountService : ServiceBase<FinanceAccount, FinanceAccountDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<FinanceAccount> GetById(long id)
        {
            try
            {
                var ret = this.dao.GetById(id);
                return new DataCollectionResponse<FinanceAccount>(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<FinanceAccount> GetByAll()
        {
            try
            {
                var ret = this.dao.GetAll().Datas.OrderBy(obj => obj.Order).ToArray();
                if (ServiceContainer.GetCurrentLoginInfo().op.Rights.Contains("显示金额") == false)
                {
                    foreach (var r in ret)
                    {
                        r.Money = 0;
                    }
                }
                return new DataCollectionResponse<FinanceAccount>(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
