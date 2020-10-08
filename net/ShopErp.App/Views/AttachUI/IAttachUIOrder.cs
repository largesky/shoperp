using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Views.AttachUI
{
    public interface IAttachUIOrder
    {
        event EventHandler Start;

        event EventHandler<AttachUIOrderPreviewDownloadEventArgs> OrderPreviewDownload;

        event EventHandler<AttachUiOrderDownloadEventArgs> OrderDownload;

        event EventHandler End;

        void DownloadOrders();

        void MarkPopDelivery(string popOrderId, string deliveryCompany, string deliveryNumber);

        string GetSellerComment(string popOrderId);
    }
}
