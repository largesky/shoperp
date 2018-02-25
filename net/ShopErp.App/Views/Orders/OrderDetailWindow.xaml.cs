using ShopErp.App.ViewModels;
 
 
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
    /// Interaction logic for OrderDetailWindow.xaml
    /// </summary>
    public partial class OrderDetailWindow : Window
    {
        public OrderViewModel OrderVM { get; set; }

        public OrderDetailWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var order = this.DataContext as Order;
                if (order.Type == OrderType.SHUA)
                {
                    order.Type = OrderType.NORMAL;
                    this.btnShuaSet.Content = "设置刷单";
                    this.OrderVM.Background =
                        new SolidColorBrush(new Color {ScA = 1, ScR = 0xDB, ScG = 0xEA, ScB = 0xF9});
                    this.tbOrderType.Text = "正常";
                }
                else
                {
                    order.Type = OrderType.SHUA;
                    this.btnShuaSet.Content = "取消刷单";
                    OrderVM.Background = Brushes.Yellow;
                    this.tbOrderType.Text = "刷单";
                }
                ServiceContainer.GetService<OrderService>().Update(order);
                MessageBox.Show("设置成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DataContext = this.OrderVM.Source;
                var order = this.DataContext as Order;
                if (order.Type == OrderType.SHUA)
                {
                    this.btnShuaSet.Content = "取消刷单";
                }
                else
                {
                    this.btnShuaSet.Content = "设置刷单";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}