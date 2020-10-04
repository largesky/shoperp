using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.App.ViewModels;
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
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;
using System.ComponentModel;
using ShopErp.App.Service;
using System.IO;
using ShopErp.App.Service.Excel;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderExportUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderExportUserControl : UserControl
    {
        private bool myLoaded = false;

        public OrderExportUserControl()
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
                this.cbbOrderTypes.Bind<OrderType>();
                var shippers = ServiceContainer.GetService<GoodsService>().GetAllShippers().Datas;
                shippers.Insert(0, "");
                this.cbbShippers.ItemsSource = shippers;
                this.myLoaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var shopids = ServiceContainer.GetService<ShopService>().GetByAll().Datas.Where(obj => obj.Enabled).Select(obj => obj.Id).ToArray();
                if (shopids.Length < 1)
                {
                    MessageBox.Show("系统中没有任何店铺");
                    return;
                }
                var dcs = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Where(obj => obj.PaperMark);
                var orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(shopids, ShopErp.Domain.OrderCreateType.NONE, ShopErp.Domain.PopPayType.None, this.cbbShippers.Text.Trim(), 0, 0).Datas.OrderBy(obj => obj.PopPayTime).Select(obj => new OrderViewModel(obj)).ToArray();
                var selType = this.cbbOrderTypes.GetSelectedEnum<OrderType>();
                if (selType != OrderType.NONE)
                {
                    orders = orders.Where(obj => obj.Source.Type == selType).ToArray();
                }
                foreach (var order in orders)
                {
                    order.IsChecked = order.Source.Type == ShopErp.Domain.OrderType.SHUA ? false : string.IsNullOrWhiteSpace(order.DeliveryCompany) || dcs.Any(obj => obj.PaperMark && obj.Name == order.DeliveryCompany);
                }
                this.dgOrders.ItemsSource = orders;
                List<string> failPhones = new List<string>();
                var group = orders.GroupBy(obj => obj.Source.ReceiverMobile);
                foreach (var v in group)
                {
                    if (v.Select(obj => obj.Source.Type).Distinct().Count() != 1)
                    {
                        failPhones.Add(v.Key);
                    }
                }
                if (failPhones.Count > 0)
                {
                    string msg = "检查订单失败，有相同电话信息但订单类型不一致：" + Environment.NewLine + string.Join(",", failPhones) + Environment.NewLine;
                    MessageBox.Show(msg, "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnExportCd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orders = this.GetSelectedOrdersAndMerge();
                if (orders == null || orders.Length < 1)
                {
                    return;
                }

                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "xlsx";
                sfd.Filter = "*.xlsx|Office 2007 文件";
                sfd.FileName = "贾勇 " + DateTime.Now.ToString("MM-dd") + ".xlsx";
                sfd.InitialDirectory = LocalConfigService.GetValue("OrderExportSaveDir_" + this.cbbShippers.Text.Trim(), "");
                if (sfd.ShowDialog().Value == false)
                {
                    return;
                }

                double shippMoney = LocalConfigService.GetValueDouble(SystemNames.CONFIG_SHIPP_MONEY, 2.5);
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                var allGoods = ServiceContainer.GetService<GoodsService>().GetByAll(0, GoodsState.NONE, 0, DateTimeUtil.DbMinTime, DateTimeUtil.DbMinTime, "", "", GoodsType.GOODS_SHOES_NONE, "", ColorFlag.None, GoodsVideoType.NONE, "", "", "", 0, 0).Datas;
                List<string[]> contents = new List<string[]>();
                foreach (var order in orders)
                {
                    var ogs = OrderService.FilterOrderGoodsCanbeSend(order);
                    int goodsCount = OrderService.CountGoodsCanbeSend(order);
                    int goodsMoney = 0;
                    foreach (var og in ogs.Where(obj => obj.GoodsId > 0))
                    {
                        var g = allGoods.FirstOrDefault(obj => obj.Id == og.GoodsId);
                        if (g != null)
                        {
                            goodsMoney += (int)g.Price + ((og.Edtion.Contains("加毛") || og.Edtion.Contains("厚毛") || og.Edtion.Contains("绒里")) ? g.JiamaoAddPrice : 0);
                        }
                    }
                    string[] ss = new string[] { shops.FirstOrDefault(obj => obj.Id == order.ShopId).Mark, DateTimeUtil.FormatDateTime(order.PopPayTime), OrderService.FormatGoodsInfoCanbeSend(order), order.DeliveryNumber, order.PopSellerComment, order.ReceiverName, order.ReceiverMobile, goodsMoney.ToString(), (shippMoney * goodsCount).ToString("F1") };
                    if (order.PopType != ShopErp.Domain.PopType.TMALL)
                    {
                        ss[4] += "放拼多多好评卡";
                    }
                    contents.Add(ss);
                }
                var columns = new ExcelColumn[] { new ExcelColumn("店铺", false), new ExcelColumn("付款时间", false), new ExcelColumn("商品信息", false), new ExcelColumn("快递单号", false), new ExcelColumn("备注", false), new ExcelColumn("姓名", false), new ExcelColumn("手机", false), new ExcelColumn("商品价格", true), new ExcelColumn("发货费", true) };
                new ExcelFile(sfd.FileName, "订单", columns, contents.ToArray()).WriteXlsx();
                LocalConfigService.UpdateValue("OrderExportSaveDir_" + this.cbbShippers.Text.Trim(), new FileInfo(sfd.FileName).DirectoryName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnCopyInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orders = this.GetSelectedOrdersAndMerge();
                if (orders == null || orders.Length < 1)
                {
                    return;
                }
                List<string> contents = new List<string>();
                //合并订单
                foreach (var order in orders)
                {
                    string[] ss = new string[] { OrderService.FormatGoodsInfoCanbeSend(order), order.ReceiverName, string.Join(",", order.ReceiverMobile, order.ReceiverPhone), order.ReceiverAddress };
                    contents.Add(string.Join(" ", ss));
                }
                System.Windows.Forms.Clipboard.SetText(string.Join(Environment.NewLine, contents));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnExportK3_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                throw new Exception("导出K3方法没有实现");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCopyAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = this.dgOrders.ItemsSource as OrderViewModel[];
                if (items == null || items.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }
                items = items.Where(obj => obj.IsChecked).ToArray();
                if (items == null || items.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }
                List<string> re = new List<string>();
                foreach (var v in items)
                {
                    re.Add(v.Source.ReceiverName + "," + v.Source.ReceiverMobile + "," + v.Source.ReceiverAddress.Replace(",", "").Replace("，", "") + ",");
                }
                var res = re.Distinct().ToArray();
                System.Windows.Forms.Clipboard.SetText(string.Join(Environment.NewLine, res));
                string msg = string.Format("订单总数：{0},合并后共复制收货人信息条数：{1}", items.Count(), res.Length);
                MessageBox.Show(msg, "复制成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Order[] GetSelectedOrdersAndMerge()
        {
            Order[] selectedOrders = (this.dgOrders.ItemsSource as OrderViewModel[] == null) ? null : (this.dgOrders.ItemsSource as OrderViewModel[]).Where(obj => obj.IsChecked).Select(obj => obj.Source).ToArray();
            if (selectedOrders == null || selectedOrders.Length < 1)
            {
                MessageBox.Show("没有数据");
                return null;
            }
            return OrderService.MergeOrders(selectedOrders);
        }

        private OrderViewModel GetCurrentSelectedItem(object sender)
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
                var item = this.GetCurrentSelectedItem(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (this.dgOrders.ItemsSource as OrderViewModel[]).ToList();
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
                var item = this.GetCurrentSelectedItem(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (this.dgOrders.ItemsSource as OrderViewModel[]).ToList();
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

        private void miEditOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.GetCurrentSelectedItem(sender);
                var w = new Orders.OrderModifyReciverInfoWindow { Order = item.Source };
                bool? ret = w.ShowDialog();
                if (ret.Value)
                {

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void dgOrders_Sorting(object sender, DataGridSortingEventArgs e)
        {
            try
            {
                string sortPath = e.Column.SortMemberPath;
                var items = this.dgOrders.ItemsSource as OrderViewModel[];
                if (items == null || items.Length < 1)
                {
                    return;
                }
                var sortType = e.Column.SortDirection == null ? ListSortDirection.Ascending : (e.Column.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
                List<OrderViewModel> newVms = null;

                EnumerableKeySelector selector = new EnumerableKeySelector(items[0].GetType(), sortPath);
                if (sortType == ListSortDirection.Ascending)
                {
                    newVms = items.OrderBy(obj => selector.GetData(obj)).ToList();
                }
                else
                {
                    newVms = items.OrderByDescending(obj => selector.GetData(obj)).ToList();
                }
                this.dgOrders.ItemsSource = newVms.ToArray();
                e.Column.SortDirection = sortType;
                e.Handled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = this.dgOrders.ItemsSource as OrderViewModel[];
                if (items == null || items.Length < 1)
                {
                    return;
                }
                bool isChecked = ((CheckBox)sender).IsChecked.Value;
                foreach (var item in items)
                {
                    item.IsChecked = isChecked;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SelectGoods(object sender, Func<OrderGoods, OrderGoods, bool> predicate)
        {
            try
            {
                var item = this.GetCurrentSelectedItem(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as OrderViewModel[];
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
                var item = this.GetCurrentSelectedItem(sender);
                MenuItem mi = sender as MenuItem;
                var orders = (((ContextMenu)mi.Parent).PlacementTarget as DataGrid).ItemsSource as OrderViewModel[];
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

    }
}
