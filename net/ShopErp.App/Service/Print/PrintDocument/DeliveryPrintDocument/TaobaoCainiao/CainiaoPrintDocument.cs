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

        public CainiaoPrintDocument(Order[] orders, WuliuNumber[] wuliuNumbers, Dictionary<string, string>[] userDatas, PrintTemplate wuliuTemplate) : base(orders, wuliuNumbers, userDatas, wuliuTemplate)
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
                if (string.IsNullOrWhiteSpace(WuliuTemplate.UserOrIsvTemplateAreaUrl) == false)
                {
                    req.task.documents[i].contents[1] = new CainiaoPrintDocumentRequestPrintTaskDocumentSelfCotent
                    {
                        templateURL = this.WuliuTemplate.UserOrIsvTemplateAreaUrl,
                        data = UserDatas[i],
                    };
                }
            }
            return req;
        }

        private T SendAndReciveObject<T>(CainiaoPrintDocumentRequest request, string printServerAdd) where T : CainiaoPrintDocumentResponse
        {
            ClientWebSocket ws = new ClientWebSocket();
            CancellationToken ct = new CancellationToken();
            try
            {
                //创建连接
                if (ws.State != WebSocketState.Open)
                {
                    var connectTask = ws.ConnectAsync(new Uri(printServerAdd), ct);
                    if (connectTask.Wait(20 * 1000) == false)
                    {
                        throw new Exception("连接打印组件超时，等待时间(秒)：" + 20);
                    }
                }

                //发送数据
                var textData = Newtonsoft.Json.JsonConvert.SerializeObject(request);
                var bData = Encoding.UTF8.GetBytes(textData);
                var task = ws.SendAsync(new ArraySegment<byte>(bData), WebSocketMessageType.Text, true, ct);
                if (task.Wait(20 * 1000) == false)
                {
                    throw new Exception("向打印组件发送数据超时，等待时间(秒)：" + 20);
                }

                //接收数据
                var buf = new byte[1024];
                var readTask = ws.ReceiveAsync(new ArraySegment<byte>(buf), ct);
                if (readTask.Wait(20 * 1000) == false)
                {
                    throw new Exception("向打印组件读取数据超时，等待时间(秒)：" + 20);
                }
                string ret = Encoding.UTF8.GetString(buf, 0, readTask.Result.Count);
                Debug.WriteLine(DateTime.Now + ": " + ret);
                var response = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(ret);

                if (request.requestID != response.requestID)
                {
                    throw new Exception("发送的请求：" + request.requestID + " 与返回的请求不匹配：" + response.requestID);
                }
                return response;
            }
            catch (AggregateException ae)
            {
                throw new Exception("连接菜鸟打印组件错误，请菜鸟打印是否开启", ae.InnerException);
            }
            finally
            {
                //关闭连接
                if (ws != null && ws.State != WebSocketState.Closed)
                {
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "", new System.Threading.CancellationToken());
                }
            }
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
