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
                //称重设备
                this.cbbDeviceTypes.ItemsSource = new IDevice[] { new KunhongDevice() };
                foreach (var item in this.cbbDeviceTypes.ItemsSource)
                {
                    if (item.GetType().AssemblyQualifiedName ==
                        LocalConfigService.GetValue(SystemNames.CONFIG_WEIGHT_DEVICE, ""))
                    {
                        this.cbbDeviceTypes.SelectedItem = item;
                        break;
                    }
                }

                //系统设置
                string imageMode = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_MODE, "内网");
                this.cbbImageMode.SelectedIndex = imageMode == "内网" ? 0 : (imageMode == "外网" ? 1 : 2);

                //打印机配置
                string[] names = System.Drawing.Printing.PrinterSettings.InstalledPrinters.OfType<string>().ToArray();
                this.cbbPrinterDeliveryNormal.ItemsSource = names;
                this.cbbPrinterDeliveryNormal.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_PRINTER_DELIVERY_NORMAL, "");
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
                this.tbImageDir.Text = LocalConfigService.GetValue(SystemNames.CONFIG_WEB_IMAGE_DIR, "");
                this.tbName.Text = LocalConfigService.GetValue("GOODS_NAME", "贾勇");
                this.tbPhone.Text = LocalConfigService.GetValue("GOODS_PHONE", "15590065809");
                this.cbbGoodsTemplateType.ItemsSource = Print.FilePrintTemplateRepertory.GetAllN().Where(obj => obj.Type == Service.Print.PrintTemplate.TYPE_GOODS).Select(obj => obj.Name).ToArray();
                this.cbbGoodsTemplateType.SelectedItem = LocalConfigService.GetValue(SystemNames.CONFIG_GOODS_TEMPLATE, "");

                this.tbAppKey.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_APP_KEY, "");
                this.tbAppSecret.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_APP_SECRET, "");
                this.tbAppSession.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_APP_SESSION, "");
                this.tbAppRefreshSession.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_APP_REFRESH_SESSION, "");
                this.tbSellerNumberId.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SELLER_ID, "");

                this.tbSenderName.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, "贾兴红");
                this.tbSenderPhone.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, "15590065809");
                this.tbSenderAddress.Text = ServiceContainer.GetService<SystemConfigService>().Get(-1, SystemNames.CONFIG_CAINIAO_SENDER_ADDRESS, "四川省 成都市 新都区 国际商贸城");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cbbDeviceTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IDevice device = this.cbbDeviceTypes.SelectedItem as IDevice;
            this.dpHostDeviceConfigs.Children.Clear();
            if (device != null)
            {
                IDeviceConfigUI ui = device.CreateNew();
                this.dpHostDeviceConfigs.Children.Add(ui.GetControl());
                this.dpHostDeviceConfigs.DataContext = ui;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //称重设备
                if (this.cbbDeviceTypes.SelectedItem != null)
                {
                    LocalConfigService.UpdateValue(SystemNames.CONFIG_WEIGHT_DEVICE, this.cbbDeviceTypes.SelectedItem.GetType().AssemblyQualifiedName);
                    IDeviceConfigUI ui = this.dpHostDeviceConfigs.DataContext as IDeviceConfigUI;
                    if (ui != null)
                    {
                        ui.Save();
                    }
                }
                //系统设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_WEB_IMAGE_MODE, this.cbbImageMode.Text.Trim());

                //打印机设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_DELIVERY_NORMAL, this.cbbPrinterDeliveryNormal.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_DELIVERY_HOT, this.cbbPrinterDeliveryHot.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_RETURN_BARCODE, this.cbbPrinterReturnBarCode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_GOODS_BARCODE, this.cbbPrinterGoodsBarCode.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_PRINTER_A4, this.cbbPrinterA4.Text.Trim());

                //业务设置
                LocalConfigService.UpdateValue(SystemNames.CONFIG_WEB_IMAGE_DIR, this.tbImageDir.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_ORDER_DOWNLOAD_MODE, this.cbbOrderDownloadMode.Text.Trim());
                LocalConfigService.UpdateValue("GOODS_PHONE", this.tbPhone.Text.Trim());
                LocalConfigService.UpdateValue("GOODS_NAME", this.tbName.Text.Trim());
                LocalConfigService.UpdateValue(SystemNames.CONFIG_GOODS_TEMPLATE, this.cbbGoodsTemplateType.Text.Trim());

                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_APP_KEY, this.tbAppKey.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_APP_SECRET, this.tbAppSecret.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_APP_SESSION, this.tbAppSession.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_APP_REFRESH_SESSION, this.tbAppRefreshSession.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SELLER_ID, this.tbSellerNumberId.Text.Trim());

                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SENDER_NAME, this.tbSenderName.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SENDER_PHONE, this.tbSenderPhone.Text.Trim());
                ServiceContainer.GetService<SystemConfigService>().SaveOrUpdate(-1, SystemNames.CONFIG_CAINIAO_SENDER_ADDRESS, this.tbSenderAddress.Text.Trim());

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