using ShopErp.App.ViewModels;
using ShopErp.App.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.Utils;
using ShopErp.App.Views.Orders;
using ShopErp.Domain;
using ShopErp.App.Service.Print;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for PrintUserControl.xaml
    /// </summary>
    public partial class PrintUserControl : UserControl
    {
        public static readonly DependencyProperty DeliveryPrintTemplatesProperty = DependencyProperty.Register("DeliveryPrintTemplates", typeof(ObservableCollection<Service.Print.PrintTemplate>), typeof(PrintUserControl));

        private OrderService orderService = ServiceContainer.GetService<OrderService>();
        private object printLock = new object();

        public System.Collections.ObjectModel.ObservableCollection<Service.Print.PrintTemplate> DeliveryPrintTemplates
        {
            get { return (ObservableCollection<Service.Print.PrintTemplate>)this.GetValue(DeliveryPrintTemplatesProperty); }
            set { this.SetValue(DeliveryPrintTemplatesProperty, value); }
        }

        private System.Collections.ObjectModel.ObservableCollection<PrintOrderPageViewModel> printOrderPages =
            new ObservableCollection<PrintOrderPageViewModel>();

        private bool myLoaded = false;

        public PrintUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //快递公司
                if (DeliveryPrintTemplates == null)
                {
                    DeliveryPrintTemplates = new System.Collections.ObjectModel.ObservableCollection<Service.Print.PrintTemplate>();
                }
                DeliveryPrintTemplates.Clear();
                foreach (var item in FilePrintTemplateRepertory.GetAllN().Where(obj => obj.Type == Service.Print.PrintTemplate.TYPE_DELIVER))
                {
                    DeliveryPrintTemplates.Add(item);
                }

                if (this.myLoaded == true)
                {
                    return;
                }

                //旗帜
                var flags = new ColorFlag[]
                {
                    ColorFlag.UN_LABEL, ColorFlag.RED, ColorFlag.YELLOW, ColorFlag.GREEN, ColorFlag.BLUE, ColorFlag.PINK
                };
                var flagVms = flags.Select(obj => new OrderFlagViewModel(false, obj)).ToArray();
                this.cbbFlags.ItemsSource = flagVms;
                //支付类型
                var payTypes = EnumUtil.GetEnumDescriptions<PopPayType>().ToList();
                payTypes.RemoveAt(0);
                this.cbbPopPayTypes.ItemsSource = payTypes;
                this.cbbPopPayTypes.SelectedIndex = 0;
                this.tc1.ItemsSource = printOrderPages;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载出错:" + ex.Message);
                MessageBox.Show(ex.StackTrace);
            }
        }

        #region 待打印订单获取

        private List<ColorFlag> GetSelectedOrderFlags()
        {
            var ovms = (this.cbbFlags.ItemsSource as OrderFlagViewModel[]).Where(obj => obj.IsChecked).Select(obj => obj.Flag).ToList();
            return ovms;
        }

        private void GenTab(Order[] orders)
        {
            PrintOrderViewModel[] vms = new PrintOrderViewModel[orders.Length];
            for (int i = 0; i < orders.Length; i++)
            {
                var vm = new PrintOrderViewModel(orders[i], (i % 2 == 0) ? PrintOrderViewModel.DEFAULTBACKGROUND_LIGHTGREEN : PrintOrderViewModel.DEFAULTBACKGROUND_LIGHTPINK);
                vms[i] = vm;
            }
            this.tbTotalCount.Text = "当前订单总数: " + orders.Length;
            CheckOrder(vms);

            var groups = vms.GroupBy(obj => obj.DeliveryCompany).ToArray();
            this.printOrderPages.Clear();
            foreach (var g in groups)
            {
                var p = new PrintOrderPageViewModel(g.ToArray());
                this.printOrderPages.Add(p);
            }
        }

        private void CheckOrder(PrintOrderViewModel[] ovs)
        {
            bool notify = false;
            foreach (var ov in ovs)
            {
                ov.State = "";
                if (ov.Source.ParseResult == false)
                {
                    ov.Background = Brushes.Red;
                    notify = true;
                    ov.State = "商品解析失败";
                }
            }

            if (notify)
            {
                MessageBox.Show("有异常订单修正后打印");
            }
        }

        private void btnDownloadOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectFlags = this.GetSelectedOrderFlags();
                var payType = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>(this.cbbPopPayTypes.SelectedIndex + 1);
                var downloadOrders = OrderDownloadWindow.DownloadOrder(payType);
                if (downloadOrders == null || downloadOrders.Count < 1)
                {
                    this.printOrderPages.Clear();
                    return;
                }
                //过滤需要打印的订单
                Order[] orders = downloadOrders.Where(obj => obj.State == OrderState.PAYED && selectFlags.Contains(obj.PopFlag)).ToArray();
                if (this.dpEnd.Value != null)
                {
                    orders = orders.Where(obj => obj.PopPayTime < this.dpEnd.Value.Value).ToArray();
                }
                this.GenTab(orders);
            }
            catch (Exception ex)
            {
                MessageBox.Show("下载订单出错:" + ex.Message + ex.GetType().FullName);
                MessageBox.Show(ex.StackTrace);
            }
        }

        private void btnGetRefundOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectFlags = this.GetSelectedOrderFlags();
                var payType = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>(this.cbbPopPayTypes.SelectedIndex + 1);
                var orders = this.orderService.GetByAll("", "", "", "", "", 0, DateTime.Now.AddDays(-30), DateTime.MinValue, "", "", OrderState.RETURNING, payType, "", "", selectFlags.ToArray(), -1, "", 0, OrderCreateType.NONE, OrderType.NONE, 0, 0).Datas.ToArray();
                List<Order> os = new List<Order>();
                foreach (var o in orders)
                {
                    if (string.IsNullOrWhiteSpace(o.DeliveryNumber) && this.orderService.IsDBMinTime(o.DeliveryTime))
                    {
                        os.Add(o);
                        o.State = OrderState.PAYED;
                        foreach (var og in o.OrderGoodss)
                        {
                            og.State = OrderState.PAYED;
                        }
                    }
                }
                this.GenTab(os.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 打印

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var printOrderPage = (sender as Button).DataContext as PrintOrderPageViewModel;
                var orders = printOrderPage.OrderViewModels.ToArray();
                var selectedOrders = orders.Where(obj => obj.IsChecked).Select(obj => obj.Source).ToArray();
                var printTemplate = printOrderPage.PrintTemplate;
                Grid grid = ((sender as Button).Parent as StackPanel).Parent as Grid;
                DataGrid dg = grid.FindName("dgOrders") as DataGrid;
                DataGridColumn goodsCol = dg.Columns.FirstOrDefault(col => col.Header.ToString() == "门牌编号");

                if (selectedOrders.Count() < 1)
                {
                    throw new Exception("没有选择需要打印的订单");
                }
                if (selectedOrders.Where(obj => obj.PopType == PopType.TAOBAO || obj.PopType == PopType.TMALL).Any(obj => obj.ReceiverName.Contains("*") || (obj.ReceiverMobile != null && obj.ReceiverMobile.Contains("**")) || (obj.ReceiverPhone != null && obj.ReceiverPhone.Contains("**"))))
                {
                    throw new Exception("淘宝天猫有订单收货人信息处理于模糊状态");
                }
                if (selectedOrders.Select(obj => obj.PopPayType).Distinct().Count() != 1)
                {
                    throw new Exception("不同支付类型的订单不能一起打印");
                }
                if (printTemplate == null)
                {
                    throw new Exception("请选择相应的快递模板");
                }

                if (goodsCol == null)
                {
                    throw new Exception("无法找到列标题为: 门牌编号 的列");
                }
                //检查货到付款
                if (selectedOrders[0].PopPayType == PopPayType.COD && printTemplate.Name.Contains("货到") == false)
                {
                    throw new Exception("货到付款订单必须使用货到付款模板");
                }
                if (selectedOrders[0].PopPayType == PopPayType.ONLINE && printTemplate.Name.Contains("货到"))
                {
                    throw new Exception("在线支付订单不能使用货到付款模板");
                }
                string printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, "");
                if (string.IsNullOrWhiteSpace(printer))
                {
                    throw new Exception("系统中没有配置快递单打印机");
                }
                string popMessage = string.Format("{0}打印模板:{1}{2}打印设备:{3}{4}打印数据:{5}", Environment.NewLine, printTemplate.Name, Environment.NewLine, printer, Environment.NewLine, selectedOrders.Count());
                if (MessageBox.Show(popMessage, "确认打印", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                //按照厂家门牌号，对数据进行排序
                goodsCol.SortDirection = null;
                this.SortData(printOrderPage, dg, goodsCol);
                WPFHelper.DoEvents();
                printOrderPage.Print(printer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 数据网格排序

        private void DataGrid_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                var p = (sender as DataGrid).DataContext as PrintOrderPageViewModel;
                this.SortData(p, sender as DataGrid, e.Column);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "排序失败");
            }
            finally
            {
                e.Handled = true;
            }
        }

        private void SortData(PrintOrderPageViewModel p, DataGrid dg, DataGridColumn col)
        {
            if (dg == null)
            {
                MessageBox.Show("事件源不是DataGrid无法排序");
                return;
            }
            string sortPath = col.SortMemberPath;
            if (string.IsNullOrWhiteSpace(sortPath))
            {
                MessageBox.Show("排序字段为空", "排序失败");
                return;
            }
            var sortType = col.SortDirection == null ? ListSortDirection.Ascending : (col.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
            List<PrintOrderViewModel> newVms = null;
            if (sortPath.Contains("PopType"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.PopType).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.PopType).ToList();
                }
            }
            else if (sortPath.Contains("PopPayTime"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.PopPayTime).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.PopPayTime).ToList();
                }
            }
            else if (sortPath.Equals("Source.Id", StringComparison.OrdinalIgnoreCase))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.Id).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.Id).ToList();
                }
            }
            else if (sortPath.Contains("PopBuyerId"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.PopBuyerId).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.PopBuyerId).ToList();
                }
            }
            else if (sortPath.Contains("Goods"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Goods).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Goods).ToList();
                }
            }
            else if (sortPath.Contains("State"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.State).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.State).ToList();
                }
            }
            else if (sortPath.Contains("DeliveryCompany"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.DeliveryCompany).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.DeliveryCompany).ToList();
                }
            }
            else if (sortPath.Contains("DeliveryNumber"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.DeliveryNumber).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.DeliveryNumber).ToList();
                }
            }
            else if (sortPath.Contains("ReceiverName"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.ReceiverName).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.ReceiverName).ToList();
                }
            }
            else if (sortPath.Contains("ReceiverPhone"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.ReceiverPhone).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.ReceiverPhone).ToList();
                }
            }
            else if (sortPath.Contains("ReceiverMobile"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.ReceiverMobile).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.ReceiverMobile).ToList();
                }
            }
            else if (sortPath.Contains("ReceiverAddress"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.ReceiverAddress).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.ReceiverAddress).ToList();
                }
            }
            else if (sortPath.Contains("DoorNumber"))
            {
                //两次排序，第一次根据门牌号排，然后根据货号排序,然后根据付款时间 类型PrintOrderViewModel实际了比较接口
                List<PrintOrderViewModel> tmpList = p.OrderViewModels.ToList();
                tmpList.Sort();
                tmpList.Sort();
                tmpList.Sort();
                newVms = tmpList;
            }
            else if (sortPath.Contains("ShopId"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.ShopId).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.ShopId).ToList();
                }
            }
            else if (sortPath.Contains("PopPayType"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.PopPayType).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.PopPayType).ToList();
                }
            }
            else if (sortPath.Contains("PopFlag"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.PopFlag).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.PopFlag).ToList();
                }
            }
            else if (sortPath.Contains("Source.Type"))
            {
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => obj.Source.Type).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => obj.Source.Type).ToList();
                }
            }
            else
            {
                MessageBox.Show("当前排序方式不支持,请联系添加");
                return;
            }
            p.OrderViewModels.Clear();
            for (int i = 0; i < newVms.Count; i++)
            {
                newVms[i].DefaultBackground = newVms[i].Background = (i % 2 == 0) ? PrintOrderViewModel.DEFAULTBACKGROUND_LIGHTGREEN : PrintOrderViewModel.DEFAULTBACKGROUND_LIGHTPINK;
                p.OrderViewModels.Add(newVms[i]);
            }
            col.SortDirection = sortType;
        }

        #endregion

        #region 前选 后选 编辑地址

        private PrintOrderViewModel GetMIOrder(object sender)
        {
            MenuItem mi = sender as MenuItem;
            var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
            var cells = dg.SelectedCells;
            if (cells.Count < 1)
            {
                throw new Exception("未选择数据");
            }

            var item = cells[0].Item as PrintOrderViewModel;
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
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as ObservableCollection<PrintOrderViewModel>;
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
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as ObservableCollection<PrintOrderViewModel>;
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

        private void miEditOrderAddress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                var w = new Orders.OrderReceiverInfoModifyWindow { Order = item.Source };
                bool? ret = w.ShowDialog();
                if (ret.Value)
                {
                    item.ReceiverAddress = item.Source.ReceiverAddress;
                    item.ReceiverMobile = item.Source.ReceiverMobile;
                    item.ReceiverName = item.Source.ReceiverName;
                    item.ReceiverPhone = item.Source.ReceiverPhone;
                    CheckOrder(new PrintOrderViewModel[] { item });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region 订单类型选择 自动切换旗帜颜色

        private void cbbPopPayTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var v = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>(this.cbbPopPayTypes.SelectedIndex + 1);
            var ovms = (this.cbbFlags.ItemsSource as OrderFlagViewModel[]).ToList();
            if (v == PopPayType.COD)
            {
                ovms.ForEach(obj => obj.IsChecked = false);
                ovms.Where(obj => obj.Flag == ColorFlag.GREEN || obj.Flag == ColorFlag.BLUE).ToList().ForEach(obj => obj.IsChecked = true);
            }
            else
            {
                ovms.ForEach(obj => obj.IsChecked = true);
            }
        }

        #endregion
    }
}