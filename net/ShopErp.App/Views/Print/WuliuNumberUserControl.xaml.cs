using ShopErp.App.Service.Restful;
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

namespace ShopErp.App.Views.Print
{
    /// <summary>
    /// WuliuNumberUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class WuliuNumberUserControl : UserControl
    {

        private bool myloaded = false;

        public WuliuNumberUserControl()
        {
            InitializeComponent();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string wuliuIds = this.tbOrderId.Text.Trim();
                string dc = this.cbbDeliveryCompany.Text.Trim();
                string deliveryNumber = this.tbDeliveryNumber.Text.Trim();
                DateTime start = this.dpStart.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpStart.Value.Value;
                DateTime end = this.dpEnd.Value == null ? Utils.DateTimeUtil.DbMinTime : this.dpEnd.Value.Value;
                var items = ServiceContainer.GetService<WuliuNumberService>().GetByAll(wuliuIds, dc, deliveryNumber, start, end, 0, 0).Datas;
                this.dgvItems.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.myloaded)
                {
                    return;
                }
                var ll = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
                ll.Insert(0, "");
                this.cbbDeliveryCompany.ItemsSource = ll;
                this.dpStart.Value = DateTime.Now.AddDays(-2);
                this.myloaded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
