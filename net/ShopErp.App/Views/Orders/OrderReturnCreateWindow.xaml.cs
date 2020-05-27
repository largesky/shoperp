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
using ShopErp.App.Views.Extenstions;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// OrderGoodsCreateReturn.xaml 的交互逻辑
    /// </summary>
    public partial class OrderReturnCreateWindow : Window
    {
        public OrderGoods OrderGoods { get; set; }

        OrderReturnService OrderReturnService = ServiceContainer.GetService<OrderReturnService>();

        public OrderReturnCreateWindow()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string deliveryCompany = this.cbbDeliveryCompanies.Text.Trim();
                string deliveryNumber = this.tbDeliveryNumber.Text.Trim();
                OrderReturnReason rr = this.cbbReason.GetSelectedEnum<OrderReturnReason>();
                OrderReturnType tt = this.cbbType.GetSelectedEnum<OrderReturnType>();
                int count = int.Parse(this.tbCount.Text.Trim());

                if (string.IsNullOrWhiteSpace(deliveryCompany) || string.IsNullOrWhiteSpace(deliveryNumber) ||
                    count < 1)
                {
                    MessageBox.Show("请输入相应信息");
                    return;
                }

                if (rr == OrderReturnReason.NONE)
                {
                    MessageBox.Show("请选择类型");
                    return;
                }

                if (tt != OrderReturnType.RETURN && tt != OrderReturnType.EXCHANGE)
                {
                    throw new Exception("必须选择换货或者退货");
                }
                long id = OrderReturnService.Create(this.OrderGoods.OrderId, this.OrderGoods.Id, deliveryCompany, deliveryNumber, tt, rr, count).data;
                var or = ServiceContainer.GetService<OrderReturnService>().GetById(id);
                if (Math.Abs(or.CreateTime.Subtract(DateTime.Now).TotalMinutes) > 2)
                {
                    MessageBox.Show("订单已经创建退货");
                }
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbDeliveryCompanies.ItemsSource = DeliveryCompanyService.GetDeliveryCompaniyNames();
            this.cbbReason.Bind<OrderReturnReason>();
            this.cbbType.Bind<OrderReturnType>();
            try
            {
                this.cbbReason.SelectedItem = EnumUtil.GetEnumValueDescription(OrderReturnReason.DAY7);
                var order = ServiceContainer.GetService<OrderService>().GetById(this.OrderGoods.OrderId);
                if (order.PopSellerComment.Contains("退") && order.PopSellerComment.Contains("换"))
                {
                    this.cbbType.SelectedItem = EnumUtil.GetEnumValueDescription(OrderReturnType.NONE);
                }
                else if (order.PopSellerComment.Contains("换"))
                {
                    this.cbbType.SelectedItem = EnumUtil.GetEnumValueDescription(OrderReturnType.EXCHANGE);
                }
                else
                {
                    this.cbbType.SelectedItem = EnumUtil.GetEnumValueDescription(OrderReturnType.RETURN);
                }

                if (System.Windows.Forms.Clipboard.ContainsText())
                {
                    string txt = System.Windows.Forms.Clipboard.GetText().Trim();
                    var data = ServiceContainer.GetService<DeliveryInService>().GetByAll("", txt,DateTime.Now.AddDays(-2), DateTime.Now, 0, 0).Datas;
                    if (data.Count > 0)
                    {
                        this.cbbDeliveryCompanies.SelectedItem = data[0].DeliveryCompany;
                        this.tbDeliveryNumber.Text = txt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbDeliveryCompanies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count < 1)
                {
                    this.tbDeliveryNumber.ItemsSource = null;
                    this.tbDeliveryNumber.Text = "";
                    return;
                }

                var dc = e.AddedItems[0] as string;
                //从收件历史中读取 
                var deliveryIn = ServiceContainer.GetService<DeliveryInService>().GetByAll(dc,
                    "", DateTime.Now.AddHours(-16), DateTime.MinValue, 0, 0).Datas;
                var ors = ServiceContainer.GetService<OrderReturnService>().GetByAll(0, 0L, "", "", "",
                        OrderReturnState.NONE, OrderReturnType.NONE, 0, DateTime.Now.AddHours(-16), DateTime.MinValue,
                        0, 0)
                    .Datas;
                var dds = ors.Select(obj => obj.DeliveryNumber).ToArray();

                var sss = deliveryIn.Select(obj => obj.DeliveryNumber)
                    .Where(o => dds.FirstOrDefault(d => d == o) == null).OrderBy(obj => obj).ToArray();
                // var sss = deliveryIn.Select(obj => obj.DeliveryNumber).OrderBy(obj => obj).ToArray();
                this.tbDeliveryNumber.ItemsSource = sss;
                if (sss.Length > 0)
                {
                    this.tbDeliveryNumber.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}