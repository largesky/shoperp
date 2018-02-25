using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;
using System.Linq;
using ShopErp.Domain.RestfulResponse.DomainResponse;

namespace ShopErp.App.Service.Restful
{
    public class OperatorService : ServiceBase<Operator>
    {
        public static Operator LoginOperator = null;

        public DataCollectionResponse<Operator> GetByAll()
        {
            return DoPost<DataCollectionResponse<Operator>>(null);
        }

        public void Login(string number, string password)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["number"] = number;
            para["password"] = Utils.Md5Util.Md5(password);
            var ret = DoPost<LoginResponse>(para);
            OperatorService.LoginOperator = ret.op;
            ServiceContainer.AccessToken = ret.session;
        }

        public void Logout(string session)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["session"] = session;
            DoPost<ResponseBase>(para);
        }

        public void ModifyPassword(long id, string password)
        {
            Dictionary<string, object> para = new Dictionary<string, object>();
            para["id"] = id;
            para["password"] = password;
            DoPost<ResponseBase>(para);
        }
    }
}