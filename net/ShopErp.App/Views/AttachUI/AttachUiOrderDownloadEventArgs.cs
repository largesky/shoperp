using ShopErp.Domain.Pop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.AttachUI
{
    public class AttachUiOrderDownloadEventArgs : EventArgs
    {
        public OrderDownload OrderDownload { get; set; }
    }
}
