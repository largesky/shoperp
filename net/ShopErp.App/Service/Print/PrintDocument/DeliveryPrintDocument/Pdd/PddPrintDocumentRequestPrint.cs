using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.Pdd
{
    public class PddPrintDocumentRequestPrint : PddPrintDocumentRequest
    {
        public PddPrintDocumentRequestPrintTask task;

        public PddPrintDocumentRequestPrint()
        {
            this.cmd = "print";
        }
    }
}
