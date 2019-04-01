using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class SystemConfigService : ServiceBase<SystemConfig, SystemConfigDao>
    {
        private SystemConfig Find(long ownerId, string name)
        {
            return this.GetFirstOrDefaultInCach(new Predicate<SystemConfig>(sc => sc.OwnerId == ownerId && sc.Name == name));
        }

        public string GetEx(long ownerId, string name, string defaultValue)
        {
            var sc = Find(ownerId, name);
            if (sc != null)
            {
                return sc.Value;
            }
            return defaultValue;
        }

        public long SaveOrUpdateEx(long ownerId, string name, string value)
        {
            var sc = Find(ownerId, name);
            if (sc != null)
            {
                sc.Value = value;
                this.dao.Update(sc);
                return sc.Id;
            }
            else
            {
                sc = new SystemConfig { Id = 0, Name = name, Value = value, OwnerId = ownerId, UpdateTime = DateTime.Now, CreateTime = DateTime.Now, UpdateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number };
                this.dao.Save(sc);
                this.AndOrReplaceInCach(sc, obj => obj.Id == sc.Id);
                return sc.Id;
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/get.html")]
        public StringResponse Get(long ownerId, string name, string defaultValue)
        {
            try
            {
                //卖家只能读取自身的配置项
                ownerId = ServiceContainer.GetSellerId(ownerId);
                var sc = Find(ownerId, name);
                if (sc != null)
                {
                    return new StringResponse { data = sc.Value };
                }

                return new StringResponse { data = defaultValue };
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/saveorupdate.html")]
        public LongResponse SaveOrUpdate(long ownerId, string name, string value)
        {
            try
            {
                //卖家只能配置自身的配置项
                ownerId = ServiceContainer.GetSellerId(ownerId);
                return new LongResponse(this.SaveOrUpdateEx(ownerId, name, value));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

    }
}
