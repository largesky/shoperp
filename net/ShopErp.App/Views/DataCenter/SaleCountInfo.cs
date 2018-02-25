using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShopErp.App.Views.DataCenter
{
    class SaleCountInfo
    {
        public string VendorName { get; set; }

        public string Number { get; set; }

        public int Count { get; set; }

        public float SaleMoney { get; set; }

        public float PerCount { get; set; }

        public float PerSaleMoney { get; set; }
    }
}