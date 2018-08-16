using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public class CainiaoPrintDocument : IDeliveryPrintDocument
    {
        private static ClientWebSocket ws;

        private static object ws_lock = new object();

        private PrintTemplate template = null;

        public Order[] Orders { get; set; }
        public event Action<object, int> PageGening;
        public event Action<object, int> PageGened;
        public event Action<object, int> PagePrinting;
        public event Action<object, int> PagePrinted;

        private void OnPageGening(int page)
        {
            if (this.PageGening != null)
                this.PageGening(this, page);
        }

        private void OnPageGened(int page)
        {
            if (this.PageGened != null)
                this.PageGened(this, page);
        }

        private void OnPagePrinting(int page)
        {
            if (this.PagePrinting != null)
                this.PagePrinting(this, page);
        }

        private void OnPagePrinted(int page)
        {
            if (this.PagePrinted != null)
                this.PagePrinted(this, page);
        }


        static void InitWebSockt()
        {
            lock (ws_lock)
            {
                if (ws_lock == null)
                {
                    ws = new ClientWebSocket();
                }
                if (ws.State == WebSocketState.Open)
                {
                    return;
                }
                if (ws.State == WebSocketState.None)
                {
                    ws.ConnectAsync(new Uri("ws://localhost:13528"), new System.Threading.CancellationToken()).Wait();
                }
            }
        }


        public void GenPages(Order[] orders, WuliuNumber[] wuliuNumbers, PrintTemplate template)
        {
            if (orders.Any(obj => obj == null))
            {
                throw new Exception("参数错误，有订单为空");
            }

            if (wuliuNumbers.Any(obj => obj == null))
            {
                throw new Exception("参数错误，有物流信息为空");
            }

            if (template == null)
            {
                throw new Exception("参数错误，打印模板为空");
            }

            if (orders.Length != wuliuNumbers.Length)
            {
                throw new Exception("订单与物流信息长度不相等");
            }

            this.Orders = new Order[orders.Length];
            Array.Copy(orders, this.Orders, orders.Length);
            this.template = template;


            //生成数据
            foreach (var o in this.Orders)
            {
            }

        }

        public void Print(string printer)
        {

        }
    }
}
