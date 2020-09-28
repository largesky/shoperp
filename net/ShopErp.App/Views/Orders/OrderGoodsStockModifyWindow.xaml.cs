

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
using ShopErp.App.Utils;
using ShopErp.Domain;
using ShopErp.App.Views.Extenstions;
namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderGoodsStockModifyWindow.xaml
    /// </summary>
    public partial class OrderGoodsStockModifyWindow : Window
    {
        public OrderGoods OrderGoods { get; set; }

        public OrderGoodsStockModifyWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string des = EnumUtil.GetEnumValueDescription(this.OrderGoods.State);
            var ss = this.cbbStates.ItemsSource.OfType<ComboBoxItem>().Select(obj => obj.Content.ToString().Trim()).ToList();
            this.cbbStates.SelectedIndex = ss.IndexOf(des);
            if (this.cbbStates.SelectedIndex < 0)
            {
                this.cbbStates.SelectedIndex = 0;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OrderState state = (OrderState)EnumUtil.GetEnumValueByDesc(typeof(OrderState), this.cbbStates.Text.Trim());
                var ser = ServiceContainer.GetService<OrderService>();
                ser.UpdateOrderGoodsState(this.OrderGoods.OrderId, this.OrderGoods.Id, state, this.cbbComment.Text.Trim());
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbStates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.cbbComment.SelectedIndex = -1;
                this.cbbComment.Text = "";
                this.cbbComment.IsEditable = true;
                List<string> ss = new List<string>();

                if (this.cbbStates.SelectedIndex == 0)
                {
                    for (int i = 1; i <= this.OrderGoods.Count; i++)
                    {
                        ss.Add("已拿" + i + "双");
                    }
                    this.cbbComment.IsEditable = false;
                }
                else if (this.cbbStates.SelectedIndex == 1)
                {
                    ss.Add("尺码不对");
                    ss.Add("颜色不对");
                    ss.Add("货号不对");
                    ss.Add("商品瑕疵");
                }
                else
                {
                }

                this.cbbComment.ItemsSource = ss;
                if (ss.Count > 0)
                {
                    this.cbbComment.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}