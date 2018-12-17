using ShopErp.App.Views.Orders.Taobao;
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

        private OrderState ConveretState(string state)
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
            if (state.Contains("订单部分退款中"))
            {
                return OrderState.RETURNING;
            }
            if (state == "交易关闭")
            {
                return OrderState.CANCLED;
            }
            if (state == "交易成功")
            {
                return OrderState.SUCCESS;
            }

            return OrderState.WAITPAY;
        }

        private void ParseOrder(TaobaoQueryOrdersResponseOrder v, long shopId)
        {
            var dm = ServiceContainer.GetService<SystemConfigService>().Get(-1, "DELIVERY_MONEY", "7");
            if (dm == null)
            {
                throw new Exception("数据库没有快递运费：DELIVERY_MONEY");
            }
            var deliveryMoney = float.Parse(dm);
            var dbMineTime = ServiceContainer.GetService<OrderService>().GetDBMinTime();
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
                DeliveryMoney = deliveryMoney,
                Id = 0,
                PopDeliveryTime = dbMineTime,
                OrderGoodss = new List<OrderGoods>(),
                ParseResult = true,
                PopBuyerComment = "",
                PopBuyerId = v.buyer.nick,
                PopBuyerPayMoney = v.payInfo.actualFee,
                PopCodNumber = "",
                PopCodSevFee = 0,
                PopCreateTime = DateTime.Parse(v.orderInfo.createTime),
                PopFlag = ConvertFlag(v.extra.sellerFlag),
                PopOrderId = v.id,
                PopOrderTotalMoney = v.payInfo.actualFee,
                PopPayTime = dbMineTime,
                PopPayType = PopPayType.ONLINE,
                PopSellerComment = "",
                PopSellerGetMoney = v.payInfo.actualFee,
                PopState = "",
                PopType = PopType.TMALL,
                PrintOperator = "",
                PrintTime = dbMineTime,
                ReceiverAddress = "",
                ReceiverMobile = "",
                ReceiverName = "",
                ReceiverPhone = "",
                ShopId = shopId,
                State = ConveretState(v.statusInfo.text.Trim()),
                Type = OrderType.NORMAL,
                Weight = 0,
            };

            //订单信息
            var js = ScriptManager.GetBody(jspath, "//TAOBAO_GET_ORDER").Replace("###bizOrderId", v.id);
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

            var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<Views.Orders.Taobao.TaobaoQueryOrderDetailResponse>(orderInfo);

            string time = oi.stepbar.options.First(obj => obj.content == "买家付款").time;
            order.PopPayTime = string.IsNullOrWhiteSpace(time) ? dbMineTime : DateTime.Parse(time);

            time = oi.stepbar.options.First(obj => obj.content == "发货").time;
            order.PopDeliveryTime = string.IsNullOrWhiteSpace(time) ? dbMineTime : DateTime.Parse(time);

            //time = oi.stepbar.options.First(obj => obj.content == "买家确认收货").time;
            //order.pop = string.IsNullOrWhiteSpace(time) ? dbMineTime : DateTime.Parse(time);

            order.PopBuyerComment = oi.basic.lists.First(obj => obj.key == "买家留言").content[0].text;
            if (order.PopBuyerComment == "-")
            {
                order.PopBuyerComment = "";
            }

            var addN = oi.basic.lists.First(obj => obj.key == "收货地址").content[0];
            string reinfo = "";
            //html 表示地址是要经过转运的地址，label是不需要经过转运的大陆地址
            if (addN.type.Equals("html", StringComparison.OrdinalIgnoreCase))
            {
                HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(addN.text);
                string hh = document.DocumentNode.InnerText;
                string nhh = hh.Substring(0, hh.IndexOf("]转&nbsp;") + 1);// 
                string read = nhh.Replace("[", "").Replace("]", "");
                string mark = "转运仓库";
                if (read.IndexOf(mark) > 0)
                {
                    read = read.Remove(read.IndexOf(mark), mark.Length + 1);
                }
                reinfo = read;
            }
            else if (addN.type.Equals("label", StringComparison.OrdinalIgnoreCase))
            {
                reinfo = addN.text;
            }
            else
            {
                throw new Exception("无法识别的地址格式");
            }

            string add = "";
            string[] reinfos = reinfo.Split(',');
            order.ReceiverName = reinfos[0].Trim();
            order.ReceiverMobile = reinfos[1].Replace("86-", "");
            if (reinfos[2].All(c => Char.IsDigit(c) || c == '-'))
            {
                order.ReceiverPhone = reinfos[2];
                for (int i = 3; i < reinfos.Length; i++)
                {
                    add += reinfos[i];
                }
            }
            else
            {
                for (int i = 2; i < reinfos.Length; i++)
                {
                    add += reinfos[i];
                }
            }
            order.ReceiverAddress = add;
            //订单金额
            var contents = new List<TaobaoQueryOrderDetailResponseAmountCountContent>();
            foreach (var c in oi.amount.count)
            {
                foreach (var cc in c)
                {
                    contents.AddRange(cc.content);
                }
            }
            var goodsPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text == "商品总价").data.money.text.Replace("￥", "").Trim();
            var deliveryPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("运费")).data.money.text.Replace("￥", "").Trim();
            var buyerPayPrice = contents.FirstOrDefault(obj => obj.data.titleLink.text.Contains("订单总价")).data.money.text.Replace("￥", "").Trim();
            var sellerGetMoney = contents.FirstOrDefault(obj => obj.data.titleLink != null && (obj.data.titleLink.text.Contains("应收款") || obj.data.titleLink.text.Contains("实收款")));

            order.PopOrderTotalMoney = float.Parse(goodsPrice) + float.Parse(deliveryPrice);
            order.PopBuyerPayMoney = float.Parse(buyerPayPrice);
            order.PopSellerGetMoney = float.Parse(sellerGetMoney.data.dotPrefixMoney.text + sellerGetMoney.data.dotSufixMoney.text);

            if (oi.overStatus.prompt != null && oi.overStatus.prompt.FirstOrDefault(obj => obj.key == "物流") != null)
            {
                order.DeliveryCompany = oi.overStatus.prompt.FirstOrDefault(obj => obj.key == "物流").content[0].companyName;
                order.DeliveryNumber = oi.overStatus.prompt.FirstOrDefault(obj => obj.key == "物流").content[0].mailNo;
            }
            if (oi.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)) != null)
            {
                string comment = oi.overStatus.operate.FirstOrDefault(obj => string.IsNullOrWhiteSpace(obj.key)).content[0].text;
                si = comment.IndexOf("备忘：</span><span>");
                ei = comment.IndexOf("</span>", si + "备忘：</span><span>".Length);
                string sellerComment = comment.Substring(si + "备忘：</span><span>".Length, ei - si - "备忘：</span><span>".Length);
                order.PopSellerComment = sellerComment.TrimStart('#');
            }
            Dictionary<string, float> namePrice = new Dictionary<string, float>();
            foreach (var vv in oi.orders.list)
            {
                foreach (var vvv in vv.status)
                {
                    foreach (var vvvv in vvv.subOrders)
                    {
                        if (namePrice.ContainsKey(vvvv.itemInfo.title) == false)
                        {
                            namePrice.Add(vvvv.itemInfo.title.Trim(), float.Parse(vvvv.priceInfo[0].text.Trim()));
                        }
                    }
                }
            }
            foreach (var so in v.subOrders)
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
                    PopPrice = 0,
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
                og.PopPrice = namePrice[so.itemInfo.title];
                og.PopInfo = og.Number + "||颜色:" + og.Color + "|尺码:" + og.Size;
                order.OrderGoodss.Add(og);
            }
            ServiceContainer.GetService<OrderService>().Save(order);
        }

        private List<Order> GetOrders()
        {
            List<Order> orders = new List<Order>();

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

                var or = Newtonsoft.Json.JsonConvert.DeserializeObject<Views.Orders.Taobao.TaobaoQueryOrdersResponse>(ret.Result.ToString());

                if (or.mainOrders == null || or.mainOrders.Length < 1)
                {
                    if (orders.Count < 1)
                    {
                        throw new Exception("没有订单");
                    }
                    else
                    {
                        break;
                    }
                }
                totalCount = or.page.totalNumber;
                totalPage = or.page.totalPage;
                foreach (var v in or.mainOrders)
                {
                    var o = ServiceContainer.GetService<OrderService>().GetByPopOrderId(v.id).First;
                    if (o == null)
                    {
                        ParseOrder(v, shop.Id);
                        o = ServiceContainer.GetService<OrderService>().GetByPopOrderId(v.id).First;
                        if (o == null)
                        {
                            throw new Exception("订单不在本地系统中，保存后重新读取也不存在");
                        }
                    }
                    else
                    {

                    }
                    orders.Add(o);
                    currentCount++;
                    this.tbMsg.Text = string.Format("已经下载：{0}/{1} {2} {3} ", currentCount, totalCount, v.id, v.orderInfo.createTime);
                    WPFHelper.DoEvents();
                }
                currentPage++;
            }
            return orders;
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
                var orders = downloadOrders.Where(obj => string.IsNullOrWhiteSpace(obj.PopOrderId) == false && os.IsDBMinTime(obj.PopDeliveryTime)).Select(obj => new OrderViewModel(obj)).OrderBy(obj => obj.Source.PopPayTime).ToArray();
                if (orders.Length < 1)
                {
                    MessageBox.Show("没有找到待发货的订单");
                    return;
                }
                foreach (var v in downloadOrders)
                {
                    Debug.WriteLine(v.Id + " " + v.PopOrderId + v.PopDeliveryTime);
                }
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

            var oi = Newtonsoft.Json.JsonConvert.DeserializeObject<ShopErp.App.Views.Orders.Taobao.TaobaoQueryOrderDetailResponse>(orderInfo);

            pos.PopOrderStateValue = oi.overStatus.status.content[0].text;
            pos.PopOrderStateDesc = oi.overStatus.status.content[0].text;
            pos.State = ConveretState(pos.PopOrderStateValue);
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
                        var st = ParseOrder(o.Source.PopOrderId);
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
            while (Char.IsDigit(ss[iStart]) == false && iStart < ss.Length) iStart++;
            if (iStart == ss.Length)
            {
                MessageBox.Show("标记发货失败，返回数据中未找到运单号码：");
                return "";
            }
            int iEnd = iStart + 1;
            while (Char.IsDigit(ss[iEnd]) && iStart < ss.Length) iEnd++;
            string dn = ss.Substring(iStart, iEnd - iStart);
            return dn.Trim();
        }

        private void ParseResult()
        {
            try
            {
                string ss = this.wb1.GetMainFrame().GetTextAsync().Result;
                string dn = FindNumbers(ss, "运单号码：");
                string oid = FindNumbers(ss, "订单编号：");

                //搜索订单编号与运单号是否匹配
                var order = this.orders.FirstOrDefault(obj => obj.Source.PopOrderId == oid);
                if (order == null)
                {
                    MessageBox.Show("标记发货失败，返回数据订单编号找不到订单：" + oid, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (order.DeliveryNumber != dn)
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