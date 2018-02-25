namespace ShopErp.App.Device
{
    public interface IDevice
    {
        string Name { get; }

        double ReadWeight();

        IDeviceConfigUI CreateNew();
    }
}