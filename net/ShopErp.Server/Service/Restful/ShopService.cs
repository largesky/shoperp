using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Server.Service.Pop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class ShopService : ServiceBase<Shop, ShopDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<Shop> GetById(long id)
        {
            try
            {
                return new DataCollectionResponse<Shop>(this.GetFirstOrDefaultInCach(new Predicate<Shop>(s => s.Id == id)));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(Shop value)
        {
            try
            {
                value.CreateTime = DateTime.Now;
                value.UpdateTime = DateTime.Now;
                this.dao.Save(value);
                this.CheckAndLoadCach();
                if (this.GetFirstOrDefaultInCach(obj => obj.Id == value.Id) == null)
                    this.AndInCach(value);
                return new LongResponse(value.Id);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/update.html")]
        public ResponseBase Update(Shop value)
        {
            try
            {
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }
                value.UpdateTime = DateTime.Now;
                this.dao.Update(value);
                this.RemoveCach(o => o.Id == value.Id);
                this.AndInCach(value);
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
                this.dao.ExcuteSqlUpdate("delete from Shop where Id=" + id);
                this.RemoveCach(o => o.Id == id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<Shop> GetByAll()
        {
            try
            {
                var ret = this.GetAllInCach().OrderBy(obj => obj.PopType).ToArray();
                return new DataCollectionResponse<Shop>(ret);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getshopoauthurl.html")]
        public StringResponse GetShopOauthUrl(long shopId)
        {
            try
            {
                var shop = this.GetFirstOrDefaultInCach(obj => obj.Id == shopId);
                if (shop == null)
                {
                    throw new Exception("没有找到指定店铺");
                }
                var ps = new PopService();
                string url = "";
                if (shop.PopType == PopType.PINGDUODUO)
                {
                    url = ps.GetShopOauthUrl(shop);
                }
                else
                {
                    throw new Exception("自动授权接口当前不支持该平台");
                }

                return new StringResponse(url);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Xml, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/pddoauth.html?code={code}&state={state}")]
        public string Pddoauth(string code, string state)
        {
            try
            {
                //http://mms.pinduoduo.com/open.html?response_type=code&client_id=346e6bc3f0054b1daf2284df57ad5b0d&redirect_uri=http://bjcgroup.imwork.net:60014/shoperp/shop/pddoauth.html
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new Exception("拼多多授权返回数据没有code");
                }

                if (string.IsNullOrWhiteSpace(state))
                {
                    throw new Exception("拼多多授权返回数据没有state");
                }

                var shop = this.GetFirstOrDefaultInCach(obj => obj.Id == long.Parse(state));
                if (shop == null)
                {
                    throw new Exception("没有找到指定店铺");
                }
                var s = new PopService().GetAcessTokenInfo(shop, code);
                this.Update(s);
                return "拼多多授权成功，请关闭程序重新登录";
            }
            catch (Exception ex)
            {
                return "拼多多授权失败：" + ex.Message;
            }

        }
    }
}