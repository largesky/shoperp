using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public class CainiaoPrintDocumentPrintRequest
    {
        public string version = "1.0";

        public string cmd = "print";

        public string requestID = Guid.NewGuid().ToString();




    }
}
