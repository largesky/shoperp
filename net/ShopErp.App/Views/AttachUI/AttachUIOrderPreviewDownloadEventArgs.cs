using ShopErp.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.AttachUI
{
    public class AttachUIOrderPreviewDownloadEventArgs : EventArgs
    {
        public string PopOrderId { get; set; }

        public OrderState State { get; set; }

        public ColorFlag PopFlag { get; set; }

        public Shop Shop { get; set; }

        public int Current { get; set; }

        public int Total { get; set; }

        public bool Skip { get; set; }

        public bool Stop { get; set; }
    }
}
