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
using System.Windows.Shapes;
using ShopErp.App.Service.Delivery;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// Interaction logic for DeliveryQueryWindow.xaml
    /// </summary>
    public partial class DeliveryQueryWindow : Window
    {
        public string DeliveryCompany { get; set; }

        public string DeliveryNumber { get; set; }

        public DeliveryQueryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.tbInfo.Text = DeliveryCompany + "  " + DeliveryNumber;
            if (string.IsNullOrWhiteSpace(DeliveryCompany) || string.IsNullOrWhiteSpace(DeliveryNumber))
            {
                MessageBox.Show("快递公司或者快递单号不能为空");
                this.DialogResult = false;
            }
            this.QueryMessage();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.QueryMessage();
        }

        private void QueryMessage()
        {
            try
            {
                DeliveryTransation item = DeliveryService.Query(this.DeliveryCompany, this.DeliveryNumber);
                this.dgvItems.ItemsSource = item.Items;
                this.tbState.Text = item.IsSigned ? "已签收" : "配送中";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.DialogResult = false;
            }
        }
    }
}