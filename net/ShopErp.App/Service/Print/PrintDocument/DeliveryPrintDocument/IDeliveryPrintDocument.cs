using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    interface IDeliveryPrintDocument : IPrintDocument
    {
        Order[] Orders { get; set; }

        void GenPages(Order[] orders, WuliuNumber[] wuliuNumbers, PrintTemplate template);
    }
}
