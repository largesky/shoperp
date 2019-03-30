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

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderExportUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderExportUserControl : UserControl
    {
        public OrderExportUserControl()
        {
            InitializeComponent();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var items = this.dgOrders.ItemsSource as OrderViewModel[];
                if (items == null || items.Length < 1)
                {
                    MessageBox.Show("没有任何数据");
                    return;
                }
                items = items.Where(obj => obj.IsChecked).ToArray();
                if (items == null || items.Length < 1)
                {
                    MessageBox.Show("没有选择任何数据");
                    return;
                }

                //进行检查，以防止有些订单没有标记到。根据收货人电话号码进行检查,先检查手机号，手机号为空的检查座机
                List<string> failPhones = new List<string>();
                failPhones.AddRange(CheckOrder(items.Where(obj => string.IsNullOrWhiteSpace(obj.Source.ReceiverMobile) == false).ToArray(), true));
                failPhones.AddRange(CheckOrder(items.Where(obj => string.IsNullOrWhiteSpace(obj.Source.ReceiverMobile)).ToArray(), false));
                if (failPhones.Count > 0)
                {
                    string msg = "检查订单失败，有相同电话信息但订单类型不一致：" + Environment.NewLine + string.Join(",", failPhones) + Environment.NewLine + "是否继续导出？";
                    if (MessageBox.Show(msg, "是否继续导出？", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }
                var shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas;
                string[] columnsHeader = new string[] { "类型", "店铺", "付款时间", "商品信息", "快递单号", "备注", "姓名", "手机", "地址" };
                Dictionary<string, string[][]> dicContents = new Dictionary<string, string[][]>();
                List<string[]> contents = new List<string[]>();
                contents.Add(columnsHeader);
                //合并订单
                var normalOrders = items.Where(obj => obj.Source.Type == ShopErp.Domain.OrderType.NORMAL).OrderBy(obj => obj.Source.PopType).ToArray();
                Dictionary<OrderViewModel, string> cc = new Dictionary<OrderViewModel, string>();
                foreach (var o in normalOrders)
                {
                    var key = cc.Keys.FirstOrDefault(obj => obj.Source.ShopId == o.Source.ShopId && obj.Source.ReceiverName == o.Source.ReceiverName && obj.Source.ReceiverPhone == o.Source.ReceiverPhone && obj.Source.ReceiverMobile == o.Source.ReceiverMobile && obj.Source.ReceiverAddress == o.Source.ReceiverAddress && (string.IsNullOrWhiteSpace(obj.Source.PopBuyerId) ? true : obj.Source.PopBuyerId == o.Source.PopBuyerId));
                    if (key == null)
                    {
                        cc[o] = o.GoodsInfo;
                    }
                    else
                    {
                        cc[key] = cc[key] + "," + Environment.NewLine + o.GoodsInfo;
                    }
                }

                foreach (var pair in cc)
                {
                    var v = pair.Key;
                    string[] ss = new string[] { EnumUtil.GetEnumValueDescription(v.Source.Type), shops.FirstOrDefault(obj => obj.Id == v.Source.ShopId).Mark, v.Source.PopPayTime.ToString("yyyy-MM-dd HH:mm:ss"), pair.Value, v.Source.DeliveryNumber, v.Source.PopSellerComment, v.Source.ReceiverName, string.IsNullOrWhiteSpace(v.Source.ReceiverPhone) ? v.Source.ReceiverMobile : v.Source.ReceiverMobile + "," + v.Source.ReceiverPhone, v.Source.ReceiverAddress };
                    if (pair.Key.Source.PopType != ShopErp.Domain.PopType.TMALL)
                    {
                        ss[5] += "不放合格证，放2元好评卡";
                    }
                    contents.Add(ss);
                }
                dicContents.Add("订单", contents.ToArray());
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "xlsx";
                sfd.Filter = "*.xlsx|Office 2007 文件";
                sfd.FileName = "贾勇 " + DateTime.Now.ToString("MM-dd") + ".xlsx";
                if (sfd.ShowDialog().Value == false)
                {
                    return;
                }
                Service.Excel.ExcelFile.WriteXlsx(sfd.FileName, dicContents);
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
                var orders = ServiceContainer.GetService<OrderService>().GetPayedAndPrintedOrders(shopids, ShopErp.Domain.OrderCreateType.NONE, ShopErp.Domain.PopPayType.None, 0, 0).Datas.OrderBy(obj => obj.PopPayTime).Select(obj => new OrderViewModel(obj)).ToArray();
                this.dgOrders.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private List<string> CheckOrder(OrderViewModel[] orders, bool isMobile)
        {
            List<string> failPhones = new List<string>();
            var group = orders.GroupBy(obj => isMobile ? obj.Source.ReceiverMobile : obj.Source.ReceiverPhone);
            foreach (var v in group)
            {
                if (v.Select(obj => obj.Source.Type).Distinct().Count() != 1)
                {
                    failPhones.Add(v.Key);
                }
            }
            return failPhones;
        }

        #region 前选 后选 编辑地址

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
                var item = this.GetMIOrder(sender);
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
                var item = this.GetMIOrder(sender);
                var w = new Orders.OrderReceiverInfoModifyWindow { Order = item.Source };
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

        #endregion


        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            Check(true);
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Check(false);
        }

        private void Check(bool isCheck)
        {
            try
            {
                var items = this.dgOrders.ItemsSource as OrderViewModel[];
                if (items == null || items.Length < 1)
                {
                    MessageBox.Show("没有数据");
                    return;
                }

                foreach (var v in items)
                {
                    v.IsChecked = isCheck;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
