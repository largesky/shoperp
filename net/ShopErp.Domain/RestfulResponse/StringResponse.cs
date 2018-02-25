using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class StringResponse : DataOneResponse<string>
    {
        public StringResponse()
        { }

        public StringResponse(string data) : base(data) { }

    }
}
