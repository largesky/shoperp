using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    class TaobaoKeywordService : ServiceBase<TaobaoKeyword, TaobaoKeywordDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<TaobaoKeyword> GetById(long id)
        {
            try
            {
                var item = this.dao.GetById(id);
                return new DataCollectionResponse<TaobaoKeyword>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(TaobaoKeyword value)
        {
            try
            {
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
        public ResponseBase Update(TaobaoKeyword value)
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
                this.dao.DeleteByLongId(id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<TaobaoKeyword> GetByAll()
        {
            try
            {
                var item = this.dao.GetAll().Datas.OrderBy(obj => obj.Number).ToList();
                return new DataCollectionResponse<TaobaoKeyword>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/updatestartandendtime.html")]
        public ResponseBase UpdateStartAndEndTime()
        {
            try
            {
                string sql1 = "update " + this.dao.GetEntiyName() + " set Start='" + Utils.DateTimeUtil.FormatDateTime(Utils.DateTimeUtil.DbMinTime)+"',End='" + Utils.DateTimeUtil.FormatDateTime(Utils.DateTimeUtil.DbMinTime) + "'";
                string sql2 = "update " + this.dao.GetEntiyName() + " set Start=(select Min(CreateTime) from TaobaoKeywordDetail where " + this.dao.GetEntiyName() + ".Number=Number) where (select Count(Id) from TaobaoKeywordDetail where TaobaoKeyword.Number=Number)>0";
                string sql3 = "update " + this.dao.GetEntiyName() + " set End=(select Max(CreateTime) from TaobaoKeywordDetail where " + this.dao.GetEntiyName() + ".Number=Number) where (select Count(Id) from TaobaoKeywordDetail where TaobaoKeyword.Number=Number)>0";
                this.dao.ExcuteSqlUpdate(sql1);
                this.dao.ExcuteSqlUpdate(sql2);
                this.dao.ExcuteSqlUpdate(sql3);

                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
