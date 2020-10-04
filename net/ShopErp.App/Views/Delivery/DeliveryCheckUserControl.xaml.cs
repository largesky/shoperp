using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service.Excel;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryCheckUserControl.xaml
    /// </summary>
    public partial class DeliveryCheckUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<DeliveryCheckViewModel> orders = new ObservableCollection<DeliveryCheckViewModel>();

        private Shop[] shops = null;

        public DeliveryCheckUserControl()
        {
            InitializeComponent();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.orders.Clear();
                this.shops = ServiceContainer.GetService<ShopService>().GetByAll().Datas.ToArray();
                var os = ServiceContainer.GetService<OrderService>().GetByAll("", "", "", "", DateTime.Now.AddDays(-90), Utils.DateTimeUtil.DbMinTime, "", "", OrderState.PRINTED, PopPayType.None, "", "", "", null, -1, "", 0, OrderCreateType.NONE, OrderType.NONE, "", 0, 0).Datas;
                var orders = os.Select(obj => new DeliveryCheckViewModel(obj) { State = "" }).OrderBy(obj => obj.Source.PopPayTime).ToArray();
                if (orders.Length < 1)
                {
                    MessageBox.Show("没有任何订单");
                    return;
                }
                foreach (var o in orders)
                {
                    this.orders.Add(o);
                }
                this.dgvOrders.ItemsSource = this.orders;
                this.tbTotal.Text = "当前共 : " + orders.Length + " 条记录";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var seOrder = this.orders.Where(obj => obj.IsChecked).ToArray();
                if (seOrder.Length < 1)
                {
                    MessageBox.Show("没有选择订单");
                    return;
                }
                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "xlsx";
                sfd.Filter = "*.xlsx|Office 2007 文件";
                sfd.FileName = "胡平物流录入" + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss");

                var ret = sfd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }
                var deliveryCompanyGroup = seOrder.GroupBy(obj => obj.Source.DeliveryCompany).ToArray();
                string dir = new FileInfo(sfd.FileName).DirectoryName;
                foreach (var g in deliveryCompanyGroup)
                {
                    var gs = g.Distinct(new DeliveryCheckViewModelComparer()).ToArray();
                    var contents = gs.Select(obj => new string[] { obj.Source.DeliveryNumber, obj.Source.ReceiverName, obj.Source.ReceiverMobile, obj.Source.ReceiverAddress }).ToList();
                    var columns = new ExcelColumn[] { new ExcelColumn("快递单号", false), new ExcelColumn("姓名", false), new ExcelColumn("手机", false), new ExcelColumn("地址", false) };
                    ExcelFile excelFile = new ExcelFile(dir + "\\" + DateTime.Now.ToString("MM_dd") + g.Key + ".xlsx", "订单", columns, contents.ToArray());
                    excelFile.WriteXlsx();
                }
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #region 前选 后选 编辑地址

        private DeliveryCheckViewModel GetMIOrder(object sender)
        {
            MenuItem mi = sender as MenuItem;
            var dg = ((ContextMenu)mi.Parent).PlacementTarget as DataGrid;
            var cells = dg.SelectedCells;
            if (cells.Count < 1)
            {
                throw new Exception("未选择数据");
            }

            var item = cells[0].Item as DeliveryCheckViewModel;
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
    }

    class DeliveryCheckViewModelComparer : IEqualityComparer<DeliveryCheckViewModel>
    {
        public bool Equals(DeliveryCheckViewModel x, DeliveryCheckViewModel y)
        {
            if (x != null && y != null)
            {
                return x.Source.DeliveryNumber.Equals(y.Source.DeliveryNumber);
            }
            return false;
        }

        public int GetHashCode(DeliveryCheckViewModel obj)
        {
            return base.GetHashCode();
        }
    }
}