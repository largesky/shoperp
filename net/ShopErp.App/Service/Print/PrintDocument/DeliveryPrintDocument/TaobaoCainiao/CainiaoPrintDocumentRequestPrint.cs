using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.TaobaoCainiao
{
    public class CainiaoPrintDocumentRequestPrint : CainiaoPrintDocumentRequest
    {
        public CainiaoPrintDocumentRequestPrintTask task;

        public CainiaoPrintDocumentRequestPrint()
        {
            this.cmd = "print";
        }
    }
}
