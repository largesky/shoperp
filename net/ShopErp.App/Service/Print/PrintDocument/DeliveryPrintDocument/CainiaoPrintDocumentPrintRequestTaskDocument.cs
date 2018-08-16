using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public class CainiaoPrintDocumentPrintRequestTaskDocument
    {
        public string documentID = Guid.NewGuid().ToString();

        public string[] contents;

    }
}
