using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Service.Print.PrintDocument.DeliveryPrintDocument.TaobaoCainiao
{
    public class CainiaoPrintDocument : DeliveryPrintDocument
    {
        static Random r = new Random((int)DateTime.Now.Ticks);

        private AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        private string data = "";

        private string error = "";

        public CainiaoPrintDocument(Order[] orders, WuliuNumber[] wuliuNumbers, Dictionary<string, string>[] userDatas, WuliuPrintTemplate wuliuTemplate) : base(orders, wuliuNumbers, userDatas, wuliuTemplate)
        {
        }

        private CainiaoPrintDocumentRequestPrint GetPrintData(string printer)
        {
            //拼装数据
            var req = new CainiaoPrintDocumentRequestPrint();
            req.requestID = r.Next(100000).ToString();
            req.task = new CainiaoPrintDocumentRequestPrintTask();
            req.task.preview = printer.ToLower().Contains("xps") || printer.ToLower().Contains("pdf");
            req.task.previewType = "pdf";
            req.task.printer = printer;
            req.task.firstDocumentNumber = 1;
            req.task.totalDocumentCount = this.Orders.Length;
            req.task.documents = new CainiaoPrintDocumentRequestPrintTaskDocument[this.Orders.Length];
            req.task.taskID = r.Next(100000).ToString();
            for (int i = 0; i < this.Orders.Length; i++)
            {
                req.task.documents[i] = new CainiaoPrintDocumentRequestPrintTaskDocument();
                req.task.documents[i].documentID = this.WuliuNumbers[i].DeliveryNumber;
                req.task.documents[i].contents = new object[2];
                //模板中的标准数据
                req.task.documents[i].contents[0] = Newtonsoft.Json.JsonConvert.DeserializeObject<CainiaoPrintDocumentRequestPrintTaskDocumentCotent>(WuliuNumbers[i].PrintData);
                //标准模板不能增加数据，只有商家或者ISV模板才能增加数据
                if (string.IsNullOrWhiteSpace(CloudPrintTemplate.UserOrIsvTemplateAreaUrl) == false)
                {
                    req.task.documents[i].contents[1] = new CainiaoPrintDocumentRequestPrintTaskDocumentSelfCotent
                    {
                        templateURL = this.CloudPrintTemplate.UserOrIsvTemplateAreaUrl,
                        data = UserDatas[i],
                    };
                }
            }
            return req;
        }

        private T SendAndReciveObject<T>(CainiaoPrintDocumentRequest request, string printServerAdd) where T : CainiaoPrintDocumentResponse
        {
            WebSocketSharp.WebSocket webSocket = new WebSocketSharp.WebSocket(printServerAdd);

            try
            {
                webSocket.OnMessage += WebSocket_OnMessage;
                webSocket.OnOpen += WebSocket_OnOpen;
                webSocket.OnError += WebSocket_OnError;

                this.data = "";
                this.error = "";
                this.autoResetEvent.Reset();

                //连接
                webSocket.Connect();
                if (autoResetEvent.WaitOne(5 * 1000) == false)
                {
                    throw new Exception("等待回收数据超时:5秒");
                }
                if (string.IsNullOrWhiteSpace(this.error) == false || webSocket.ReadyState != WebSocketSharp.WsState.OPEN)
                {
                    throw new Exception("连接打印组件错误，请检查菜鸟或者拼多多打印组件是否开启:" + this.error);
                }

                //发送数据
                webSocket.Send(Newtonsoft.Json.JsonConvert.SerializeObject(request));
                if (autoResetEvent.WaitOne(60 * 1000) == false)
                {
                    throw new Exception("等待回收数据超时:60秒");
                }
                if (string.IsNullOrWhiteSpace(this.data))
                {
                    throw new Exception("打印组件发送数据失败，没有返回数据：" + this.error);
                }
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(this.data);
                if (request.requestID != response.requestID)
                {
                    throw new Exception("发送的请求：" + request.requestID + " 与返回的请求不匹配：" + response.requestID);
                }
                return response;
            }
            catch (AggregateException ae)
            {
                throw new Exception("连接打印组件错误，请打印是否开启", ae.InnerException);
            }
            finally
            {
                //关闭连接
                if (webSocket != null && webSocket.IsAlive)
                {
                    webSocket.Close();
                }
            }
        }

        private void WebSocket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            this.error = e.Message;
            this.autoResetEvent.Set();
        }

        private void WebSocket_OnOpen(object sender, EventArgs e)
        {
            this.autoResetEvent.Set();
        }

        private void WebSocket_OnMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            this.data = e.Data;
            this.autoResetEvent.Set();
        }

        public override string StartPrint(string printer, string printServerAdd)
        {
            var req = GetPrintData(printer);
            var rsp = SendAndReciveObject<CainiaoPrintDocumentResponsePrint>(req, printServerAdd);
            if (rsp.status.Equals("success", StringComparison.OrdinalIgnoreCase) == false)
            {
                throw new Exception("发送打印任务失败");
            }
            return rsp.previewURL;
        }
    }
}
