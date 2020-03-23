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
    /// Interaction logic for OrderCloseWindow.xaml
    /// </summary>
    public partial class OrderCloseWindow : Window
    {
        public Order Order { get; set; }
        OrderService ser = ServiceContainer.GetService<OrderService>();

        public OrderCloseWindow()
        {
            InitializeComponent();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var item = this.cbbOrderGods.SelectedItem as OrderGoodsCloseViewModel;
                if (item == null)
                {
                    MessageBox.Show("请选择要关闭的商品");
                    return;
                }

                if (MessageBox.Show(item.Title, "关闭商品", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                    MessageBoxResult.Yes)
                {
                    return;
                }
                int count = int.Parse(this.tbCount.Text.Trim());
                ser.CloseOrder(this.Order.Id, item.OrderGoodsId, count);
                MessageBox.Show("已成功");
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var vms = new List<OrderGoodsCloseViewModel>();
            if (this.Order.OrderGoodss != null)
            {
                vms = this.Order.OrderGoodss.Where(obj => (int) obj.State <= (int) OrderState.SHIPPED)
                    .Select(obj => new OrderGoodsCloseViewModel
                    {
                        Title = obj.Number + "," + obj.Color + obj.Size + "," + obj.GoodsId,
                        OrderGoodsId = obj.Id
                    }).ToList();
            }

            vms.Insert(0, new OrderGoodsCloseViewModel {Title = "所有", OrderGoodsId = 0});
            this.cbbOrderGods.ItemsSource = vms;
            this.cbbOrderGods.SelectedIndex = 0;
        }
    }
}