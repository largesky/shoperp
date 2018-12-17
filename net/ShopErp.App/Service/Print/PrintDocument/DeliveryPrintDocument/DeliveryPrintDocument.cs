using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public abstract class DeliveryPrintDocument : FixedPrintDocument<ShopErp.Domain.Order>
    {
        public WuliuNumber[] WuliuNumbers { get; set; }
    }
}
