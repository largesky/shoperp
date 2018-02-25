using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class FloatResponse : DataOneResponse<float>
    {

        public FloatResponse() { }

        public FloatResponse(float data) : base(data) { }

    }
}
