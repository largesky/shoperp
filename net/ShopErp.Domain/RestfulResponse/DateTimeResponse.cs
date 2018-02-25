using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class DateTimeResponse : DataOneResponse<DateTime>
    {
        public DateTimeResponse()
        {
        }

        public DateTimeResponse(DateTime dateTime) : base(dateTime)
        {
        }
    }
}
