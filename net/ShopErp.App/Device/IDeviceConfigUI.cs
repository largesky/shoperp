using System.Windows.Controls;

namespace ShopErp.App.Device
{
    public interface IDeviceConfigUI
    {
        Control GetControl();

        void Save();
    }
}