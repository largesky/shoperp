 
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
using ShopErp.App.Service.Restful;
using ShopErp.App.Views.Extenstions;
 

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// CreateOrderReturnWithoutOrderWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OrderReturnCreateWithoutOrderWindow : Window
    {
        OrderReturnService ors = ServiceContainer.GetService<OrderReturnService>();

        public OrderReturnCreateWithoutOrderWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbDeliveryCompanys.ItemsSource = ServiceContainer.GetService<DeliveryCompanyService>().GetByAll().Datas.Select(obj => obj.Name).ToList();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string com = this.cbbDeliveryCompanys.Text.Trim();
                string devliveryNumber = this.tbDeliveryNumber.Text.Trim();
                float goodsMoney = float.Parse(this.tbGoodsMoney.Text.Trim());
                string vendor = this.tbVendor.Text.Trim();
                string number = this.tbNumber.Text.Trim();
                string edtion = this.tbEdtion.Text.Trim();
                string color = this.tbColor.Text.Trim();
                string size = this.tbSize.Text.Trim();
                int count = int.Parse(this.tbCount.Text.Trim());

                if (count < 1)
                {
                    throw new Exception("商品数量必须大于0");
                }

                if (goodsMoney < 5)
                {
                    throw new Exception("商品金额必须大于5元");
                }

                if (string.IsNullOrWhiteSpace(vendor) || string.IsNullOrWhiteSpace(number) ||
                    string.IsNullOrWhiteSpace(color) || string.IsNullOrWhiteSpace(size))
                {
                    throw new Exception("商品信息不能为空");
                }

                if (string.IsNullOrWhiteSpace(com) || string.IsNullOrWhiteSpace(devliveryNumber))
                {
                    throw new Exception("快递信息不能为空");
                }

                ors.CreateWithoutOrder( com, devliveryNumber,string.Join(" ", vendor + "," + number, edtion, color, size), goodsMoney, count);
                MessageBox.Show("创建成功");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}