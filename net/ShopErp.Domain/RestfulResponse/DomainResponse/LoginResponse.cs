using System;

namespace ShopErp.Domain.RestfulResponse.DomainResponse
{
    public class LoginResponse : ResponseBase
    {
        public string session;

        public DateTime loginTime;

        public DateTime lastOperateTime;

        public Operator op;
    }
}
