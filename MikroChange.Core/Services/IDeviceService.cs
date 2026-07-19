using MikroChange.Core.Enums;
using MikroChange.Core.Models;


namespace MikroChange.Core.Services;


public interface IDeviceService
{
    public AddDeviceResult Add(string ip, string name);

    public RemoveDeviceResult RemoveByIp(string ip);

    public List<Device> GetAllDevices();
}
