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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.ViewModels;
using ShopErp.App.Domain;
using ShopErp.App.Utils;
using System.Printing;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using ShopErp.App.Service.Print;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// ReturnProcessUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class OrderReturnUserControl : UserControl
    {
        private VendorService vs = ServiceContainer.GetService<VendorService>();
        private bool myContentLoaded = false;

        public OrderReturnUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (myContentLoaded == true)
            {
                return;
            }
            myContentLoaded = true;
            this.cbbStates.Bind<OrderReturnState>();
            this.cbbTypes.Bind<OrderReturnType>();
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            int id = 0;
            long orderId = 0;
            int.TryParse(this.tbId.Text.Trim(), out id);
            long.TryParse(this.tbOrderId.Text.Trim(), out orderId);

            string delivery = this.tbDeliveryNumber.Text.Trim();
            DateTime startTime = this.dpStartTime.Value == null
                ? DateTime.Now.AddDays(-30)
                : this.dpStartTime.Value.Value;
            DateTime endTime = this.dpEndTime.Value == null ? DateTime.MinValue : this.dpEndTime.Value.Value;
            string number = this.tbNumber.Text.Trim();
            string vendor = this.tbVendor.Text.Trim();

            this.pbBar.Parameters.Clear();
            this.pbBar.Parameters.Add("Id", id);
            this.pbBar.Parameters.Add("OrderId", orderId);
            this.pbBar.Parameters.Add("Vendor", vendor);
            this.pbBar.Parameters.Add("Number", number);
            this.pbBar.Parameters.Add("TimeType", this.cbbTimeType.SelectedIndex);
            this.pbBar.Parameters.Add("Start", startTime);
            this.pbBar.Parameters.Add("End", endTime);
            this.pbBar.Parameters.Add("State", (int)(this.cbbStates.GetSelectedEnum<OrderReturnState>()));
            this.pbBar.Parameters.Add("Type", (int)(this.cbbTypes.GetSelectedEnum<OrderReturnType>()));
            this.pbBar.Parameters.Add("DeliveryNumber", delivery);

            this.pbBar.StartPage();
        }

        private void pbBar_PageChanging(object sender, PageBar.PageChangeEventArgs e)
        {
            var ret = ServiceContainer.GetService<OrderReturnService>().GetByAll(e.GetParameter<int>("Id"),
                e.GetParameter<long>("OrderId"), e.GetParameter<string>("Vendor"), e.GetParameter<string>("Number"),
                e.GetParameter<string>("DeliveryNumber"), e.GetParameter<OrderReturnState>("State"),
                e.GetParameter<OrderReturnType>("Type"), e.GetParameter<int>("TimeType"),
                e.GetParameter<DateTime>("Start"), e.GetParameter<DateTime>("End"), e.CurrentPage - 1, e.PageSize);
            OrderService os = ServiceContainer.GetService<OrderService>();
            List<OrderReturnViewModel> vms = new List<OrderReturnViewModel>();

            foreach (var item in ret.Datas)
            {
                OrderReturnViewModel vm = new OrderReturnViewModel(item);
                vm.Order = ServiceContainer.GetService<OrderService>().GetById(item.OrderId);
                vm.OrderGoods = vm.Order == null || vm.Order.OrderGoodss == null ? null : vm.Order.OrderGoodss.FirstOrDefault(obj => obj.Id == item.OrderGoodsId);
                vms.Add(vm);
            }
            this.lstOrderReturns.ItemsSource = vms.ToArray();
            this.pbBar.Total = ret.Total;
            this.pbBar.CurrentCount = ret.Datas.Count;
            this.pbBar.TitleMessage = "当前页退货金额:" + ret.Datas.Select(obj => obj.GoodsMoney).Sum();
        }

        private void btnModify_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                var vm = fe.DataContext as OrderReturnViewModel;

                if (vm == null)
                {
                    MessageBox.Show("没有要修改退换货记录");
                    return;
                }

                var window = new OrderModifyDeliveryInfoWindow
                {
                    DeliveryCompany = vm.Source.DeliveryCompany,
                    DeliveryNumber = vm.Source.DeliveryNumber,
                };
                bool? ret = window.ShowDialog();
                if (ret != null && ret.Value)
                {
                    vm.Source.DeliveryCompany = window.DeliveryCompany;
                    vm.Source.DeliveryNumber = window.DeliveryNumber;
                    ServiceContainer.GetService<OrderReturnService>().Update(vm.Source);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnQueryDelivery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null || btn.DataContext == null || btn.DataContext is OrderReturnViewModel == false)
                {
                    return;
                }
                var order = btn.DataContext as OrderReturnViewModel;
                Delivery.DeliveryQueryWindow window =
                    new Delivery.DeliveryQueryWindow
                    {
                        DeliveryCompany = order.Source.DeliveryCompany,
                        DeliveryNumber = order.Source.DeliveryNumber
                    };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateWithoutOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new OrderGoodsCreateReturnWithoutOrderWindow();
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnProcessEx_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                OrderReturnViewModel vm = fe.DataContext as OrderReturnViewModel;

                if (vm == null)
                {
                    throw new Exception("DataContext数据为空");
                }

                if (string.IsNullOrWhiteSpace(vm.Source.DeliveryCompany) ||
                    string.IsNullOrWhiteSpace(vm.Source.DeliveryNumber))
                {
                    throw new Exception("快递公司和快递单号不能为空");
                }

                ReturnProcessWindowEx window = new ReturnProcessWindowEx { OrderReturn = vm };
                window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPrintInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                OrderReturnViewModel vm = fe.DataContext as OrderReturnViewModel;

                if ((int)vm.Source.State < (int)OrderReturnState.WAITPROCESS)
                {
                    MessageBox.Show("订单没有处理，不能被打印");
                    return;
                }

                OrderReturnPrintDocument orp = new OrderReturnPrintDocument();
                var printTemplate = Print.FilePrintTemplateRepertory.GetAllN().FirstOrDefault(obj => obj.Type == Service.Print.PrintTemplate.TYPE_RETURN);
                if (printTemplate == null)
                {
                    throw new Exception("未找到退货模板");
                }
                orp.GenPages(new OrderReturn[] { vm.Source }, printTemplate);
                //获取打印机对象
                string printer = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_RETURN_BARCODE);
                PrintDialog pd = PrintUtil.GetPrinter(printer);
                pd.PrintTicket.PageMediaSize = new PageMediaSize(printTemplate.Width, printTemplate.Height);
                pd.PrintDocument(orp, "退货");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GoodsImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    return;
                }

                if (fe.Tag == null)
                {
                    return;
                }
                int numberId = (int)fe.Tag;
                if (numberId <= 0)
                {
                    return;
                }
                var s = ServiceContainer.GetService<GoodsService>().GetById(numberId);
                if (s == null)
                {
                    MessageBox.Show("无法获取指定的商品");
                    return;
                }
                if (string.IsNullOrWhiteSpace(s.Url))
                {
                    return;
                }
                Process.Start(s.Url);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string FormatCSVContent(params string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < values.Length; i++)
            {
                sb.Append("\"");
                sb.Append(values[i].Replace("\"", "\"\"")); //双引号，需要转义
                sb.Append("\",");
            }
            sb.Length = sb.Length - 1; //支最后一个 , 号
            sb.AppendLine(); //增加换行
            return sb.ToString();
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var orderVm = this.lstOrderReturns.ItemsSource as OrderReturnViewModel[];
                if (orderVm == null || orderVm.Length < 1)
                {
                    MessageBox.Show("没有需要导出的商品");
                    return;
                }

                Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog();
                sfd.DefaultExt = "csv";
                sfd.AddExtension = true;
                sfd.Filter = "*.csv|CSV文件";
                sfd.FileName = "奇牛退货导出" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".csv";
                bool? ret = sfd.ShowDialog();
                if (ret == false || ret.Value == false)
                {
                    return;
                }
                if (File.Exists(sfd.FileName))
                {
                    File.Delete(sfd.FileName);
                }
                VendorService vs = ServiceContainer.GetService<VendorService>();
                //添加头部
                File.AppendAllText(sfd.FileName, FormatCSVContent("退货编号", "订单编号", "创建时间", "商品信息", "数量", "描述"),
                    Encoding.Default);
                foreach (var order in orderVm)
                {
                    File.AppendAllText(sfd.FileName,
                        FormatCSVContent(order.Source.Id.ToString(),
                            order.Order == null ? "00000000" : order.Order.Id.ToString(),
                            order.Source.CreateTime.ToString("yyyy-MM-dd"), order.Source.GoodsInfo,
                            order.Source.Count.ToString(), order.Source.Comment), Encoding.Default);
                }
                MessageBox.Show(sfd.FileName, "已成功导出");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                OrderReturnViewModel vm = fe.DataContext as OrderReturnViewModel;
                if (vm == null)
                {
                    throw new Exception("对象数据为空");
                }

                if (MessageBox.Show("是否删除退货编号：" + vm.Source.Id, "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (MessageBox.Show("是否删除退货编号：" + vm.Source.Id, "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                OrderReturnService os = ServiceContainer.GetService<OrderReturnService>();
                os.Delete(vm.Source.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                OrderReturnViewModel vm = fe.DataContext as OrderReturnViewModel;
                if (vm == null)
                {
                    throw new Exception("对象数据为空");
                }

                if (MessageBox.Show("是否关闭退货编号：" + vm.Source.Id, "警告", MessageBoxButton.YesNo,
                        MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                vm.Source.State = OrderReturnState.PROCESSED;
                vm.Source.ProcessTime = DateTime.Now;
                vm.Source.ProcessOperator = OperatorService.LoginOperator.Number;
                ServiceContainer.GetService<OrderReturnService>().Update(vm.Source);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateNew_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FrameworkElement fe = sender as FrameworkElement;
                if (fe == null)
                {
                    MessageBox.Show("事件源不为:FrameworkElement");
                    return;
                }
                OrderReturnViewModel vm = fe.DataContext as OrderReturnViewModel;
                if (vm == null)
                {
                    throw new Exception("对象数据为空");
                }

                if (OperatorService.LoginOperator.Rights.Contains("创建订单") == false)
                {
                    throw new Exception("你没有权限创建订单");
                }

                if (vm.Source.Type != OrderReturnType.EXCHANGE)
                {
                    if (MessageBox.Show("该退货不是换货，确认需要创建?", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question) !=
                        MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                var window = new OrderEditWindow { SourceOrder = vm.Order };
                window.ShowDialog();
                if (window.Order == null)
                {
                    return;
                }
                vm.Source.NewOrderId = window.Order.Id;
                ServiceContainer.GetService<OrderReturnService>().Update(vm.Source);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}