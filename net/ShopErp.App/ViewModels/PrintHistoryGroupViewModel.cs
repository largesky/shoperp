using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShopErp.Domain;

namespace ShopErp.App.ViewModels
{
    class PrintHistoryGroupViewModel
    {
        public string DeliveryTitle { get; set; }

        public List<PrintHistoryViewModel> OrderViewModels { get; set; }

        public PrintHistoryGroupViewModel(PrintHistory[] orders)
        {
            this.DeliveryTitle = orders[0].DeliveryTemplate;
            this.OrderViewModels = new List<PrintHistoryViewModel>();
            for (int i = 0; i < orders.Length; i++)
            {
                this.OrderViewModels.Add(new PrintHistoryViewModel(orders[i],
                    (i % 2 == 0)
                        ? PrintHistoryViewModel.DEFAULTBACKGROUND_LIGHTGREEN
                        : PrintHistoryViewModel.DEFAULTBACKGROUND_LIGHTPINK));
            }
        }
    }
}