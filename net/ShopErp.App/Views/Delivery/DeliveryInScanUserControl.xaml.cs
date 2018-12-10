using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
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

namespace ShopErp.App.Views.Delivery
{
    /// <summary>
    /// ReturnUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class DeliveryInScanUserControl : UserControl
    {
        private bool myLoaded = false;
        private OrderService orderService = ServiceContainer.GetService<OrderService>();

        private System.Collections.ObjectModel.ObservableCollection<DeliveryInViewModel> deliveryInViewModels = new System.Collections.ObjectModel.ObservableCollection<DeliveryInViewModel>();

        public DeliveryInScanUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myLoaded)
            {
                return;
            }
            this.myLoaded = true;
            this.dgvOrderChanges.ItemsSource = this.deliveryInViewModels;
            this.deliveryInViewModels.CollectionChanged += OrderReturns_CollectionChanged;
            this.cbbDeliveryCompanies.ItemsSource = DeliveryCompanyService.GetDeliveryCompaniyNames();
        }

        void OrderReturns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.tbTotalInfo.Text = "当前共:" + this.deliveryInViewModels.Select(obj => obj.DeliveryNumber).Distinct().Count() + "条快递记录, 商品记录:" + this.deliveryInViewModels.Count();
        }

        private void ProcessOrders(Order[] orders, string deliveryCompany, string deliveryNumber, bool isRefused)
        {
            foreach (var or in orders)
            {
                //空包
                if (or.OrderGoodss == null || or.OrderGoodss.Count < 1)
                {
                    DeliveryInViewModel vm = new DeliveryInViewModel
                    {
                        State = "",
                        DeliveryNumber = deliveryNumber,
                        DeliveryCompany = isRefused ? or.DeliveryCompany : deliveryCompany,
                        Action = "删除",
                        SourceOrderGoods = null,
                        OrderId = or.Id,
                        ShopId = or.ShopId,
                        ReceiverMobile = or.ReceiverMobile,
                        OrderGoodsInfo = "",
                    };
                    this.deliveryInViewModels.Add(vm);
                    Speaker.Speak("已找到订单");
                    continue;
                }

                //实体包
                foreach (var og in or.OrderGoodss)
                {
                    DeliveryInViewModel vm = new DeliveryInViewModel
                    {
                        State = "",
                        OrderGoodsInfo = og.Vendor + " " + og.Number + " " + og.Edtion + " " + og.Color + " " + og.Size + " " + og.Count,
                        DeliveryNumber = deliveryNumber,
                        DeliveryCompany = isRefused ? or.DeliveryCompany : deliveryCompany,
                        Action = "删除",
                        SourceOrderGoods = og,
                        OrderId = or.Id,
                        ShopId = or.ShopId,
                        IsRefused = isRefused,
                        ReceiverMobile = or.ReceiverMobile,
                    };
                    this.deliveryInViewModels.Add(vm);
                    Speaker.Speak("已找到订单");
                }
            }
        }

        private void ProcessRefused(string deliveryNumber)
        {
            //先读取发出信息
            var data = ServiceContainer.GetService<OrderService>().GetByDeliveryNumber(deliveryNumber);
            if (data == null || data.Datas.Count < 1)
            {
                Speaker.Speak("拒收未找到订单");
                return;
            }
            this.ProcessOrders(data.Datas.ToArray(), "", deliveryNumber, true);
        }

        private void ProcessReturn(string deliveryCompany, string deliveryNumber)
        {
            //先读取发出信息，在系统找到相应信息,返回单号，有时会填写系统备注中
            var data = ServiceContainer.GetService<OrderService>().GetByAll("", "", "", "", "", 0, DateTime.MinValue, DateTime.MinValue, "", "", OrderState.NONE, PopPayType.None, "", "", null, -1, deliveryNumber, 0, OrderCreateType.NONE, OrderType.NONE, 0, 0);
            if (data != null && data.Datas.Count > 0)
            {
                this.ProcessOrders(data.Datas.ToArray(), deliveryCompany, deliveryNumber, false);
                return;
            }

            //未找到信息
            DeliveryInViewModel vm = new DeliveryInViewModel
            {
                State = "",
                DeliveryNumber = deliveryNumber,
                DeliveryCompany = deliveryCompany,
                Action = "删除",
                SourceOrderGoods = null,
                OrderId = 0,
                OrderGoodsInfo = "",
                IsRefused = false,
            };
            this.deliveryInViewModels.Add(vm);
            Speaker.Speak("退货未找到订单");
        }

        private void tbDeliveryNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                e.Handled = false;
                return;
            }

            try
            {
                String deliveryNumber = this.tbDeliveryNumber.Text.Trim().ToUpper();
                if (deliveryNumber.Length < 1)
                {
                    return;
                }

                if (this.deliveryInViewModels.FirstOrDefault(obj => obj.DeliveryNumber == deliveryNumber) != null)
                {
                    Speaker.Speak("已存在");
                    return;
                }

                if (this.isRefused.IsChecked.Value)
                {
                    this.ProcessRefused(deliveryNumber);
                }
                else
                {
                    if (this.cbbDeliveryCompanies.SelectedItem == null)
                    {
                        Speaker.Speak("未选择快递公司");
                    }
                    else
                    {
                        this.ProcessReturn(this.cbbDeliveryCompanies.Text.Trim(), deliveryNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                Speaker.Speak("发生错误");
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                e.Handled = true;
                this.tbDeliveryNumber.Text = "";
            }
        }


        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("是否清空?", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }
            this.deliveryInViewModels.Clear();
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var s = ServiceContainer.GetService<DeliveryInService>();
                List<string> uploadedDeliveryNumbers = new List<string>();
                foreach (var item in this.deliveryInViewModels)
                {
                    WPFHelper.DoEvents();

                    //已经上传过
                    if (item.Id > 0)
                    {
                        item.State = "上传成功";
                        continue;
                    }

                    //货到付款订单，标记已拒签
                    if (item.SourceOrder != null && item.SourceOrder.PopPayType == PopPayType.COD)
                    {
                        item.SourceOrder.Refused = true;
                        ServiceContainer.GetService<OrderService>().Update(item.SourceOrder);
                    }

                    //上传记录
                    DeliveryIn di = new DeliveryIn
                    {
                        Comment = item.OrderGoodsInfo,
                        CreateTime = DateTime.Now,
                        DeliveryCompany = item.DeliveryCompany,
                        DeliveryNumber = item.DeliveryNumber,
                        CreateOperator = OperatorService.LoginOperator.Number,
                        Id = 0,
                    };
                    di.Id = s.Save(di);
                    item.Id = di.Id;
                    WPFHelper.DoEvents();

                    //如果是拒签就创建退货，并处理
                    if (item.IsRefused)
                    {
                        var ors = ServiceContainer.GetService<OrderReturnService>();
                        var id = ors.Create(item.OrderId, item.SourceOrderGoods.Id, item.DeliveryCompany, item.DeliveryNumber, OrderReturnType.REFUSED, OrderReturnReason.DAY7, item.SourceOrderGoods.Count);
                        var or = ors.GetById(id.data);
                        or.ProcessOperator = OperatorService.LoginOperator.Number;
                        or.ProcessTime = DateTime.Now;
                        or.State = OrderReturnState.PROCESSED;
                        ors.Update(or);
                    }
                    item.State = "上传成功";
                    WPFHelper.DoEvents();
                }
                MessageBox.Show("上传成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeliveryInViewModel order = (e.Source as Hyperlink).DataContext as DeliveryInViewModel;
                if (order == null)
                {
                    return;
                }
                this.deliveryInViewModels.Remove(order);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}