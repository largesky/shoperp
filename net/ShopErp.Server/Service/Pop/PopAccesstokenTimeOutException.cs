using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Server.Service.Pop
{
    public class PopAccesstokenTimeOutException : Exception
    {
        public PopAccesstokenTimeOutException()
        { }

        public PopAccesstokenTimeOutException(string message) : base(message)
        { }

        public PopAccesstokenTimeOutException(string message, Exception innerException) : base(message, innerException)
        { }
    }
}
