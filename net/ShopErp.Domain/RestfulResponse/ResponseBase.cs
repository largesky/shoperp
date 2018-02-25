using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.Domain.RestfulResponse
{
    public class ResponseBase
    {
        public static readonly ResponseBase SUCCESS = new ResponseBase();

        public string error;

        public ResponseBase()
        {
            this.error = "success";
        }

        public ResponseBase(string error)
        {
            this.error = error;
        }

        public ResponseBase(Exception ex)
        {
            var e = ex;
            while (e.InnerException != null)
            {
                e = e.InnerException;
            }
            this.error = e.Message;
        }
    }
}
