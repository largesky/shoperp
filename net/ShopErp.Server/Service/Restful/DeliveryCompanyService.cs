using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class DeliveryCompanyService : ServiceBase<DeliveryCompany, DeliveryCompanyDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<DeliveryCompany> GetById(long id)
        {
            try
            {
                var item = this.dao.GetById(id);
                return new DataCollectionResponse<DeliveryCompany>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(DeliveryCompany value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Name))
                {
                    throw new Exception("快递公司不能为空");
                }
                if (this.GetFirstOrDefaultInCach(new Predicate<DeliveryCompany>(o => o.Name == value.Name)) != null)
                {
                    throw new Exception("快递公司已经存在");
                }
                value.CreateTime = DateTime.Now;
                value.PopMapChuchujie = value.PopMapChuchujie ?? "";
                value.PopMapJd = value.PopMapJd ?? "";
                value.PopMapKuaidi100 = value.PopMapKuaidi100 ?? "";
                value.PopMapMeiliShuo = value.PopMapMeiliShuo ?? "";
                value.PopMapMogujie = value.PopMapMogujie ?? "";
                value.PopMapPingduoduo = value.PopMapPingduoduo ?? "";
                value.PopMapTaobao = value.PopMapTaobao ?? "";
                value.UpdateOperator = value.UpdateOperator ?? "";
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
        public ResponseBase Update(DeliveryCompany value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value.Name))
                {
                    throw new Exception("快递公司不能为空");
                }
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }
                if (this.GetFirstOrDefaultInCach(new Predicate<DeliveryCompany>(o => o.Id != value.Id && o.Name == value.Name)) != null)
                {
                    throw new Exception("已存在相同的快递公司名称");
                }

                value.PopMapChuchujie = value.PopMapChuchujie ?? "";
                value.PopMapJd = value.PopMapJd ?? "";
                value.PopMapKuaidi100 = value.PopMapKuaidi100 ?? "";
                value.PopMapMeiliShuo = value.PopMapMeiliShuo ?? "";
                value.PopMapMogujie = value.PopMapMogujie ?? "";
                value.PopMapPingduoduo = value.PopMapPingduoduo ?? "";
                value.PopMapTaobao = value.PopMapTaobao ?? "";
                value.UpdateOperator = value.UpdateOperator ?? "";
                value.UpdateTime = DateTime.Now;
                this.dao.Update(value);
                this.RemoveCach(new Predicate<DeliveryCompany>(o => o.Id == value.Id));
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
                this.dao.ExcuteSqlUpdate("delete from DeliveryCompany where Id=" + id);
                this.RemoveCach(obj => obj.Id == id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<DeliveryCompany> GetByAll()
        {
            try
            {
                return new DataCollectionResponse<DeliveryCompany>(this.GetAllInCach());
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getdeliverycompany.html")]
        public DataCollectionResponse<DeliveryCompany> GetDeliveryCompany(string name)
        {
            try
            {
                var dc = this.GetFirstOrDefaultInCach(obj => obj.Name == name);
                if (dc == null)
                {
                    throw new Exception(string.Format("未找到匹配的快递公司:{0}", name));
                }
                return new DataCollectionResponse<DeliveryCompany>(dc);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }

        }

    }
}
