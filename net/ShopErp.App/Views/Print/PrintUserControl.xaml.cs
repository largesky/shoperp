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
using System.Reflection;

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// Interaction logic for PrintUserControl.xaml
    /// </summary>
    public partial class PrintUserControl : UserControl
    {
        private OrderService orderService = ServiceContainer.GetService<OrderService>();

        private object printLock = new object();

        private System.Collections.ObjectModel.ObservableCollection<PrintOrderPageViewModel> printOrderPages = new ObservableCollection<PrintOrderPageViewModel>();

        private bool myLoaded = false;

        public PrintUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var pp in printOrderPages)
                {
                    pp.LoadBarValue();
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
                this.cbbPopPayTypes.Bind<PopPayType>();
                this.cbbPopPayTypes.SetSelectedEnum(PopPayType.ONLINE);
                var shippers = ServiceContainer.GetService<GoodsService>().GetAllShippers().Datas;
                shippers.Insert(0, "");
                this.cbbShippers.ItemsSource = shippers;
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

            var groups = vms.GroupBy(obj => obj.Source.DeliveryCompany).ToArray();
            this.printOrderPages.Clear();
            foreach (var g in groups)
            {
                var p = new PrintOrderPageViewModel(g.ToArray(), this.cbbShippers.Text.Trim());
                this.printOrderPages.Add(p);
                p.LoadBarValue();
            }
        }

        private void btnDownloadOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectFlags = this.GetSelectedOrderFlags();
                var payType = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>();

                if (payType == PopPayType.None)
                {
                    MessageBox.Show("选择支付类型");
                    return;
                }

                var downloadOrders = OrderDownloadWindow.DownloadOrder(payType, this.cbbShippers.Text.Trim());
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
                var payType = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>();
                if (payType == PopPayType.None)
                {
                    MessageBox.Show("选择支付类型");
                    return;
                }
                var orders = this.orderService.GetByAll("", "", "", "", DateTime.Now.AddDays(-90), Utils.DateTimeUtil.DbMinTime, "", "", OrderState.RETURNING, payType, "", "", "", selectFlags.ToArray(), -1, "", 0, OrderCreateType.NONE, OrderType.NONE, this.cbbShippers.Text.Trim(), 0, 0).Datas.ToArray();
                List<Order> os = new List<Order>();
                foreach (var o in orders)
                {
                    if (string.IsNullOrWhiteSpace(o.DeliveryNumber) && DateTimeUtil.IsDbMinTime(o.DeliveryTime))
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

                if (printOrderPage.IsRunning)
                {
                    if (MessageBox.Show("是否停止打印", "提示", MessageBoxButton.YesNo, MessageBoxImage.Stop) == MessageBoxResult.Yes)
                    {
                        printOrderPage.Stop();
                        return;
                    }
                }

                var orders = printOrderPage.OrderViewModels.ToArray();
                var selectedOrders = orders.Where(obj => obj.IsChecked).Select(obj => obj.Source).ToArray();
                var printTemplate = printOrderPage.WuliuPrintTemplate;
                Grid grid = ((sender as Button).Parent as StackPanel).Parent as Grid;
                DataGrid dg = grid.FindName("dgOrders") as DataGrid;
                DataGridColumn goodsCol = dg.Columns.FirstOrDefault(col => col.Header != null && col.Header.ToString() == "门牌编号");

                if (printOrderPage.WuliuBranch == null)
                {
                    throw new Exception("未选择发货网点");
                }

                if (printTemplate == null)
                {
                    throw new Exception("请选择相应的快递模板");
                }

                if (selectedOrders.Count() < 1)
                {
                    throw new Exception("没有选择需要打印的订单");
                }

                if (selectedOrders.Select(obj => obj.PopPayType).Distinct().Count() != 1)
                {
                    throw new Exception("不同支付类型的订单不能一起打印");
                }

                if (goodsCol == null)
                {
                    throw new Exception("无法找到列标题为: 门牌编号 的列");
                }

                if (printTemplate.SourceType == WuliuPrintTemplateSourceType.PINDUODUO && selectedOrders.Any(obj => obj.PopType != PopType.PINGDUODUO))
                {
                    throw new Exception("拼多多电子面单只能打印拼多多的订单");
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
                string printer = printOrderPage.Printer;
                if (string.IsNullOrWhiteSpace(printer))
                {
                    throw new Exception("没有选择打印机");
                }
                string popMessage = string.Format("发货网点：{0}{1}打印模板：{2}{3}打印设备：{4}{5}打印数据：{6}", printOrderPage.WuliuBranch.Name, Environment.NewLine, printTemplate.Name, Environment.NewLine, printer, Environment.NewLine, selectedOrders.Count());
                if (MessageBox.Show(popMessage, "确认打印", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
                //按照厂家门牌号，对数据进行排序
                goodsCol.SortDirection = null;
                this.SortData(printOrderPage, dg, goodsCol);
                WPFHelper.DoEvents();
                printOrderPage.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private object GetData(PrintOrderViewModel source)
        {
            return null;
        }


        private void SortData(PrintOrderPageViewModel p, DataGrid dg, DataGridColumn col)
        {
            if (dg == null)
            {
                MessageBox.Show("事件源不是DataGrid无法排序");
                return;
            }
            string sortPath = col.SortMemberPath;
            if (p.OrderViewModels.Count < 1)
            {
                return;
            }
            var sortType = col.SortDirection == null ? ListSortDirection.Ascending : (col.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
            List<PrintOrderViewModel> newVms = null;
            if (sortPath.Contains("DoorNumber"))
            {
                //两次排序，第一次根据门牌号排，然后根据货号排序,然后根据付款时间 类型PrintOrderViewModel实际了比较接口
                List<PrintOrderViewModel> tmpList = p.OrderViewModels.ToList();
                tmpList.Sort();
                tmpList.Sort();
                tmpList.Sort();
                newVms = tmpList;
            }
            else
            {
                EnumerableKeySelector selector = new EnumerableKeySelector(p.OrderViewModels[0].GetType(), sortPath);
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = p.OrderViewModels.OrderBy(obj => selector.GetData(obj)).ToList();
                }
                else
                {
                    newVms = p.OrderViewModels.OrderByDescending(obj => selector.GetData(obj)).ToList();
                }
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


        private void SelectGoods(object sender, Func<OrderGoods, OrderGoods, bool> predicate)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as ObservableCollection<PrintOrderViewModel>;
                if (item.Source.OrderGoodss == null || item.Source.OrderGoodss.Count > 1)
                {
                    MessageBox.Show("空订单不能选择");
                }
                foreach (var or in orders)
                {
                    if (or.Source.OrderGoodss == null || or.Source.OrderGoodss.Count < 1)
                    {
                        continue;
                    }
                    or.IsChecked = or.Source.OrderGoodss.Any(o => item.Source.OrderGoodss.Any(obj => predicate(o, obj)));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SelectOrder(object sender, Func<Order, Order, bool> predicate)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as ObservableCollection<PrintOrderViewModel>;
                if (item.Source.OrderGoodss == null || item.Source.OrderGoodss.Count > 1)
                {
                    MessageBox.Show("空订单不能选择");
                }
                foreach (var or in orders)
                {
                    if (or.Source.OrderGoodss == null || or.Source.OrderGoodss.Count < 1)
                    {
                        continue;
                    }
                    or.IsChecked = predicate(item.Source, or.Source);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MiSelectSameVendor_Click(object sender, RoutedEventArgs e)
        {
            SelectGoods(sender, (o1, o2) => o1.Vendor == o2.Vendor);
        }

        private void MiSelectSameGoodsById_Click(object sender, RoutedEventArgs e)
        {
            SelectGoods(sender, (o1, o2) => o1.GoodsId == o2.GoodsId);
        }

        private void MiSelectSameShop_Click(object sender, RoutedEventArgs e)
        {
            SelectOrder(sender, (o1, o2) => o1.ShopId == o2.ShopId);
        }

        private void MiSelectSamePop_Click(object sender, RoutedEventArgs e)
        {
            SelectOrder(sender, (o1, o2) => o1.PopType == o2.PopType);
        }

        private void MiSelectSameGoodsByIdAndColorSize_Click(object sender, RoutedEventArgs e)
        {
            SelectGoods(sender, (o1, o2) => o1.GoodsId == o2.GoodsId && o1.Edtion == o1.Edtion && o1.Color == o2.Color && o1.Size == o2.Size);
        }

        private void miEditOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetMIOrder(sender);
                var w = new Orders.OrderModifyReciverInfoWindow { Order = item.Source };
                bool? ret = w.ShowDialog();
                if (ret.Value)
                {
                    item.ReceiverAddress = item.Source.ReceiverAddress;
                    item.ReceiverMobile = item.Source.ReceiverMobile;
                    item.ReceiverName = item.Source.ReceiverName;
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
            var v = this.cbbPopPayTypes.GetSelectedEnum<PopPayType>();
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

        private void btnUpdateAddressArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resp = ServiceContainer.GetService<WuliuNumberService>().UpdateAddressArea();
                MessageBox.Show("更新成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


    }
}