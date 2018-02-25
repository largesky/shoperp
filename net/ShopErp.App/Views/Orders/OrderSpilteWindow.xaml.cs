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
    /// OrderSpilteWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OrderSpilteWindow : Window
    {
        public Order Order { get; set; }

        private OrderSpilteViewModel[] vms = null;

        private OrderService ser = ServiceContainer.GetService<OrderService>();


        public OrderSpilteWindow()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (var vm in this.vms)
                {
                    if (vm.SpilteCount > vm.Count)
                    {
                        throw new Exception(vm.OrderGoodsInfo + "商品要拆分的数量不能现有的数量大");
                    }
                }

                var items = this.vms.Where(obj => obj.SpilteCount > 0).Select(obj => new OrderSpilteInfo
                {
                    OrderId = obj.OrderId,
                    OrderGoodsId = obj.OrderGoodsId,
                    Count = obj.SpilteCount
                }).ToArray();
                ServiceContainer.GetService<OrderService>().SpilteOrderGoods(this.Order.Id, items);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Order.OrderGoodss.Where(obj => (int) obj.State < (int) OrderState.SHIPPED).Count() <= 1)
                {
                    //throw new Exception("订单没有可以拆分的商品");
                }

                if ((int) this.Order.State > (int) OrderState.SHIPPED)
                {
                    throw new Exception("订单已经关闭或者发货不能拆分");
                }

                var orderGoods = this.Order.OrderGoodss.Where(o => (int) o.State < (int) OrderState.SHIPPED).ToArray();
                if (orderGoods.Count() < 1)
                {
                    throw new Exception("订单中没有可以折分的商品");
                }
                this.vms = orderGoods.Select(obj => new OrderSpilteViewModel(obj)).ToArray();
                this.dgvOrderGoods.ItemsSource = vms;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.DialogResult = true;
            }
        }
    }
}