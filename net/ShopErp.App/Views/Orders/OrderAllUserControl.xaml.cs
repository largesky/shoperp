using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.ExceptionServices;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.ViewModels;
using ShopErp.App.Views.PageBar;
using Microsoft.Win32;
using ShopErp.App.Views.Finance;
using ShopErp.App.Domain;
using ShopErp.App.Service;
using ShopErp.App.Service.Net;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using ShopErp.App.Service.Print;
using ShopErp.App.Service.Print.PrintDocument;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderAllUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderAllUserControl : UserControl
    {
        private bool myLoaded = false;

        private const int QUERY_TYPE_ID = 1;

        private const int QUERY_TYPE_OTHER = 3;

        private int currentQueryType = 0;
        private OrderService orderService = null;
        private VendorService vs = null;

        public OrderAllUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myLoaded)
            {
                return;
            }
            try
            {
                this.dpStart.Value = DateTime.Now.AddDays(-30);
                this.cbbOrderType.Bind<OrderType>();
                this.cbbState.Bind<OrderState>();
                this.cbbCreateType.Bind<OrderCreateType>();
                orderService = ServiceContainer.GetService<OrderService>();
                vs = ServiceContainer.GetService<VendorService>();

                //快递公司
                var coms = DeliveryCompanyService.GetDeliveryCompaniyNames().ToList();
                coms.Insert(0, "");
                this.cbbDeliveryCompany.ItemsSource = coms;

                //店铺
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled)
                    .ToList();
                shops.Insert(0, new Shop { Mark = "" });
                this.cbbShops.ItemsSource = shops;

                //旗帜
                var flags = new ColorFlag[]
                {
                    ColorFlag.UN_LABEL, ColorFlag.RED, ColorFlag.YELLOW, ColorFlag.GREEN, ColorFlag.BLUE, ColorFlag.PINK
                };
                var flagVms = flags.Select(obj => new OrderFlagViewModel(false, obj)).ToArray();
                this.cbbFlags.ItemsSource = flagVms;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.pbBar.Parameters.Clear();
            try
            {
                string strId = this.tbId.Text.Trim();
                if (string.IsNullOrWhiteSpace(strId) == false)
                {
                    this.pbBar.Parameters.Add("id", strId);
                    this.currentQueryType = QUERY_TYPE_ID;
                    this.pbBar.StartPage();
                    return;
                }

                this.currentQueryType = QUERY_TYPE_OTHER;

                this.pbBar.Parameters.Add("PopBuyerId", this.tbPopBuyerId.Text.Trim());
                this.pbBar.Parameters.Add("ReceiverPhone", this.tbReceiverPhone.Text.Trim());
                this.pbBar.Parameters.Add("ReceiverMobile", this.tbReceiverMobile.Text.Trim());
                this.pbBar.Parameters.Add("ReceiverName", this.tbReceiverName.Text.Trim());
                this.pbBar.Parameters.Add("ReceiverAddress", this.tbReceiverAddress.Text.Trim());

                this.pbBar.Parameters.Add("TimeType", this.cbbTimeType.SelectedIndex);
                this.pbBar.Parameters.Add("StartTime", this.dpStart.Value == null ? DateTime.Now.AddDays(-45) : this.dpStart.Value.Value);
                this.pbBar.Parameters.Add("EndTime", this.dpEnd.Value == null ? DateTime.MinValue : this.dpEnd.Value.Value);
                this.pbBar.Parameters.Add("DeliveryCompany", this.cbbDeliveryCompany.Text.Trim());
                this.pbBar.Parameters.Add("DeliveryNumber", this.tbDeliveryNumber.Text.Trim());

                this.pbBar.Parameters.Add("OrderState", this.cbbState.GetSelectedEnum<OrderState>());
                this.pbBar.Parameters.Add("Type", this.cbbOrderType.GetSelectedEnum<OrderType>());
                this.pbBar.Parameters.Add("Vendor", this.tbVendor.Text.Trim());
                this.pbBar.Parameters.Add("Number", this.tbNumber.Text.Trim());

                this.pbBar.Parameters.Add("Flags", this.cbbFlags.Items.OfType<OrderFlagViewModel>().Where(obj => obj.IsChecked).Select(obj => obj.Flag).ToArray());
                this.pbBar.Parameters.Add("ParseResult", this.cbbParseResult.SelectedIndex - 1);
                this.pbBar.Parameters.Add("CreateType", this.cbbCreateType.GetSelectedEnum<OrderCreateType>());
                this.pbBar.Parameters.Add("SellerComment", this.tbSellerComment.Text.Trim());
                this.pbBar.Parameters.Add("ShopId", this.cbbShops.SelectedItem == null ? 0 : (this.cbbShops.SelectedItem as Shop).Id);

                this.pbBar.StartPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [HandleProcessCorruptedStateExceptions]
        private void pbBar_PageChanging(object sender, PageChangeEventArgs e)
        {
            try
            {
                DataCollectionResponse<Order> data = null;
                if (this.currentQueryType == QUERY_TYPE_ID)
                {
                    data = this.orderService.GetById(e.GetParameter<string>("id"));
                }
                else if (this.currentQueryType == QUERY_TYPE_OTHER)
                {
                    data = ServiceContainer.GetService<OrderService>().GetByAll(e.GetParameter<string>("PopBuyerId"),
                        e.GetParameter<string>("ReceiverPhone"), e.GetParameter<string>("ReceiverMobile"),
                        e.GetParameter<string>("ReceiverName"), e.GetParameter<string>("ReceiverAddress"),
                        e.GetParameter<int>("TimeType"), e.GetParameter<DateTime>("StartTime"),
                        e.GetParameter<DateTime>("EndTime"), e.GetParameter<string>("DeliveryCompany"),
                        e.GetParameter<string>("DeliveryNumber"),
                        e.GetParameter<OrderState>("OrderState"), PopPayType.None, e.GetParameter<string>("Vendor"),
                        e.GetParameter<string>("Number"),
                        e.GetParameter<ColorFlag[]>("Flags"), e.GetParameter<int>("ParseResult"),
                        e.GetParameter<string>("SellerComment"), e.GetParameter<long>("ShopId"),
                        e.GetParameter<OrderCreateType>("CreateType"), e.GetParameter<OrderType>("Type"),
                        e.CurrentPage - 1, e.PageSize);
                }

                this.pbBar.Total = data.Total;
                this.pbBar.CurrentCount = data.Datas.Count;
                this.pbBar.TitleMessage = "当前页金额：" + data.Datas.Select(obj => obj.PopSellerGetMoney).Sum();
                var ordervms = data.Datas.Select(new Func<Order, OrderViewModel>(obj => new OrderViewModel(obj))).ToArray();
                this.lstItems.ItemsSource = ordervms;
                foreach (var item in ordervms)
                {
                    if (item.Source.Type == OrderType.SHUA)
                    {
                        item.Background = Brushes.Yellow;
                    }
                    else
                    {
                        item.Background = new SolidColorBrush(new Color { ScA = 1, ScR = 0xDB, ScG = 0xEA, ScB = 0xF9 });
                    }
                }
                if (ordervms.Length > 0)
                {
                    this.lstItems.ScrollIntoView(ordervms[0]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private void btnDetail_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrderDetailWindow window = new OrderDetailWindow { OrderVM = GetCurrentOrderViewModel(sender) };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDeliveryNumber_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = GetCurrentOrderViewModel(sender);
                if (string.IsNullOrWhiteSpace(order.Source.DeliveryCompany) ||
                    string.IsNullOrWhiteSpace(order.Source.DeliveryNumber))
                {
                    return;
                }
                Delivery.DeliveryQueryWindow window =
                    new Delivery.DeliveryQueryWindow
                    {
                        DeliveryCompany = order.Source.DeliveryCompany,
                        DeliveryNumber = order.Source.DeliveryNumber
                    };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCloseOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = GetCurrentOrderViewModel(sender);
                if ((int)order.Source.State > (int)OrderState.SHIPPED)
                {
                    MessageBox.Show("已经发货的订单不能关闭");
                    return;
                }
                OrderCloseWindow win = new OrderCloseWindow { Order = order.Source };
                if (win.ShowDialog().Value == true)
                    this.RefreshItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnModifyDeliveryInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement btn = sender as FrameworkElement;
                if (btn == null)
                {
                    MessageBox.Show("事件源对不是 btnSetPrice_Click FrameworkElement");
                    return;
                }
                var order = btn.Tag as Order;
                if (order == null)
                {
                    throw new Exception("绑定信息不是Order对象");
                }
                OrderModifyDeliveryInfoWindow win = new OrderModifyDeliveryInfoWindow { DeliveryCompany = order.DeliveryCompany, DeliveryNumber = order.DeliveryNumber };
                if (win.ShowDialog().Value == true)
                {
                    this.orderService.UpdateDelivery(order.Id, 0, win.DeliveryCompany, win.DeliveryNumber, DateTime.Now);
                    order.DeliveryCompany = win.DeliveryCompany;
                    order.DeliveryNumber = win.DeliveryNumber;
                    MessageBox.Show("更新成功");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void GoodsImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                OrderGoods og = fe.Tag as OrderGoods;

                if (og == null)
                {
                    throw new Exception("数据绑定信息不正确");
                }

                string url = "";
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (string.IsNullOrWhiteSpace(og.PopUrl))
                    {
                        throw new Exception("商品网店地址为空");
                    }
                    var o = ServiceContainer.GetService<OrderService>().GetById(og.OrderId);
                    if (o.PopType == PopType.TAOBAO || o.PopType == PopType.TMALL)
                    {
                        url = "http://item.taobao.com/item.htm?id=" + og.PopUrl;
                    }
                    else if (o.PopType == PopType.CHUCHUJIE)
                    {
                        url = "http://wx.chuchujie.com/index.php?s=/WebProduct/product_detail/product_id/" + og.PopUrl;
                    }
                    else if (o.PopType == PopType.PINGDUODUO)
                    {
                        url = "http://mobile.yangkeduo.com/goods.html?goods_id=" + og.PopUrl;
                    }
                    else
                    {
                        throw new Exception("无法识别的平台");
                    }
                }
                else
                {
                    long numberId = og.NumberId;
                    if (numberId <= 0)
                    {
                        return;
                    }

                    var gu = ServiceContainer.GetService<GoodsService>().GetById(numberId);
                    if (gu == null)
                    {
                        throw new Exception("指定的商品不存在");
                    }
                    url = gu.Url;

                    if (string.IsNullOrWhiteSpace(url))
                    {
                        return;
                    }
                }

                Process.Start(url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCopyStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不正确");
                    return;
                }
                var orderGoods = fe.Tag as OrderGoods;
                if (orderGoods == null)
                {
                    MessageBox.Show("Tag对象不为OrderGoods");
                    return;
                }
                Clipboard.SetText(orderGoods.Vendor + " " + orderGoods.Edtion + " " + orderGoods.Number + " " + orderGoods.Color + " " + orderGoods.Size + " " + orderGoods.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbDeliveryNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            try
            {
                this.btnSearch_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                e.Handled = true;
                this.Dispatcher.BeginInvoke(new Action(() => this.tbDeliveryNumber.SelectAll()));
            }
        }

        private void btnModifyStock_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不正确");
                    return;
                }

                var orderGoods = fe.Tag as OrderGoods;
                if (orderGoods == null)
                {
                    MessageBox.Show("Tag对象不为OrderGoods");
                    return;
                }

                if ((int)orderGoods.State >= (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单状态不正确");
                }

                OrderGoodsStockModifyWindow win = new OrderGoodsStockModifyWindow { OrderGoods = orderGoods };
                if (win.ShowDialog().Value == true)
                    this.RefreshItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RefreshItems()
        {
            var items = this.lstItems.ItemsSource;
            this.lstItems.ItemsSource = null;
            this.lstItems.ItemsSource = items;
        }

        private void btnCopyDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    throw new Exception("事件源不是 FrameworkElement");
                }

                string number = fe.Tag as string;
                if (string.IsNullOrWhiteSpace(number))
                {
                    throw new Exception("快递单号为空");
                }
                //SetText方法有时会抛出异常
                //Clipboard.SetText(number);
                Clipboard.SetDataObject(number);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSpilteOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrderViewModel vm = GetCurrentOrderViewModel(sender);
                if ((int)vm.Source.State > (int)OrderState.SHIPPED)
                {
                    throw new Exception("订单状态不正确");
                }
                OrderSpilteWindow window = new OrderSpilteWindow { Order = vm.Source };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnModifyComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = GetCurrentOrderViewModel(sender);
                var edtion = new OrderCommentAndFlagEditWindow { Flag = order.OrderFlag, Comment = order.PopSellerComment };
                if (edtion.ShowDialog().Value == false)
                {
                    return;
                }
                Shop s = ServiceContainer.GetService<ShopService>().GetById(order.Source.ShopId);
                ServiceContainer.GetService<OrderService>().ModifyPopSellerComment(order.Source.Id, edtion.Flag, edtion.Comment.Trim());
                order.Source.PopSellerComment = edtion.Comment.Trim();
                order.Source.PopFlag = edtion.Flag;
                order.PopSellerComment = edtion.Comment.Trim();
                order.OrderFlag = edtion.Flag;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("是否删除当前订单?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }

                OrderViewModel vm = GetCurrentOrderViewModel(sender);
                ServiceContainer.GetService<OrderService>().Delete(vm.Source.Id);
                MessageBox.Show("删除成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnPrintOrderGoodsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不正确");
                    return;
                }

                var orderGoods = fe.Tag as OrderGoods;
                if (orderGoods == null)
                {
                    MessageBox.Show("Tag对象不为OrderGoods");
                    return;
                }
                var goodsTemplate = LocalConfigService.GetValue(SystemNames.CONFIG_GOODS_TEMPLATE, "");
                if (string.IsNullOrWhiteSpace(goodsTemplate))
                {
                    throw new Exception("系统中没有配置商品模板不能打印");
                }
                var template = PrintTemplateService.GetAllLocal().FirstOrDefault(obj => obj.Type == PrintTemplate.TYPE_GOODS && obj.Name == goodsTemplate);
                if (template == null)
                {
                    throw new Exception("系统中配置的商品模板:" + goodsTemplate + "不存在，或者类型不对");
                }
                var gpd = new GoodsPrintDocument();
                string printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_GOODS_BARCODE, "");
                gpd.StartPrint(new OrderGoods[] { orderGoods }, printer, false, template);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void startPPCRM_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrderViewModel vm = GetCurrentOrderViewModel(sender);
                Shop s = ServiceContainer.GetService<ShopService>().GetById(vm.Source.ShopId);
                PopProgramUtil.StartPopProgram(s.PopType, s.PopSellerId, vm.Source.PopBuyerId, "");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static OrderViewModel GetCurrentOrderViewModel(object sender)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe == null)
            {
                throw new Exception("事件源不是FrameworkElement");
            }
            OrderViewModel vm = fe.Tag as OrderViewModel;
            if (vm == null)
            {
                throw new Exception("Tag 数据为空或者类型不对");
            }
            return vm;
        }

        private void btnHand_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (OperatorService.LoginOperator.Rights.Contains("创建订单") == false)
                {
                    throw new Exception("你没有权限创建订单");
                }
                new OrderEditWindow().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateReturn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不正确");
                    return;
                }

                var orderGoods = fe.Tag as OrderGoods;
                if (orderGoods == null)
                {
                    MessageBox.Show("Tag对象不为OrderGoods");
                    return;
                }

                if (orderGoods.State == OrderState.SPILTED && orderGoods.State == OrderState.CANCLED)
                {
                    throw new Exception("订单状态不正确");
                }

                var orderSample = ServiceContainer.GetService<OrderService>().GetById(orderGoods.OrderId);
                if (orderSample.PopSellerComment.Contains("差价") || orderSample.PopSellerComment.Contains("钱"))
                {
                    MessageBox.Show("盒子里面有差价请注意检查!!!!!!!!!!!!", "警告", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                OrderGoodsCreateReturnWindow win = new OrderGoodsCreateReturnWindow { OrderGoods = orderGoods };
                if (win.ShowDialog().Value == true)
                    this.RefreshItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateReturnCashButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var or = (sender as Button).Tag as OrderViewModel;
                if (string.IsNullOrWhiteSpace(or.Source.PopOrderId))
                {
                    throw new Exception("网店订单编号为空不能创建");
                }

                if (this.orderService.IsDBMinTime(or.Source.PopPayTime))
                {
                    throw new Exception("订单未付过款不能创建");
                }
                var win = new CreateReturnCashWindow { Order = or.Source };
                var ret = win.ShowDialog();
                if (ret.Value)
                {
                    var flag = or.Source.PopFlag == ColorFlag.None ? ColorFlag.RED : or.Source.PopFlag;
                    string comment = or.PopSellerComment + win.ReturnCash.Type + " " + win.ReturnCash.AccountType +
                                     " " + win.ReturnCash.AccountInfo + " " + win.ReturnCash.Money + "元【" +
                                     OperatorService.LoginOperator.Number + "】";
                    this.orderService.ModifyPopSellerComment(or.Source.Id, flag, comment);
                    or.Source.PopSellerComment = comment;
                    or.Source.PopFlag = flag;
                    or.PopSellerComment = or.Source.PopSellerComment;
                    or.OrderFlag = flag;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var or = (sender as Button).Tag as OrderViewModel;
                if (or == null)
                {
                    throw new Exception("订单对象为空");
                }

                var win = new OrderEditWindow { Order = or.Source };
                win.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}