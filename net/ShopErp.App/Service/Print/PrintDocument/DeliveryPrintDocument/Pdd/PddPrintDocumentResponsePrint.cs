using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.Pdd
{
    public class PddPrintDocumentResponsePrint : PddPrintDocumentResponse
    {
        public string taskID;

        public string status;

        public string previewURL;

        public string[] previewImage;
    }
}
