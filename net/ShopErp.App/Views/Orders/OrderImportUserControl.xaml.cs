using Microsoft.Win32;
using ShopErp.App.Service.Restful;
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
    /// OrderImportUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderImportUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<OrderViewModel> orderViewModels = new System.Collections.ObjectModel.ObservableCollection<OrderViewModel>();

        private OpenFileDialog ofd = new OpenFileDialog();

        public OrderImportUserControl()
        {
            InitializeComponent();
        }

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.orderViewModels.Clear();
                this.dgOrders.ItemsSource = this.orderViewModels;

                if (ofd.ShowDialog().Value == false)
                {
                    return;
                }
                this.tbFile.Text = ofd.FileName;
                List<string> notQuery = new List<string>();
                var excel = Service.Excel.ExcelFile.Open(ofd.FileName);
                var datas = excel.ReadFirstSheet();
                int recivierIndex = Service.Excel.ExcelFile.GetIndex(datas[0], "收货", false);
                int numberIndex = Service.Excel.ExcelFile.GetIndex(datas[0], "单号", false);
                int deliveryNameIndex = Service.Excel.ExcelFile.GetIndex(datas[0], "快递名称", false);
                for (int i = 1; i < datas.Length; i++)
                {
                    string[] rrs = datas[i][recivierIndex].Split(new char[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries);
                    var os = ServiceContainer.GetService<OrderService>().GetByAll("", "", rrs[1], rrs[0], "", 0, DateTime.Now.AddDays(-10), DateTime.Now.AddDays(1), "", "", ShopErp.Domain.OrderState.PAYED, ShopErp.Domain.PopPayType.None, "", "", null, -1, "", 0, ShopErp.Domain.OrderCreateType.NONE, ShopErp.Domain.OrderType.NONE, 0, 0).Datas;
                    foreach (var o in os)
                    {
                        o.DeliveryNumber = datas[i][numberIndex].Trim();
                        o.DeliveryCompany = datas[i][deliveryNameIndex].Trim();
                        this.orderViewModels.Add(new OrderViewModel(o));
                    }
                    if (os.Count < 1)
                    {
                        notQuery.Add(datas[i][numberIndex].Trim());
                    }
                }
                this.tbDeliveryNumberFail.Text = string.Join(",", notQuery);
                string msg = string.Format("查询完成，共输入订单数：{0},查询到订单数：{1}，未查询到订单列表：{2}", datas.Length - 1, this.orderViewModels.Count, notQuery.Count);
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.orderViewModels.Count < 1)
                {
                    throw new Exception("没有订单数据");
                }

                foreach (var ov in this.orderViewModels)
                {
                    ServiceContainer.GetService<OrderService>().UpdateDelivery(ov.Source.Id, 0, ov.Source.DeliveryCompany, ov.Source.DeliveryNumber, DateTime.Now);
                }
                MessageBox.Show("更新完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
