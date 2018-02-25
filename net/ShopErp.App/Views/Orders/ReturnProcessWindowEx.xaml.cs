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
using System.IO;
using ShopErp.App.ViewModels;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.Domain;
using ShopErp.App.Utils;
using System.Printing;
using ShopErp.App.Converters;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;

namespace ShopErp.App.Views.Orders
{
    /// <summary>
    /// ReturnProcessWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ReturnProcessWindowEx : System.Windows.Window
    {
        public OrderReturnViewModel OrderReturn { get; set; }
        private VendorService vs = ServiceContainer.GetService<VendorService>();

        public ReturnProcessWindowEx()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbReason.Bind<OrderReturnReason>();
            this.cbbReason.SelectedItem = EnumUtil.GetEnumValueDescription(this.OrderReturn.Source.Reason);
            this.tbRecivedCount.Text = this.OrderReturn.Source.Count.ToString();
            this.tbGoodsMoney.Text = this.OrderReturn.Source.GoodsMoney.ToString("F2");
            this.tbGoodsInfo.Text = this.OrderReturn.Source.GoodsInfo;
        }

        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                if (btn == null)
                {
                    MessageBox.Show("事件源不是Button");
                    return;
                }
                var imageCtr = btn.Content as Image;
                if (imageCtr == null)
                {
                    MessageBox.Show("按钮下面不是图形控件");
                }

                Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
                bool? ret = ofd.ShowDialog();
                if (ret == null || ret.Value == false)
                {
                    return;
                }

                imageCtr.Source = (new WebUrlImageConverter()).Convert(ofd.FileName, null, null, null) as ImageSource;
                imageCtr.Tag = ofd.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void PrintInfo()
        {
            try
            {
                OrderReturnPrintDocument orp = new OrderReturnPrintDocument();
                var printTemplate = Print.FilePrintTemplateRepertory.GetAllN()
                    .FirstOrDefault(obj => obj.Type == PrintTemplate.TYPE_RETURN);
                if (printTemplate == null)
                {
                    throw new Exception("未找到退货模板");
                }
                orp.GenPages(new OrderReturn[] { this.OrderReturn.Source }, printTemplate);
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

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var ser = ServiceContainer.GetService<OrderReturnService>();
                if (this.cbbReason.GetSelectedEnum<OrderReturnReason>() == OrderReturnReason.NONE)
                {
                    MessageBox.Show("请选择退换原因");
                    return;
                }

                int count = int.Parse(this.tbRecivedCount.Text.Trim());
                if (count < 0)
                {
                    MessageBox.Show("请输入实收数量");
                    return;
                }
                float goodsMoney = float.Parse(this.tbGoodsMoney.Text.Trim());
                string goodsInfo = tbGoodsInfo.Text.Trim();
                if (string.IsNullOrWhiteSpace(goodsInfo))
                {
                    throw new Exception("商品信息不能为空");
                }
                this.OrderReturn.Source.Reason = this.cbbReason.GetSelectedEnum<OrderReturnReason>();
                this.OrderReturn.Source.GoodsInfo = goodsInfo;
                this.OrderReturn.Source.Count = count;
                this.OrderReturn.Source.GoodsMoney = goodsMoney;
                this.OrderReturn.Source.Comment = "";
                this.OrderReturn.Source.ProcessOperator = OperatorService.LoginOperator.Number;
                this.OrderReturn.Source.ProcessTime = DateTime.Now;
                this.OrderReturn.Source.State = OrderReturnState.PROCESSED;
                if (this.chkPrintInfo.IsChecked != null && this.chkPrintInfo.IsChecked.Value)
                {
                    this.PrintInfo();
                }
                ser.Update(this.OrderReturn.Source);
                this.DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}