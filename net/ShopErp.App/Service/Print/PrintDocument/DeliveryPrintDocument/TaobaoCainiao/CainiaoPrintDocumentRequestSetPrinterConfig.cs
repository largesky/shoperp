using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.TaobaoCainiao
{
    class CainiaoPrintDocumentRequestSetPrinterConfig : CainiaoPrintDocumentRequest
    {
        public string status;
        public CainiaoPrintDocumentRequestSetPrinterConfigPrinter printer;

        public CainiaoPrintDocumentRequestSetPrinterConfig()
        {
            this.cmd = "setPrinterConfig";
        }
    }
}
