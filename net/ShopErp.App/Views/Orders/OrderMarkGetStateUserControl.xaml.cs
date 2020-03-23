using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
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
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// Interaction logic for OrderGetedMarkUserControl.xaml
    /// </summary>
    public partial class OrderMarkGetStateUserControl : UserControl
    {
        private bool myIsLoaded = false;

        private System.Collections.ObjectModel.ObservableCollection<Order> orders =
            new System.Collections.ObjectModel.ObservableCollection<Order>();

        private OrderService os = ServiceContainer.GetService<OrderService>();
        SpeechSynthesizer synth = new SpeechSynthesizer();

        public OrderMarkGetStateUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myIsLoaded)
            {
                return;
            }
            this.dgvOrders.ItemsSource = orders;
            this.myIsLoaded = true;
            this.orders.CollectionChanged += orders_CollectionChanged;
        }

        void orders_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                var group = this.orders.GroupBy(obj => obj.DeliveryCompany).ToArray();
                string info = string.Join("  ", group.Select(obj => obj.Key + ":" + obj.Count()));
                this.tbCount.Text = "统计信息: " + info;
            }
            catch (Exception ex)
            {
                Sound(false);
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Sound(bool success)
        {
            if (success == false)
            {
                this.synth.Speak("错错错");
            }
            else
            {
                this.synth.Speak("好");
            }
        }

        private void tbDeliveryNumber_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            try
            {
                e.Handled = true;
                string number = this.tbDeliveryNumber.Text.Trim();
                if (string.IsNullOrWhiteSpace(number))
                {
                    return;
                }

                if (this.orders.FirstOrDefault(obj => obj.DeliveryNumber.Equals(number)) != null)
                {
                    Speaker.Speak("已存在");
                    return;
                }

                var orders = this.os.GetByDeliveryNumber(number).Datas;
                if (orders == null || orders.Count < 1)
                {
                    Speaker.Speak("订单不存在");
                    return;
                }

                foreach (var order in orders)
                {
                    if ((int)order.State >= (int)OrderState.PAYED && (int)order.State < (int)OrderState.SHIPPED)
                    {
                        this.os.UpdateOrderToGeted(order.Id);
                    }
                    this.orders.Add(order);
                    Sound(true);
                }
            }
            catch (Exception ex)
            {
                Sound(false);
                this.tbDeliveryNumber.Text = "";
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.tbDeliveryNumber.Text = "";
            }
        }
    }
}