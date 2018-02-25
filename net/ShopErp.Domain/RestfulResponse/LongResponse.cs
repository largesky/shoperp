using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class LongResponse : DataOneResponse<long>
    {

        public LongResponse() { }

        public LongResponse(long data) : base(data) { }

    }
}
