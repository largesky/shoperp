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
using ShopErp.App.ViewModels;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class ShippingCheckHtmlUserControl : UserControl
    {
        private bool myLoaded = false;
        string jspath = System.IO.Path.Combine(EnvironmentDirHelper.DIR_DATA + "\\TAOBAOJS.js");
        private bool isRunning = false;
        private ObservableCollection<OrderViewModel> orders = new ObservableCollection<OrderViewModel>();

        public ShippingCheckHtmlUserControl()
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
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private OrderState ConvertState(string state)
        {
            if (state == "等待买家付款" || state.Contains("商品已拍下，等待买家付款"))
            {
                return OrderState.WAITPAY;
            }
            if (state == "买家已付款" || state.Contains("买家已付款，等待商家发货"))
            {
                return OrderState.PAYED;
            }
            if (state == "卖家已发货" || state.Contains("商家已发货，等待买家确认"))
            {
                return OrderState.SHIPPED;
            }
            if (state.Contains("订单部分退款中") || state.Contains("待退货") || state.Contains("请退款"))
            {
                return OrderState.RETURNING;
            }
            if (state.Contains("交易关闭") || state.Contains("退款成功"))
            {
                return OrderState.CANCLED;
            }
            if (state.Contains("交易成功"))
            {
                return OrderState.SUCCESS;
            }

            return OrderState.WAITPAY;
        }

        private ShopErp.Domain.Order ParseOrder(TaobaoQueryOrderListResponseOrder orderShort, Shop shop)
        {
            var dbMineTime = ServiceContainer.GetService<OrderService>().GetDBMinTime();


            //订单信息
            var js = ScriptManager.GetBody(jspath, "//TAOBAO_GET_ORDER").Replace("###bizOrderId", orderShort.id);
            var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(js, "", 1, new TimeSpan(0, 0, 30));
            var ret = task.Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }

            var content = ret.Result.ToString();
            string title = shop.PopType == PopType.TMALL ? "var detailData" : "var data = JSON";

            int si = content.IndexOf(title);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title);
            }
            si = content.IndexOf('{', si);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title);
            }
            int ei = content.IndexOf("</script>", si);
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }
            while (ei >= 0 && content[ei] != '}') ei--;
            if (ei <= si)
            {
                throw new Exception("未找到详情结尾数据");
            }

            String orderInfo = content.Substring(si, ei - si + 1).Trim();
            DateTime popPayTime = dbMineTime, popDeliveryTime = dbMineTime;
            string buyerComment = "", sellerComment = "", reciverInfo = "", popOrderState = "";
            float goodsPrice = 0, deliveryPrice = 0, sellerGetMoney = 0;
            Dictionary<string, float> namePrice = new Dictionary<string, float>();

            if (shop.PopType == PopType.TMALL)
            {
                var orderDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TmallQueryOrderDetailResponse>(orderInfo, new Newtonsoft.Json.JsonSerializerSettings { StringEscapeHandling = Newtonsoft.Json.StringEscapeHandling.EscapeHtml });

                string payTime = orderDetail.stepbar.options.First(obj => obj.content == "买家付款").time;
                string deliveryTime = orderDetail.stepbar.options.First(obj => obj.content == "发货").time;

                popOrderState = orderDetail.overStatus.status.content[0].text;
                popPayTime = string.IsNullOrWhiteSpace(payTime) ? dbMineTime : DateTime.Parse(payTime);
                popDeliveryTime = string.IsNullOrWhiteSpace(deliveryTime) ? dbMineTime : DateTime.Parse(deliveryTime);
                buyerComment = orderDetail.basic.lists.First(obj => obj.key == "买家留言").content[0].text;
                if (buyerComment == "-")
                {
                    buyerComment = "";
                }
                var addN = orderDetail.basic.lists.First(obj => obj.key == "收货地址").content[0];
                //html 表示地址是要经过转运的地址，label是不需要经过转运的大陆地址
                if (addN.type.Equals("html", StringComparison.OrdinalIgnoreCase))
                {
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    document.LoadHtml(addN.text);
                    string hh = document.DocumentNode.InnerText;
                    string nhh = hh.Substring(0, hh.IndexOf("]转&nbsp;") + 1);// 
                    string read = nhh.Trim().TrimEnd(']');
                    string mark = "[转运仓转运仓库：";
                    if (read.IndexOf(mark) > 0)
                    {
                        read = read.Replace(mark, "");
                    }
                    reciverInfo = read;
                }
                else if (addN.type.Equals("label", StringComparison.OrdinalIgnoreCase))
                {
                    reciverInfo = addN.text;
                }
                else
                {
                    throw new Exception("无法识别的地址格式");
                }

                //订单金额
                var contents = new List<TmallQueryOrderDetailResponseAmountCountContent>();
                foreach (var c in orderDetail.amount.count)
                {
                    foreach (var cc in c)
                    {
                        contents.AddRange(cc.content);
                    }
                }

                string strGoodsPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text == "商品总价").data.money.text.Replace("￥", "").Trim();
                string strDeliveryPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("运费")).data.money.text.Replace("￥", "").Trim();
                string strBuyerPayPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("订单总价")).data.money.text.Replace("￥", "").Trim();
                var strSellerGetMoney = contents.FirstOrDefault(obj => obj.data.titleLink != null && (obj.data.titleLink.text.Contains("应收款") || obj.data.titleLink.text.Contains("实收款")));

                goodsPrice = float.Parse(strGoodsPrice);
                deliveryPrice = float.Parse(strDeliveryPrice);
                sellerGetMoney = float.Parse(strSellerGetMoney.data.dotPrefixMoney.text + strSellerGetMoney.data.dotSufixMoney.text);

                //商家备注
                if (orderDetail.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)) != null)
                {
                    string comment = orderDetail.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)).content[0].text;
                    si = comment.IndexOf("备忘：</span><span>");
                    ei = comment.IndexOf("</span>", si + "备忘：</span><span>".Length);
                    sellerComment = comment.Substring(si + "备忘：</span><span>".Length, ei - si - "备忘：</span><span>".Length);
                }

                foreach (var vv in orderDetail.orders.list)
                {
                    foreach (var vvv in vv.status)
                    {
                        foreach (var vvvv in vvv.subOrders)
                        {
                            if (namePrice.ContainsKey(vvvv.itemInfo.title) == false)
                            {
                                namePrice.Add(vvvv.itemInfo.title, float.Parse(vvvv.priceInfo[0].text.Trim()));
                            }
                        }
                    }
                }
            }
            else
            {
                orderInfo = Regex.Unescape(orderInfo);
                var orderDetail = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderDetailResponse>(orderInfo);
                List<TaobaoQueryOrderDetailResponseOrderInfoLineContentNameValue> infoLines = new List<TaobaoQueryOrderDetailResponseOrderInfoLineContentNameValue>();
                foreach (var v in orderDetail.mainOrder.orderInfo.lines)
                {
                    foreach (var vv in v.content)
                    {
                        infoLines.Add(vv.value);
                    }
                }

                var infoLinePayTime = infoLines.FirstOrDefault(obj => obj.name.Contains("付款时间"));
                var infoLineDeliveryTime = infoLines.FirstOrDefault(obj => obj.name.Contains("发货时间"));
                popPayTime = infoLinePayTime != null ? DateTime.Parse(infoLinePayTime.value) : dbMineTime;
                popDeliveryTime = infoLineDeliveryTime != null ? DateTime.Parse(infoLineDeliveryTime.value) : dbMineTime;
                popOrderState = orderDetail.mainOrder.statusInfo.text;
                buyerComment = orderDetail.buyMessage;
                foreach (var v in orderDetail.operationsGuide)
                {
                    foreach (var vv in v.lines)
                    {
                        if (vv.content.Any(obj => obj.value == "标记："))
                        {
                            sellerComment = vv.content[1].value;
                            break;
                        }
                    }
                }
                goodsPrice = float.Parse(orderDetail.mainOrder.totalPrice[0].content[0].value);
                deliveryPrice = float.Parse(orderDetail.mainOrder.totalPrice.FirstOrDefault(obj => obj.content[0].value.Contains("快递")).content[0].value.Replace("(快递:", ""));
                sellerGetMoney = orderDetail.mainOrder.payInfo.actualFee.value;
                foreach (var v in orderDetail.mainOrder.subOrders)
                {
                    namePrice.Add(v.itemInfo.title, v.priceInfo);
                }
                var addN = orderDetail.tabs.FirstOrDefault(obj => obj.id == "logistics");
                if (addN == null)
                {
                    throw new Exception("未找到地址结点");
                }
                reciverInfo = addN.content.address;
            }

            var order = new Order
            {
                CloseOperator = "",
                CloseTime = dbMineTime,
                CreateOperator = "",
                CreateTime = DateTime.Now,
                CreateType = OrderCreateType.DOWNLOAD,
                DeliveryCompany = "",
                DeliveryNumber = "",
                DeliveryOperator = "",
                DeliveryTime = dbMineTime,
                DeliveryMoney = 0,
                Id = 0,
                PopDeliveryTime = popDeliveryTime,
                OrderGoodss = new List<OrderGoods>(),
                ParseResult = true,
                PopBuyerComment = buyerComment,
                PopBuyerId = orderShort.buyer.nick,
                PopBuyerPayMoney = orderShort.payInfo.actualFee,
                PopCodNumber = "",
                PopCodSevFee = 0,
                PopFlag = ConvertFlag(orderShort.extra.sellerFlag),
                PopOrderId = orderShort.id,
                PopOrderTotalMoney = goodsPrice + deliveryPrice,
                PopPayTime = popPayTime,
                PopPayType = PopPayType.ONLINE,
                PopSellerComment = sellerComment,
                PopSellerGetMoney = sellerGetMoney,
                PopState = popOrderState,
                PopType = shop.PopType,
                PrintOperator = "",
                PrintTime = dbMineTime,
                ReceiverAddress = "",
                ReceiverMobile = "",
                ReceiverName = "",
                ReceiverPhone = "",
                ShopId = shop.Id,
                State = ConvertState(orderShort.statusInfo.text.Trim()),
                Type = OrderType.NORMAL,
                Weight = 0,
                DeliveryTemplateId = 0,
                Refused = false,
            };
            foreach (var so in orderShort.subOrders)
            {
                var og = new OrderGoods
                {
                    CloseOperator = "",
                    CloseTime = dbMineTime,
                    Color = so.itemInfo.skuText.FirstOrDefault(obj => obj.name.Contains("颜色")).value,
                    Comment = "",
                    Count = so.quantity,
                    Edtion = "",
                    GetedCount = 0,
                    Id = 0,
                    Image = so.itemInfo.pic,
                    Number = so.itemInfo.extra[0].value,
                    NumberId = 0,
                    OrderId = 0,
                    PopNumber = "",
                    PopOrderSubId = "",
                    PopPrice = namePrice[so.itemInfo.title],
                    PopUrl = "",
                    Price = 0,
                    Size = so.itemInfo.skuText.FirstOrDefault(obj => obj.name.Contains("尺码")).value,
                    State = OrderState.PAYED,
                    PopRefundState = PopRefundState.NOT,
                    Weight = 0,
                    StockOperator = "",
                    StockTime = dbMineTime,
                    Vendor = "",
                    IsPeijian = false,
                };
                og.PopInfo = og.Number + "||颜色:" + og.Color + "|尺码:" + og.Size;

                if (so.operations != null && so.operations.FirstOrDefault(obj => obj.text.Trim() == "退款成功") != null)
                {
                    og.State = OrderState.CANCLED;
                }
                else if (so.operations != null && so.operations.FirstOrDefault(obj => obj.text.Trim() == "请卖家处理" || obj.text.Trim() == "请退款") != null)
                {
                    og.State = OrderState.RETURNING;
                }
                else
                {
                    og.State = OrderState.PAYED;
                }
                order.OrderGoodss.Add(og);
            }

            if (order.OrderGoodss.Select(obj => obj.State).Distinct().Count() == 1)
            {
                order.State = order.OrderGoodss[0].State;
            }

            string add = "";
            string[] reinfos = reciverInfo.Split(new char[] { '，', ',' }, StringSplitOptions.RemoveEmptyEntries);
            order.ReceiverName = reinfos[0].Trim();
            order.ReceiverMobile = reinfos[1].Replace("86-", "");
            int start = 2;
            if (reinfos[2].All(c => Char.IsDigit(c) || c == '-'))
            {
                order.ReceiverPhone = reinfos[2];
                start = 3;
            }
            for (; start < reinfos.Length; start++)
            {
                if (start == reinfos.Length - 1 && reinfos[start].All(c => Char.IsDigit(c)))
                {
                    break;
                }
                add += reinfos[start] + ",";
            }
            order.ReceiverAddress = add.Trim(',');
            return order;
        }

        private List<OrderDownload> GetOrders()
        {
            List<OrderDownload> allOrders = new List<OrderDownload>();

            int totalCount = 0, currentCount = 0;
            int totalPage = 0, currentPage = 1;
            string htmlRet = this.wb1.GetTextAsync().Result;
            var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            var shop = allShops.FirstOrDefault(obj => htmlRet.Contains(obj.PopSellerId));

            if (shop == null)
            {
                throw new Exception("系统中没有找到相应店铺");
            }

            while (this.isRunning)
            {
                string script = ScriptManager.GetBody(jspath, "//TAOBAO_SEARCH_ORDER").Replace("###prePageNo", (currentPage - 1 >= 0 ? currentPage - 1 : 1).ToString()).Replace("###pageNum", currentPage.ToString());
                var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(script, "", 1, new TimeSpan(0, 0, 30));
                var ret = task.Result;

                if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
                {
                    throw new Exception("执行操作失败：" + ret.Message);
                }
                var or = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderListResponse>(ret.Result.ToString());
                if (or.page == null)
                {
                    throw new Exception("执行操作失败：返回数据格式无法识别");
                }
                if (or.mainOrders == null || or.mainOrders.Length < 1)
                {
                    break;
                }
                totalCount = or.page.totalNumber;
                totalPage = or.page.totalPage;

                List<OrderDownload> orders = new List<OrderDownload>(1);
                foreach (var v in or.mainOrders)
                {
                    OrderDownload od = new OrderDownload();
                    orders.Clear();
                    orders.Add(od);
                    try
                    {
                        this.tbMsg.Text = string.Format("正在下载：{0}/{1} {2} ", currentCount, totalCount, v.id);
                        WPFHelper.DoEvents();
                        var odInDb = ServiceContainer.GetService<OrderService>().GetByPopOrderId(v.id);
                        if (odInDb.Total >= 1)
                        {
                            od.Order = odInDb.First;
                            var state = ConvertState(v.statusInfo.text);
                            if (od.Order.State == state || od.Order.State == OrderState.CLOSED || od.Order.State == OrderState.CANCLED)
                            {
                                continue;
                            }
                            if (state == OrderState.RETURNING || state == OrderState.CLOSED || state == OrderState.CANCLED)
                            {
                                od.Order.State = state;
                                od.Order.PopState = v.statusInfo.text;
                                var resp = ServiceContainer.GetService<OrderService>().Update(od.Order);
                            }
                        }
                        else
                        {
                            var order = ParseOrder(v, shop);
                            od.Order = order;
                            var resp = ServiceContainer.GetService<OrderService>().SaveOrUpdateOrdersByPopOrderId(shop, orders);
                            od = resp.First;
                        }

                        if (this.isRunning == false)
                        {
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        od.Error = new OrderDownloadError(shop.Id, v.id, "", ex.Message, ex.StackTrace);
                    }
                    finally
                    {
                        currentCount++;
                        allOrders.Add(od);
                    }

                }
                currentPage++;
            }
            return allOrders;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if (this.isRunning)
                {
                    this.isRunning = false;
                    return;
                }
                this.btnRefresh.Content = "停止";
                this.isRunning = true;

                this.orders.Clear();
                string htmlRet = this.wb1.GetTextAsync().Result;
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                var shop = shops.FirstOrDefault(obj => htmlRet.Contains(obj.PopSellerId));
                var downloadOrders = GetOrders();
                if (downloadOrders == null || downloadOrders.Count < 1)
                {
                    MessageBox.Show("没有找到待发货的订单");
                    return;
                }
                var os = ServiceContainer.GetService<OrderService>();
                var orders = downloadOrders.Where(obj => obj.Order != null).Select(obj => obj.Order).Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false && os.IsDBMinTime(obj.PopDeliveryTime)).Select(obj => new OrderViewModel(obj)).OrderBy(obj => obj.Source.PopPayTime).ToArray();
                //分析
                foreach (var order in orders)
                {
                    var time = DateTime.Now.Subtract(order.Source.PopPayTime).TotalHours;
                    var sTime = shops.FirstOrDefault(obj => obj.Id == order.Source.ShopId).ShippingHours;
                    if (time >= sTime)
                    {
                        order.Background = Brushes.Red;
                        order.IsChecked = true;
                    }
                    else if (time - sTime >= -1)
                    {
                        order.Background = Brushes.Yellow;
                        order.IsChecked = true;
                    }
                    this.orders.Add(order);
                }
                this.dgvOrders.ItemsSource = this.orders;
                this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";

                var error = downloadOrders.Where(obj => obj.Error != null).Select(obj => obj.Error).ToArray();
                if (error.Length > 0)
                {
                    string msg = string.Format("下载失败订单列表：{0}", string.Join(",", error.Select(obj => obj.PopOrderId)));
                    MessageBox.Show(msg, "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
            finally
            {
                this.isRunning = false;
                this.btnRefresh.Content = "刷新";
            }
        }

        private PopOrderState ParseOrderState(string popOrderId)
        {
            var pos = new PopOrderState()
            {
                PopOrderId = popOrderId,
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

            var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<ShopErp.App.Domain.TaobaoHtml.Order.TmallQueryOrderDetailResponse>(orderInfo);

            pos.PopOrderStateValue = oi.overStatus.status.content[0].text;
            pos.State = ConvertState(pos.PopOrderStateValue);
            return pos;
        }

        private void MarkPopDelivery(string popOrderId, string deliveryCompany, string deliveryNumber)
        {
            //订单信息
            var js = ScriptManager.GetBody(jspath, "//TAOBAO_MARK_DELIVERY").Replace("###companyCode", deliveryCompany).Replace("###mailNo", deliveryNumber).Replace("###taobaoTradeId", popOrderId).Replace("###trade_id", popOrderId);
            var task = wb1.GetBrowser().MainFrame.EvaluateScriptAsync(js, "", 1, new TimeSpan(0, 0, 30));
            var ret = task.Result;
            if (ret.Success == false || (ret.Result != null && ret.Result.ToString().StartsWith("ERROR")))
            {
                throw new Exception("执行操作失败：" + ret.Message);
            }
            var content = ret.Result.ToString();
            if (content.Contains("运单号不符合规则或已经被使用"))
            {
                throw new Exception("运单号不符合规则或已经被使用");
            }
        }

        private void btnMarkDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var so = this.orders.Where(obj => obj.IsChecked).ToArray();
                if (so.Length < 1)
                {
                    throw new Exception("没有选择订单");
                }
                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas;
                var os = ServiceContainer.GetService<OrderService>();
                foreach (var o in so)
                {
                    WPFHelper.DoEvents();
                    try
                    {
                        var st = ParseOrderState(o.Source.PopOrderId);
                        if ((int)st.State >= (int)(OrderState.SHIPPED))
                        {
                            o.State = "订单已经发货";
                            o.Background = null;
                            o.Source.PopDeliveryTime = DateTime.Now;
                        }
                        else
                        {
                            var dc = dcs.FirstOrDefault(obj => obj.Name == o.DeliveryCompany).PopMapTaobao;
                            MarkPopDelivery(o.Source.PopOrderId, dc, o.DeliveryNumber);
                            o.State = "标记成功";
                            o.Background = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        o.State = ex.Message;
                        o.Background = Brushes.Red;
                    }
                }
                MessageBox.Show("所有订单标记完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnGoToTaobao_Click(object sender, RoutedEventArgs e)
        {
            this.wb1.Load("https://trade.taobao.com/trade/itemlist/list_sold_items.htm");
        }

        #region 前选 后选 

        private OrderViewModel GetMIOrder(object sender)
        {
            MenuItem mi = sender as MenuItem;
            var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
            var cells = dg.SelectedCells;
            if (cells.Count < 1)
            {
                throw new Exception("未选择数据");
            }

            var item = cells[0].Item as OrderViewModel;
            if (item == null)
            {
                throw new Exception("数据对象不正确");
            }
            return item;
        }

        private void miSelectPre_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);
                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i <= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void miSelectForward_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = this.orders;
                int index = orders.IndexOf(item);

                for (int i = 0; i < orders.Count; i++)
                {
                    orders[i].IsChecked = i >= index ? true : false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        private void dgvOrders_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                if (this.dgvOrders.SelectedCells.Count < 1)
                {
                    return;
                }
                var item = this.dgvOrders.SelectedCells[0].Item as OrderViewModel;
                if (item == null)
                {
                    throw new Exception("对象数据类型不为：" + typeof(OrderViewModel).FullName);
                }
                if (string.IsNullOrWhiteSpace(item.Source.PopOrderId))
                {
                    throw new Exception("该订单没有平台订单编号");
                }

                if ((int)item.Source.State >= (int)(OrderState.RETURNING))
                {
                    if (MessageBox.Show("订单状态不正确：" + item.Source.State + " 是否确认要发货？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                this.wb1.Load("https://wuliu.taobao.com/user/consign.htm?trade_id=" + item.Source.PopOrderId);
                Clipboard.SetText(item.Source.DeliveryNumber);
                this.tbMsg1.Text = "已自动复制:" + item.Source.DeliveryNumber;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void wb1_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            try
            {
                if (e.IsLoading)
                {
                    return;
                }

                string url = e.Browser.MainFrame.Url;
                if (url.Contains("https://wuliu.taobao.com/user/order_detail_old.htm") == false)
                {
                    return;
                }

                this.Dispatcher.BeginInvoke(new Action(ParseResult));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private string FindNumbers(string ss, string mark)
        {
            int index = ss.IndexOf(mark);
            if (index < 0)
            {
                return "";
            }
            int iStart = index + mark.Length;
            while ((Char.IsDigit(ss[iStart]) == false && Char.IsUpper(ss[iStart]) == false) && iStart < ss.Length) iStart++;
            if (iStart == ss.Length)
            {
                MessageBox.Show("标记发货失败，返回数据中未找到运单号码：");
                return "";
            }
            int iEnd = iStart + 1;
            while ((Char.IsDigit(ss[iEnd]) || char.IsUpper(ss[iEnd])) && iEnd < ss.Length) iEnd++;
            string dn = ss.Substring(iStart, iEnd - iStart);
            return dn.Trim();
        }

        private void ParseResult()
        {
            try
            {
                string ss = this.wb1.GetMainFrame().GetTextAsync().Result.ToUpper();
                string dn = FindNumbers(ss, "运单号码：");
                string oid = FindNumbers(ss, "订单编号：");

                //搜索订单编号与运单号是否匹配
                var order = this.orders.FirstOrDefault(obj => obj.Source.PopOrderId.Equals(oid, StringComparison.OrdinalIgnoreCase));
                if (order == null)
                {
                    MessageBox.Show("标记发货失败，返回数据订单编号找不到订单：" + oid, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (order.DeliveryNumber.Equals(dn, StringComparison.OrdinalIgnoreCase) == false)
                {
                    MessageBox.Show("标记发货失败，返回的快递单号与指定的订单快递单号不一致" + oid, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (ss.Contains(order.DeliveryCompany) == false)
                {
                    MessageBox.Show("标记发货失败，返回的快递公司与指定的快递公司不一致" + oid, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var os = ServiceContainer.GetService<OrderService>();
                os.MarkPopDelivery(order.Source.Id, os.FormatTime(DateTime.Now));
                order.State = "已标记";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}