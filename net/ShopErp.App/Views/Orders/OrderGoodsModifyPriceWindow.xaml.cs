using ShopErp.Domain;
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
using System.Windows.Shapes;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderGoodsModifyPriceWindow.xaml 的交互逻辑
    /// </summary>
    public partial class OrderGoodsModifyPriceWindow : Window
    {

        public OrderGoods OrderGoods { get; set; }

        public OrderGoodsModifyPriceWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.tbGoodsInfo.Text = string.Join(" ", OrderGoods.Vendor, OrderGoods.Number, OrderGoods.Edtion, OrderGoods.Color, OrderGoods.Size, OrderGoods.Count);
                this.tbPrice.Text = ((int)OrderGoods.Price).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                float price = float.Parse(this.tbPrice.Text.Trim());
                ShopErp.App.Service.Restful.ServiceContainer.GetService<ShopErp.App.Service.Restful.OrderService>().ModifyOrderGoodsPrice(this.OrderGoods.Id, price);
                MessageBox.Show("保存成功");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
