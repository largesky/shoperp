using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using ShopErp.App.Device;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.App.Utils;
using ShopErp.Domain;

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// DeliveryScanUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryOutScanUserControl : UserControl
    {
        private bool myLoaded = false;
        private OrderService os = ServiceContainer.GetService<OrderService>();

        private System.Collections.ObjectModel.ObservableCollection<DeliveryScanViewModel> scanedViewModels =
            new System.Collections.ObjectModel.ObservableCollection<DeliveryScanViewModel>();

        public Control UIControl
        {
            get { return this; }
        }

        public DeliveryOutScanUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myLoaded)
            {
                return;
            }
            this.dgvScanedItems.ItemsSource = this.scanedViewModels;
        }

        private void tbDeliveryNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }
            e.Handled = true;
            this.tbResult.Text = "请扫描条码";
            try
            {
                string number = this.tbDeliveryNumber.Text.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(number))
                {
                    return;
                }
                int goodsCount = int.Parse(this.tbGoodsCount.Text.Trim());
                var orders = this.os.MarkDelivery(number, goodsCount, this.chkPopState.IsChecked.Value, this.chkLocalState.IsChecked.Value);
                var normalOrder = orders.Count(obj => obj.Type == OrderType.NORMAL) > 0 ? orders.First(obj => obj.Type == OrderType.NORMAL) : orders.First();
                string goodsInfo = "";
                foreach (var o in orders.Where(obj => obj.Type == OrderType.NORMAL))
                {
                    if (o.OrderGoodss != null && o.OrderGoodss.Count > 0)
                    {
                        goodsInfo += string.Join(" ", o.OrderGoodss.Select(og => og.Vendor + " " + og.Number + " " + og.Edtion + " " + og.Color + " " + og.Size + "(" + og.Count + ")")) + ",";
                    }
                }
                DeliveryScanViewModel svm = new DeliveryScanViewModel
                {
                    DeliveryCompany = normalOrder.DeliveryCompany,
                    DeliveryNumber = number,
                    OrderId = normalOrder.Id.ToString(),
                    Time = DateTime.Now,
                    GoodsCount = goodsCount,
                    OrderGoodsInfo = goodsInfo,
                    ReceiverInfo = normalOrder.ReceiverName + "," + normalOrder.ReceiverPhone + "," + normalOrder.ReceiverMobile + "," + normalOrder.ReceiverAddress,
                };
                var first = this.scanedViewModels.FirstOrDefault(obj => obj.DeliveryNumber == number);
                if (first != null)
                {
                    this.scanedViewModels.Remove(first);
                }
                this.scanedViewModels.Add(svm);
                var count = this.scanedViewModels.GroupBy(obj => obj.DeliveryCompany).ToArray();
                string message = string.Join(",", count.Select(obj => obj.Key + ": " + obj.Select(o => o.DeliveryNumber).Distinct().Count()));
                this.tbTotal.Text = string.Format("订单总数：{0},快递总数:{1},{2}", this.scanedViewModels.Count, this.scanedViewModels.Select(obj => obj.DeliveryNumber).Distinct().Count(), message);
                this.tbResult.Text = svm.OrderGoodsInfo + "  " + svm.ReceiverInfo;
                Speaker.Speak(normalOrder.DeliveryCompany);
            }
            catch (Exception ex)
            {
                Speaker.Speak("出现错误 " + ex.Message);
                this.tbResult.Text = ex.Message;
            }
            finally
            {
                this.tbDeliveryNumber.Text = "";
                this.chkLocalState.IsChecked = true;
            }
        }

        private void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("是否清空当前记录?", "确定", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }
                this.scanedViewModels.Clear();
                this.tbTotal.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

    }
}