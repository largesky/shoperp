using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class DataOneResponse<T> : ResponseBase
    {
        public T data;

        public DataOneResponse() { }

        public DataOneResponse(T data) { this.data = data; }
    }
}
