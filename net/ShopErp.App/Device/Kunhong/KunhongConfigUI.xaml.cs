using System.Windows;
using System.Windows.Controls;
using ShopErp.App.Service;

namespace ShopErp.App.Device.Kunhong
{
    /// <summary>
    /// KunhongConfigUI.xaml 的交互逻辑
    /// </summary>
    public partial class KunhongConfigUI : UserControl, IDeviceConfigUI
    {
        public KunhongConfigUI()
        {
            InitializeComponent();
        }

        public Control GetControl()
        {
            return this;
        }

        public void Save()
        {
            this.btnSave_Click(null, null);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            LocalConfigService.UpdateValue(KunhongDevice.SERIAL_PORT, this.cbbPorts.Text.Trim());
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.cbbPorts.ItemsSource = System.IO.Ports.SerialPort.GetPortNames();
            this.cbbPorts.Text = LocalConfigService.GetValue(KunhongDevice.SERIAL_PORT, "");
        }
    }
}