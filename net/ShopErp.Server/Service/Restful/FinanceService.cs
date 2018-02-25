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
    public class FinanceService : ServiceBase<Finance, FinanceDao>
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<Finance> GetById(long id)
        {
            try
            {
                return new DataCollectionResponse<Finance>(this.dao.GetById(id));
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(Finance value)
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
        public ResponseBase Update(Finance value)
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
                this.dao.ExcuteSqlUpdate("delete from `finance` where id=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<Finance> GetByAll(string type, long accountId, string comment, DateTime startTime, DateTime endTime, int pageIndex, int pageSize)
        {
            try
            {
                var ret = this.dao.GetByAll(type, accountId, comment, startTime, endTime, pageIndex, pageSize);
                return ret;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/create.html")]
        public ResponseBase Create(string type, DateTime time, float money, long account, long account2, string comment, string opposite)
        {
            try
            {
                var ac = ServiceContainer.GetService<FinanceAccountService>().GetById(account).First;
                var ac2 = ServiceContainer.GetService<FinanceAccountService>().GetById(account2).First;

                if (string.IsNullOrWhiteSpace(type))
                {
                    throw new Exception("type 参数不能为空");
                }

                if (time.Subtract(new DateTime(2017, 01, 01)).TotalDays < 1)
                {
                    throw new Exception("时间不能小于2017年");
                }

                if (ac == null)
                {
                    throw new Exception("账户1不存在");
                }

                if (ac2 == null && account2 > 0)
                {
                    throw new Exception("账户2不存在");
                }

                if ((int)(money * 100) == 0)
                {
                    throw new Exception("金额不能为0");
                }

                if (ac2 != null)
                {
                    Finance f = new Finance
                    {
                        FinaceAccountId = account,
                        Type = type,
                        Money = money,
                        Comment = comment,
                        CreateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number,
                        CreateTime = time,
                        Opposite = opposite,
                    };
                    Finance f2 = new Finance
                    {
                        FinaceAccountId = account,
                        Type = type,
                        Money = money,
                        Comment = comment,
                        CreateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number,
                        CreateTime = time,
                        Opposite = opposite,
                    };
                    this.dao.Save(f, f2);
                }
                else
                {
                    Finance f = new Finance
                    {
                        FinaceAccountId = account,
                        Type = type,
                        Money = money,
                        Comment = comment,
                        CreateOperator = ServiceContainer.GetCurrentLoginInfo().op.Number,
                        CreateTime = time,
                        Opposite = opposite,
                    };
                    this.dao.Save(f);
                }
                return StringResponse.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

    }
}
