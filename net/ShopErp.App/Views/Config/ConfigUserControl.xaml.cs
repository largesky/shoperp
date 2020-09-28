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
using ShopErp.App.Device;
using ShopErp.App.Device.Kunhong;
using ShopErp.App.Domain;
using ShopErp.App.Utils;
using ShopErp.App.Views.Extenstions;
using ShopErp.App.Service;
using ShopErp.App.Service.Restful;
using ShopErp.Domain;
using System.Xml.Linq;
using ShopErp.App.Service.Print;

namespace ShopErp.App.Views.Config
{
    /// <summary>
    /// Interaction logic for ConfigUserControl.xaml
    /// </summary>
    public partial class ConfigUserControl : UserControl
    {
        private bool myLoaded = false;

        public ConfigUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.myLoaded)
            {
                return;
            }

            try
            {
                //图片
                string imageMode = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_MODE, "内网");
                this.cbbImageMode.SelectedIndex = imageMode == "内网" ? 0 : (imageMode == "外网" ? 1 : 2);
                this.tbImageDir.Text = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");

                //电子面单发货人信息
                this.tbSenderName.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, "贾勇");
                this.tbSenderPhone.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, "19950350106");

                //天猫设置
                this.cbbGoodsTemplateType.ItemsSource = PrintTemplateService.GetAllLocal().Where(obj => obj.Type == PrintTemplate.TYPE_GOODS).Select(obj => obj.Name).ToArray();
                this.cbbGoodsTemplateType.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_GOODS_TEMPLATE, "");

                //打印机配置
                string[] names = System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().ToArray();
                this.cbbPrinterDeliveryHot.ItemsSource = names;
                this.cbbPrinterDeliveryHot.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, "");
                this.cbbPrinterReturnBarCode.ItemsSource = names;
                this.cbbPrinterReturnBarCode.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_RETURN_BARCODE, "");
                this.cbbPrinterGoodsBarCode.ItemsSource = names;
                this.cbbPrinterGoodsBarCode.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_GOODS_BARCODE, "");
                this.cbbPrinterA4.ItemsSource = names;
                this.cbbPrinterA4.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_A4, "");

                //业务设置
                string odm = LocalConfigService.GetValue(SystemNames.CONFIG_ORDER_DOWNLOAD_MODE, "");
                this.cbbOrderDownloadMode.SelectedIndex = "本地读取" == odm ? 1 : 0;
                this.tbNetworkMaxTimeOut.Text = LocalConfigService.GetValue(SystemNames.CONFIG_NETWORK_MAX_TIMEOUT, "10");
                this.tbGoodsCountName.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, "GOODS_NAME", "贾勇");
                this.tbGoodsCountPhone.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, "GOODS_PHONE", "19950350106");
                this.tbOrderDownloadDay.Text = LocalConfigService.GetValue(SystemNames.CONFIG_ORDER_DOWNLOAD_DAY, "3");
                this.tbShippMoney.Text = LocalConfigService.GetValue(SystemNames.CONFIG_SHIPP_MONEY, "2.5");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //图片设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_WEB_IMAGE_MODE, this.cbbImageMode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_WEB_IMAGE_DIR, this.tbImageDir.Text.Trim());

                //电子面单发货人信息
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, this.tbSenderName.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, this.tbSenderPhone.Text.Trim());

                //天猫设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_GOODS_TEMPLATE, this.cbbGoodsTemplateType.Text.Trim());

                //打印机设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, this.cbbPrinterDeliveryHot.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_RETURN_BARCODE, this.cbbPrinterReturnBarCode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_GOODS_BARCODE, this.cbbPrinterGoodsBarCode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_A4, this.cbbPrinterA4.Text.Trim());

                //业务设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_ORDER_DOWNLOAD_MODE, this.cbbOrderDownloadMode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_NETWORK_MAX_TIMEOUT, int.Parse(this.tbNetworkMaxTimeOut.Text.Trim()).ToString());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, "GOODS_PHONE", this.tbGoodsCountPhone.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, "GOODS_NAME", this.tbGoodsCountName.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_ORDER_DOWNLOAD_DAY, this.tbOrderDownloadDay.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_SHIPP_MONEY, this.tbShippMoney.Text.Trim());
                MessageBox.Show("保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdateAddressArea_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var resp = ServiceContainer.GetService<WuliuNumberService>().UpdateAddressArea();
                MessageBox.Show("更新成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}