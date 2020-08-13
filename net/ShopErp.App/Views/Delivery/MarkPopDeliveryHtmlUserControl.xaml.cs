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
using ShopErp.App.CefSharpUtils;
using ShopErp.App.Service.Net;
using System.ComponentModel;
using System.Web;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for OrderSyncUserControl.xaml
    /// </summary>
    public partial class MarkPopDeliveryHtmlUserControl : UserControl
    {
        private bool isRunning = false;
        private ObservableCollection<OrderViewModel> orders = new ObservableCollection<OrderViewModel>();

        public MarkPopDeliveryHtmlUserControl()
        {
            InitializeComponent();
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
            string url = shop.PopType == PopType.TMALL ? "https://trade.tmall.com/detail/orderDetail.htm?bizOrderId=" : "https://trade.taobao.com/detail/orderDetail.htm?bizOrderId=";
            //订单信息
            var content = MsHttpRestful.GetReturnString(url + orderShort.id, CefCookieVisitor.GetCookieValue(shop.PopType == PopType.TMALL ? "trade.tmall.com" : "trade.taobao.com"));
            string title = shop.PopType == PopType.TMALL ? "var detailData" : "var data = JSON";

            int si = content.IndexOf(title);
            if (si <= 0)
            {
                throw new Exception("未找到订单详情数据开始标识" + title + " 请点击订单详情进行验证");
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
                //运费显示有三种：运费(快递) 运费(平邮) 运费(EMS)
                if (contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("运费(快递)")) == null)
                {
                    sellerComment += "[系统自动识别：此件发邮政]";
                }

                foreach (var vv in orderDetail.orders.list)
                {
                    foreach (var vvv in vv.status)
                    {
                        foreach (var vvvv in vvv.subOrders)
                        {
                            namePrice[vvvv.itemInfo.title] = float.Parse(vvvv.priceInfo[0].text.Trim());
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
                    namePrice[v.itemInfo.title] = v.priceInfo;
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
                    GoodsId = 0,
                    OrderId = 0,
                    PopOrderSubId = "",
                    PopPrice = namePrice[so.itemInfo.title],
                    PopUrl = "",
                    Price = 0,
                    Size = so.itemInfo.skuText.FirstOrDefault(obj => obj.name.Contains("尺码")).value,
                    State = OrderState.PAYED,
                    Weight = 0,
                    StockOperator = "",
                    StockTime = dbMineTime,
                    Vendor = "",
                    IsPeijian = false,
                };
                og.PopInfo = og.Number + " " + og.Color + " " + og.Size;

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
            order.ReceiverName = HttpUtility.HtmlDecode(reinfos[0].Trim());
            order.ReceiverMobile = HttpUtility.HtmlDecode(reinfos[1].Replace("86-", ""));
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
            order.ReceiverAddress = HttpUtility.HtmlDecode(add.Trim(','));
            return order;
        }

        private List<OrderDownload> GetOrders()
        {
            List<OrderDownload> allOrders = new List<OrderDownload>();

            int totalCount = 0, currentCount = 0;
            int totalPage = 0, currentPage = 1;
            var allShops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
            var shop = MainWindow.ProgramMainWindow.QueryUserControlInstance<AttachUI.TaobaoUserControl>().GetLoginShop();
            if (shop == null)
            {
                throw new Exception("系统中没有找到相应店铺");
            }
            Dictionary<string, string> param = new Dictionary<string, string>();
            param["action"] = "itemlist/SoldQueryAction";
            param["auctionType"] = "0";
            param["orderStatus"] = "PAID";
            param["tabCode"] = "waitSend";
            param["pageSize"] = "15";
            while (this.isRunning)
            {
                param["prePageNo"] = (currentPage - 1 >= 0 ? currentPage - 1 : 1).ToString();
                param["pageNum"] = currentPage.ToString();
                var header = CefCookieVisitor.GetCookieValue("trade.taobao.com");
                string ret = MsHttpRestful.PostUrlEncodeBodyReturnString("https://trade.taobao.com/trade/itemlist/asyncSold.htm?event_submit_do_query=1&_input_charset=utf8", param, header, null, "https://trade.taobao.com/trade/itemlist/list_sold_items.htm");
                var or = Newtonsoft.Json.JsonConvert.DeserializeObject<TaobaoQueryOrderListResponse>(ret);
                if (or.page == null)
                {
                    throw new Exception("执行操作失败：返回数据格式无法识别,请点击订单下一页或者上一页进行验证");
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

                            //未发货订单，即使有退款商品，整个订单状态也是待发货不是退款中,需要检测对应商品显示的信息，如果某个商品退款成功，但还有其它商品也会在发货中
                            if (state == OrderState.PAYED && v.subOrders.All(obj => obj.operations != null && (obj.operations.FirstOrDefault(op => op.text.Trim() == "退款成功" || op.text.Trim() == "请卖家处理" || op.text.Trim() == "请退款") != null)))
                            {
                                state = OrderState.RETURNING;
                            }
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
                    if (order.Source.State == OrderState.SHIPPED)
                    {
                        order.IsChecked = true;
                    }
                    this.orders.Add(order);
                }
                this.dgvOrders.ItemsSource = this.orders;
                this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";

                var error = downloadOrders.Where(obj => obj.Error != null).Select(obj => obj.Error).ToArray();
                if (error.Length > 0)
                {
                    string msg = string.Format("下载失败订单列表：\r\n{0}", string.Join(Environment.NewLine, error.Select(obj => obj.PopOrderId + ":" + obj.Error)));
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

        private void MarkPopDelivery(string popOrderId, string deliveryCompany, string deliveryNumber)
        {
            string formId = "//form[@id = 'orderForm']";
            string inputMark = "input";
            //第一步读取页面信息,需要从该页面中提取要发送的数据
            var uri = new Uri("https://wuliu.taobao.com/user/consign.htm?trade_id=" + popOrderId);
            Dictionary<string, string> headers = CefCookieVisitor.GetCookieValue(uri.Host);
            string html = MsHttpRestful.GetReturnString(uri.OriginalString, headers);

            //第二步 提取数据
            Dictionary<string, string> paras = new Dictionary<string, string>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var form = doc.DocumentNode.SelectSingleNode(formId);
            if (form == null)
            {
                Debug.WriteLine(form);
                throw new Exception("未在页面中找到：" + formId);
            }

            var inputNodes = doc.DocumentNode.SelectNodes("//input");
            if (inputNodes == null || inputNodes.Count < 1)
            {
                throw new Exception("未找到任何：" + inputMark + " 元素");
            }

            foreach (var v in inputNodes)
            {
                if (string.IsNullOrWhiteSpace(v.GetAttributeValue("name", "")))
                {
                    continue;
                }
                paras[v.GetAttributeValue("name", "")] = v.GetAttributeValue("value", "");
            }
            paras.Remove("q");
            paras.Remove("type");
            paras.Remove("cat");
            paras["companyName"] = "请输入物流公司名称";
            paras["event_submit_do_offline_consign"] = "1";
            paras["offlineNewFlag"] = "1";
            paras["_fmw.r._0.c"] = paras["receiverContactName"];
            paras["_fmw.r._0.adr"] = paras["receiverDetail"];
            paras["_fmw.f._0.fe"] = "";
            paras["_fmw.f._0.fet"] = "";
            paras["_fmw.n._0.g"] = "您可以在此输入备忘信息（仅卖家自己可见）。";
            paras["initialWeightOld"] = "all";
            //进行几种常见错误的检测
            if (paras.ContainsKey("receiverZipCode") == false || paras["receiverZipCode"].Length != 6)
            {
                throw new Exception("邮政编码不为6位,当前值：" + paras["receiverZipCode"]);
            }
            if (paras.ContainsKey("receiverDetail") == false || paras["receiverDetail"].Length < 4)
            {
                throw new Exception("详细地址必须大于等于4个字，当前值：" + paras["receiverDetail"]);
            }

            paras["mailNo"] = deliveryNumber;
            paras["companyCode"] = deliveryCompany;
            //页面使用的什么编码，发送数据就要使用什么编码，否则对面无法接收
            string ret = MsHttpRestful.PostUrlEncodeBodyReturnString(uri.OriginalString, paras, headers, doc.DeclaredEncoding ?? doc.Encoding);
            if (ret.Contains("恭喜您，操作成功"))
            {
                return;
            }
            if (ret.Contains("运单号不符合规则或已经被使用"))
            {
                throw new Exception("运单号不符合规则或已经被使用");
            }
            throw new Exception("返回页面数据无法识别，请查看相应结果");
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

                if (so.Any(obj => obj.Source.State == OrderState.RETURNING))
                {
                    if (MessageBox.Show("所选订单中含有退款中订单是否确认发货？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Asterisk) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas;
                var os = ServiceContainer.GetService<OrderService>();
                foreach (var o in so)
                {
                    WPFHelper.DoEvents();
                    try
                    {
                        if (string.IsNullOrEmpty(o.DeliveryNumber))
                        {
                            throw new Exception("快递单号为空");
                        }
                        var dc = dcs.FirstOrDefault(obj => obj.Name == o.DeliveryCompany).PopMapTaobao;
                        MarkPopDelivery(o.Source.PopOrderId, dc, o.DeliveryNumber);
                        o.State = "标记成功";
                        o.Background = null;
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

        private void DgvOrders_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                string sortPath = e.Column.SortMemberPath;
                if (this.orders.Count < 1)
                {
                    return;
                }
                var sortType = e.Column.SortDirection == null ? ListSortDirection.Ascending : (e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                List<OrderViewModel> newVms = null;

                EnumerableKeySelector selector = new EnumerableKeySelector(orders[0].GetType(), sortPath);
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = orders.OrderBy(obj => selector.GetData(obj)).ToList();
                }
                else
                {
                    newVms = orders.OrderByDescending(obj => selector.GetData(obj)).ToList();
                }
                this.orders.Clear();
                foreach (var v in newVms)
                {
                    this.orders.Add(v);
                }
                e.Column.SortDirection = sortType;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}