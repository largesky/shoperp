using ShopErp.App.Service.Restful;
using ShopErp.App.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderBatchEditUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderBatchEditUserControl : UserControl
    {
        private System.Collections.ObjectModel.ObservableCollection<OrderViewModel> orderViewModels = new System.Collections.ObjectModel.ObservableCollection<OrderViewModel>();

        public OrderBatchEditUserControl()
        {
            InitializeComponent();
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.orderViewModels.Clear();
                this.dgOrders.ItemsSource = this.orderViewModels;
                string[] popOrderIds = this.tbPopOrderId.Text.Trim().Split(new string[] { " ", Environment.NewLine, ",", "，", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                if (popOrderIds.Length < 1)
                {
                    throw new Exception("请先输入订单数据");
                }
                int type = this.cbbDataType.SelectedIndex;
                List<string> notQuery = new List<string>();
                foreach (var popOrderId in popOrderIds)
                {
                    List<Order> orders = null;
                    if (type == 0)
                    {
                        orders = ServiceContainer.GetService<OrderService>().GetByDeliveryNumber(popOrderId).Datas;
                    }
                    else if (type == 1)
                    {
                        orders = ServiceContainer.GetService<OrderService>().GetById(popOrderId).Datas;
                    }
                    if (orders == null || orders.Count < 1)
                    {
                        notQuery.Add(popOrderId);
                        continue;
                    }
                    foreach (var ov in orders)
                    {
                        this.orderViewModels.Add(new OrderViewModel(ov));
                    }
                }
                this.tbPopOrderIdFail.Text = string.Join(",", notQuery);
                string msg = string.Format("查询完成，共输入订单数：{0},查询到订单数：{1}，未查询到订单列表：{2}", popOrderIds.Length, this.orderViewModels.Count, notQuery.Count);
                MessageBox.Show(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.orderViewModels.Count < 1)
                {
                    throw new Exception("没有订单数据");
                }

                foreach (var ov in this.orderViewModels)
                {
                    ov.Source.Type = ov.Source.Type == ShopErp.Domain.OrderType.SHUA ? ShopErp.Domain.OrderType.NORMAL : ShopErp.Domain.OrderType.SHUA;
                    ServiceContainer.GetService<OrderService>().Update(ov.Source);
                }
                MessageBox.Show("反向更新完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnScanDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.orderViewModels.Count < 1)
                {
                    throw new Exception("没有订单数据");
                }
                var os = ServiceContainer.GetService<OrderService>();
                var orders = this.orderViewModels.Select(obj => obj.Source).ToList();
                List<string> fails = new List<string>();
                while (orders.Count > 0)
                {
                    var first = orders.First();
                    if (string.IsNullOrWhiteSpace(first.DeliveryNumber))
                    {
                        orders.Remove(first);
                        continue;
                    }
                    var oo = orders.Where(obj => obj.DeliveryNumber == first.DeliveryNumber).ToArray();
                    float weight = 0;
                    foreach (var v in oo)
                    {
                        if (v.OrderGoodss != null && v.OrderGoodss.Count > 0)
                        {
                            foreach (var og in v.OrderGoodss)
                            {
                                if (og.State == OrderState.CANCLED || og.State == OrderState.CLOSED || og.State == OrderState.RETURNING || og.State == OrderState.SPILTED)
                                {
                                    continue;
                                }
                                weight += (og.Weight == 0 ? 0.5F : og.Weight) * og.Count;
                            }
                        }
                    }
                    try
                    {
                        os.MarkDelivery(first.DeliveryNumber, weight, true, true, true);
                    }
                    catch (Exception ex)
                    {
                        fails.Add(first.DeliveryNumber);
                    }
                    finally
                    {
                        foreach (var o in oo)
                        {
                            orders.Remove(o);
                        }
                    }
                }
                if (fails.Count < 1)
                {
                    MessageBox.Show("全部标记成功");
                }
                else
                {
                    MessageBox.Show(string.Join(",", fails), "以下快递单号标记失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tbPopOrderId_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string txt = this.tbPopOrderId.Text.Trim();
                string[] popOrderIds = this.tbPopOrderId.Text.Trim().Split(new string[] { " ", Environment.NewLine, ",", "，", "\t" }, StringSplitOptions.RemoveEmptyEntries);
                string nTxt = string.Join(",", popOrderIds);
                if (txt != nTxt)
                {
                    this.tbPopOrderId.Text = nTxt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
