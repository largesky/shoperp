using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument
{
    public class CainiaoPrintDocument : DeliveryPrintDocument
    {
        private static ClientWebSocket ws;

        private static object ws_lock = new object();

        protected override void BenginPrint()
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

        protected override void EndPrint()
        {
            base.EndPrint();
        }

        protected override object FormatData(PrintTemplateItem printTemplateItem)
        {
            throw new NotImplementedException();
        }

        protected override void PrintValue(PrintPageEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
