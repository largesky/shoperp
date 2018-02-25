using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.Server.Dao.NHibernateDao;
using ShopErp.Server.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.Server.Service.Restful
{
    [ServiceContract]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, AddressFilterMode = AddressFilterMode.Exact)]
    public class OperatorService : ServiceBase<Operator, OperatorDao>
    {
        public static readonly List<LoginResponse> operators = new List<LoginResponse>();

        static void RemoveOffline(List<LoginResponse> infos)
        {
            var items = infos.Where(obj => DateTime.Now.Subtract(obj.lastOperateTime).TotalHours > 12).ToArray();
            foreach (var v in items)
            {
                infos.Remove(v);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyid.html")]
        public DataCollectionResponse<Operator> GetById(long id)
        {
            try
            {
                var item = this.dao.GetById(id);
                return new DataCollectionResponse<Operator>(item);
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/save.html")]
        public LongResponse Save(Operator value)
        {
            try
            {
                if (ServiceContainer.GetCurrentLoginInfo().op.Rights.Contains("用户管理") == false)
                {
                    throw new Exception("当前用户没有 用户管理 权限");
                }
                value.CreateTime = DateTime.Now;
                value.UpdateTime = DateTime.Now;
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
        public ResponseBase Update(Operator value)
        {
            try
            {
                if (ServiceContainer.GetCurrentLoginInfo().op.Rights.Contains("用户管理") == false)
                {
                    throw new Exception("当前用户没有 用户管理 权限");
                }
                if (value.Id < 1)
                {
                    throw new Exception("数据未保存过，不能直接更新");
                }
                string sql = string.Format("update `operator` set Number='{0}', Name='{1}',Phone='{2}',Rights='{3}',Enabled={4},UpdateTime=NOW() where Id={5}", value.Number, value.Name, value.Phone, value.Rights, value.Enabled ? 1 : 0, value.Id);
                this.dao.ExcuteSqlUpdate(sql);
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
                if (ServiceContainer.GetCurrentLoginInfo().op.Rights.Contains("用户管理") == false)
                {
                    throw new Exception("当前用户没有 用户管理 权限");
                }
                this.dao.ExcuteSqlUpdate("delete from `Operator` where Id=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/getbyall.html")]
        public DataCollectionResponse<Operator> GetByAll()
        {
            try
            {
                return this.dao.GetAll();
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/login.html")]
        public LoginResponse Login(string number, string password)
        {
            try
            {
                var login = new LoginResponse { lastOperateTime = DateTime.Now, loginTime = DateTime.Now };
                lock (operators)
                {
                    RemoveOffline(operators);
                    var op = this.dao.GetAllByField("number", number, 0, 0);
                    if (op.Total < 1)
                    {
                        login.error = "用户不存在";
                    }
                    else if (op.Total > 1)
                    {
                        login.error = "系统错误：出现多个相同账户";
                    }
                    else
                    {
                        if (password.ToUpper() != op.Datas[0].Password.ToUpper())
                        {
                            login.error = "用户密码错误";
                        }
                        else if (op.Datas[0].Enabled == false)
                        {
                            login.error = "当前用户已经禁用";
                        }
                        else
                        {
                            string session = Guid.NewGuid().ToString();
                            login.lastOperateTime = DateTime.Now;
                            login.loginTime = DateTime.Now;
                            login.op = op.Datas[0];
                            login.session = session;
                            operators.Add(login);
                        }
                    }
                }
                return login;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/logout.html")]
        public ResponseBase Logout(string session)
        {
            lock (operators)
            {
                RemoveOffline(operators);
                var op = operators.FirstOrDefault(obj => obj.session == session);
                if (op != null)
                {
                    operators.Remove(op);
                }
            }
            return ResponseBase.SUCCESS;
        }


        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest, UriTemplate = "/modifypassword.html")]
        public ResponseBase ModifyPassword(long id, string password)
        {
            try
            {
                if (ServiceContainer.GetCurrentLoginInfo().op.Rights.Contains("用户管理") == false)
                {
                    throw new Exception("当前用户没有 用户管理 权限");
                }
                this.dao.ExcuteSqlUpdate("update `operator` set `password`='" + password + "' where Id=" + id);
                return ResponseBase.SUCCESS;
            }
            catch (Exception ex)
            {
                throw new WebFaultException<ResponseBase>(new ResponseBase(ex.Message), System.Net.HttpStatusCode.OK);
            }
        }
    }
}
