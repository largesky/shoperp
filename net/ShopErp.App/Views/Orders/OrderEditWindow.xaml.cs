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
    /// Interaction logic for OrderHandWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        private System.Collections.ObjectModel.ObservableCollection<OrderGoods> ogs = new System.Collections.ObjectModel.ObservableCollection<OrderGoods>();

        public Order Order { get; set; }

        public Order SourceOrder { get; set; }

        public OrderEditWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //检查权限
                if (OperatorService.LoginOperator.Rights.Contains("创建订单") == false && this.Order == null)
                {
                    throw new Exception("你没有权限创建订单");
                }
                if (Order == null)
                {
                    var minTime = ServiceContainer.GetService<OrderService>().GetDBMinTime();
                    //生成订单
                    this.Order = new Order
                    {
                        CloseOperator = "",
                        CloseTime = minTime,
                        CreateTime = DateTime.Now,
                        DeliveryCompany = "",
                        DeliveryNumber = "",
                        DeliveryOperator = "",
                        DeliveryTime = minTime,
                        DeliveryMoney = 0,
                        PopDeliveryTime = minTime,
                        OrderGoodss = new List<OrderGoods>(),
                        ParseResult = true,
                        PopPayTime = DateTime.Now,
                        PopBuyerComment = "",
                        PopBuyerId = "",
                        PopCodNumber = "",
                        PopFlag = ColorFlag.UN_LABEL,
                        PopOrderId = this.tbPopOrderId.Text.Trim(),
                        PopOrderTotalMoney = 0,
                        PopPayType = PopPayType.ONLINE,
                        PopSellerComment = this.SourceOrder != null ? "换货" : "",
                        PopState = "",
                        PopType = PopType.None,
                        PrintOperator = "",
                        PrintTime = minTime,
                        ReceiverAddress = "",
                        ReceiverMobile = "",
                        ReceiverName = "",
                        ReceiverPhone = "",
                        ShopId = 0,
                        State = OrderState.PAYED,
                        Weight = 0,
                        Id = 0,
                        CreateOperator = OperatorService.LoginOperator.Number,
                        PopCodSevFee = 0,
                        CreateType = OrderCreateType.MANUAL,
                        Type = OrderType.NORMAL,
                    };
                }

                if (this.SourceOrder != null)
                {
                    this.Order.ShopId = this.SourceOrder.ShopId;
                    this.Order.PopBuyerId = this.SourceOrder.PopBuyerId;
                    this.Order.ReceiverName = this.SourceOrder.ReceiverName;
                    this.Order.ReceiverPhone = this.SourceOrder.ReceiverPhone;
                    this.Order.ReceiverMobile = this.SourceOrder.ReceiverMobile;
                }
                if (this.Order.Id < 1)
                {
                    btnAdd_Click(null, null);
                }
                else
                {
                    if (this.Order.OrderGoodss != null && this.Order.OrderGoodss.Count > 0)
                    {
                        foreach (var v in this.Order.OrderGoodss)
                        {
                            this.ogs.Add(v);
                        }
                    }
                }

                this.dgvOrderGoods.ItemsSource = ogs;
                var shops = ServiceContainer.GetService<ShopService>().GetByAll();
                this.cbbShops.ItemsSource = shops.Datas;
                this.cbbShops.SelectedItem = shops.Datas.FirstOrDefault(obj => obj.Id == this.Order.ShopId);
                this.tbQQ.Text = this.Order.PopBuyerId;
                this.dpPayTime.Value = this.Order.PopPayTime;
                this.tbPopOrderId.Text = this.Order.PopOrderId;
                this.tbReceiverName.Text = this.Order.ReceiverName;
                this.tbReceiverPhone.Text = this.Order.ReceiverPhone;
                this.tbReceiverMobile.Text = this.Order.ReceiverMobile;
                this.tbReceiverAddress.Text = this.Order.ReceiverAddress;
                this.chkEmpty.IsChecked = this.Order.Type == OrderType.SHUA;
                var dts = ServiceContainer.GetService<DeliveryTemplateService>().GetByAll().Datas;
                this.cbbDeliveryTemplate.ItemsSource = dts;
                this.cbbDeliveryTemplate.SelectedItem = dts.FirstOrDefault(obj => obj.Id == this.Order.DeliveryTemplateId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.Close();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.ogs.Add(new OrderGoods { Count = 1,  State = this.Order.State });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.dgvOrderGoods.SelectedCells.Count < 1)
            {
                return;
            }

            OrderGoods og = this.dgvOrderGoods.SelectedCells[0].Item as OrderGoods;
            if (og == null)
            {
                MessageBox.Show("未选择数据");
                return;
            }

            this.ogs.Remove(og);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var os = ServiceContainer.GetService<OrderService>();
                DateTime minTime = os.GetDBMinTime();
                DateTime payTime = dpPayTime.Value ?? DateTime.Now;
                //检测订单基础信息
                var shop = this.cbbShops.SelectedItem as Shop;
                string qq = this.tbQQ.Text.Trim();
                string rname = this.tbReceiverName.Text.Trim();
                string rphone = this.tbReceiverPhone.Text.Trim();
                string rmobile = this.tbReceiverMobile.Text.Trim();
                string raddress = this.tbReceiverAddress.Text.Trim();

                if (string.IsNullOrWhiteSpace(qq))
                {
                    if (MessageBox.Show("买家账号为空，是否保存?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                if (this.SourceOrder == null && string.IsNullOrWhiteSpace(tbPopOrderId.Text.Trim()))
                {
                    if (MessageBox.Show("订单编号为空是否继续", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                if (shop == null)
                {
                    throw new Exception("店铺不能为空");
                }

                if (string.IsNullOrWhiteSpace(rname) ||
                    (string.IsNullOrWhiteSpace(rphone) && string.IsNullOrWhiteSpace(rmobile)) ||
                    string.IsNullOrWhiteSpace(raddress))
                {
                    throw new Exception("收货人信息不能为空");
                }
                this.Order.ShopId = shop.Id;
                this.Order.PopType = shop.PopType;
                this.Order.PopBuyerId = qq;
                this.Order.PopPayTime = payTime;
                this.Order.PopOrderId = tbPopOrderId.Text.Trim();
                this.Order.ReceiverName = rname;
                this.Order.ReceiverMobile = rmobile;
                this.Order.ReceiverPhone = rphone;
                this.Order.ReceiverAddress = raddress;
                this.Order.Type = this.chkEmpty.IsChecked.Value ? OrderType.SHUA : OrderType.NORMAL;
                var dt = this.cbbDeliveryTemplate.SelectedItem as DeliveryTemplate;
                this.Order.DeliveryTemplateId = dt == null ? 0 : dt.Id;
                //检测商品信息
                foreach (var og in this.ogs)
                {
                    if (string.IsNullOrWhiteSpace(og.Number) || string.IsNullOrWhiteSpace(og.Color) || string.IsNullOrWhiteSpace(og.Size) || og.Count < 0)
                    {
                        throw new Exception("商品信息不能为空");
                    }
                }

                this.Order.OrderGoodss.Clear();
                foreach (var og in this.ogs)
                {
                    this.Order.OrderGoodss.Add(og);
                }

                if (this.Order.Id > 0)
                {
                    ServiceContainer.GetService<OrderService>().Update(this.Order);
                }
                else
                {
                    this.Order.Id = ServiceContainer.GetService<OrderService>().Save(this.Order);
                }
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