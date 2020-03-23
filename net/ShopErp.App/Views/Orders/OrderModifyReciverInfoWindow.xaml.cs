 
 
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
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderReceiverInfoModifyWindow.xaml
    /// </summary>
    public partial class OrderModifyReciverInfoWindow : Window
    {
        public Order Order { get; set; }

        public OrderModifyReciverInfoWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.tbReceiverAddress.Text = this.Order.ReceiverAddress;
            this.tbReceiverMobile.Text = this.Order.ReceiverMobile;
            this.tbReceiverName.Text = this.Order.ReceiverName;
            this.tbReceiverPhone.Text = this.Order.ReceiverPhone;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string add = this.tbReceiverAddress.Text.Trim();
                string mobile = this.tbReceiverMobile.Text.Trim();
                string phone = this.tbReceiverPhone.Text.Trim();
                string name = this.tbReceiverName.Text.Trim();

                if (add.Equals(this.Order.ReceiverAddress.Trim()) && mobile.Equals(Order.ReceiverMobile.Trim()) &&
                    phone.Equals(Order.ReceiverPhone.Trim()) && name.Equals(Order.ReceiverName))
                {
                    throw new Exception("信息全部相同,未保存");
                }

                if (add.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries).Length < 4)
                {
                    throw new Exception("地址格式必须是四段式");
                }

                if (string.IsNullOrWhiteSpace(mobile) == false)
                {
                    if (mobile.Length != 11)
                    {
                        MessageBox.Show("手机位数必须为11位");
                    }
                }

                Order.ReceiverAddress = add;
                Order.ReceiverMobile = mobile;
                Order.ReceiverPhone = phone;
                Order.ReceiverName = name;
                ServiceContainer.GetService<OrderService>().Update(Order);
                MessageBox.Show("修改成功");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}