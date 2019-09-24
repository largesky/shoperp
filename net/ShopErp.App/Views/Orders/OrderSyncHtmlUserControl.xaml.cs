using ShopErp.App.Domain.TaobaoHtml.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CefSharp;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.Pop;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class OrderSyncHtmlUserControl : UserControl
    {
        private bool isRunning = false;
        private bool myLoaded = false;
        string jspath = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA + "\\TAOBAOJS.js");

        public OrderSyncHtmlUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myLoaded)
                {
                    return;
                }
                this.dpStart.Value = DateTime.Now.AddDays(-10);
                this.dpEnd.Value = DateTime.Now;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            string popOrderId = this.tbPopOrderId.Text.Trim();
            var dt = this.dpStart.Value.Value;
            var end = this.dpEnd.Value.Value;
            Task.Factory.StartNew(new Action(() => Start(popOrderId, dt, end)));
        }

        private void Start(string popOrderId, DateTime startTime, DateTime endTime)
        {
            try
            {
                if (this.isRunning)
                {
                    this.isRunning = false;
                    return;
                }
                this.isRunning = true;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "停止"));
                string htmlRet = this.wb1.GetTextAsync().Result;
                var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                var shop = allShops.FirstOrDefault(obj => htmlRet.Contains(obj.PopSellerId));
                var ors = ServiceContainer.GetService<OrderUpdateService>().GetByAll(new long[] { shop.Id }, popOrderId, startTime, endTime, 0, 0);
                var orders = ors.Datas.Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false).ToArray();
                if (orders.Length < 1)
                {
                    throw new Exception("订单不存在");
                }
                int i = 0;
                foreach (var o in orders)
                {
                    if (this.isRunning == false)
                    {
                        break;
                    }
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (this.tbMessage.LineCount > 10000)
                        {
                            this.tbMessage.Text = "";
                        }
                        this.tbMessage.AppendText(DateTime.Now + ":正在下载订单:" + (++i) + "/" + orders.Count() + "  " + o.PopOrderId + Environment.NewLine);
                        this.tbMessage.ScrollToEnd();
                    }));
                    var pos = this.ParseOrder(o.PopOrderId);
                    string ret = ServiceContainer.GetService<OrderService>().UpdateOrderState(pos, o, shop).data;
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (this.tbMessage.LineCount > 10000)
                        {
                            this.tbMessage.Text = "";
                        }
                        this.tbMessage.AppendText(DateTime.Now + ":操作结果:" + o.PopOrderId + " " + ret +
                                                  Environment.NewLine);
                        this.tbMessage.ScrollToEnd();
                    }));
                    WPFHelper.DoEvents();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.isRunning = false;
                this.Dispatcher.BeginInvoke(new Action(() => this.btnUpdate.Content = "开始同步"));
            }
        }

        private ColorFlag ConvertFlag(int flag)
        {
            if (flag == 0)
            {
                return ColorFlag.UN_LABEL;
            }
            if (flag == 1)
            {
                return ColorFlag.RED;
            }
            if (flag == 2)
            {
                return ColorFlag.YELLOW;
            }
            if (flag == 3)
            {
                return ColorFlag.GREEN;
            }
            if (flag == 4)
            {
                return ColorFlag.BLUE;
            }
            if (flag == 5)
            {
                return ColorFlag.PINK;
            }
            return ColorFlag.UN_LABEL;
        }

        private OrderState ConveretState(string state)
        {
            if (state.Contains("商品已拍下，等待买家付款"))
            {
                return OrderState.WAITPAY;
            }
            if (state.Contains("买家已付款，等待商家发货"))
            {
                return OrderState.PAYED;
            }
            if (state.Contains("商家已发货，等待买家确认"))
            {
                return OrderState.SHIPPED;
            }
            if (state.Contains("订单部分退款中"))
            {
                return OrderState.RETURNING;
            }
            if (state.Contains("交易关闭"))
            {
                return OrderState.CANCLED;
            }
            if (state.Contains("交易成功"))
            {
                return OrderState.SUCCESS;
            }
            return OrderState.WAITPAY;
        }

        private PopOrderState ParseOrder(string popOrderId)
        {
            var pos = new PopOrderState()
            {
                PopOrderId = popOrderId,
                PopOrderStateDesc = "",
                PopOrderStateValue = "",
                State = OrderState.NONE
            };

            //订单信息
            var js = ScriptManager.GetBody(jspath, "//TAOBAO_GET_ORDER").Replace("###bizOrderId", popOrderId);
            var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(js, "", 1, new TimeSpan(0, 0, 30));
            var ret = task.Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }

            var content = ret.Result.ToString();
            int si = content.IndexOf("var detailData");
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据");
            }

            int ei = content.IndexOf("</script>", si);
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }
            string orderInfo = content.Substring(si + "var detailData".Length, ei - si - "var detailData".Length).Trim().TrimStart('=');

            var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<TmallQueryOrderDetailResponse>(orderInfo);

            pos.PopOrderStateValue = oi.overStatus.status.content[0].text;
            pos.PopOrderStateDesc = oi.overStatus.status.content[0].text;
            pos.State = ConveretState(pos.PopOrderStateValue);
            return pos;
        }

        private void btnGoToTaobao_Click(object sender, RoutedEventArgs e)
        {
            this.wb1.Load("https://trade.taobao.com/trade/itemlist/list_sold_items.htm");
        }
    }
}