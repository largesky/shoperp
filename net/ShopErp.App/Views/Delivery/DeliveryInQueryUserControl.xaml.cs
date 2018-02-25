using System;
using System.Collections.Generic;
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
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// ArriveInQueryUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryInQueryUserControl : UserControl
    {
        private DeliveryInService deliveryInService = ServiceContainer.GetService<DeliveryInService>();
        private bool myLoaded = false;

        public DeliveryInQueryUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (myLoaded)
            {
                return;
            }
            var com = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj=>obj.Name).ToList();
            com.Insert(0, "");
            this.cbbDeliveryCompany.ItemsSource = com;
            myLoaded = true;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.pgBar.Parameters.Clear();
                this.pgBar.Parameters.Add("deliveryCompany", this.cbbDeliveryCompany.Text);
                this.pgBar.Parameters.Add("deliveryNumber", this.tbDeliveryNumber.Text.Trim());
                this.pgBar.Parameters.Add("startTime",
                    this.tbStart.Value == null ? DateTime.MinValue : this.tbStart.Value.Value);
                this.pgBar.Parameters.Add("endTime",
                    this.tbEnd.Value == null ? DateTime.MinValue : this.tbEnd.Value.Value);
                this.pgBar.StartPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pgBar_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            var data = this.deliveryInService.GetByAll(e.GetParameter<string>("deliveryCompany"), e.GetParameter<string>("deliveryNumber"),
                e.GetParameter<DateTime>("startTime"), e.GetParameter<DateTime>("endTime"), e.CurrentPage - 1,
                e.PageSize);
            this.pgBar.Total = data.Total;
            this.pgBar.CurrentCount = data.Datas.Count;
            this.dgvItems.ItemsSource = data.Datas;
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var item in this.dgvItems.SelectedCells.Select(obj => obj.Item).Distinct().ToArray())
                {
                    ServiceContainer.GetService<DeliveryInService>().Update(item as DeliveryIn);
                }
                MessageBox.Show("已成功更新");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}