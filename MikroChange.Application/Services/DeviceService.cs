using MikroChange.Core.Enums;
using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.Infra.Persistence;


namespace MikroChange.Application.Services;


/// <summary>
/// Implementação das regras de negócio para cadastro, remoção e listagem de dispositivos.
/// </summary>
public class DeviceService : IDeviceService
{
    public AddDeviceResult Add(string ip, string name)
    {
        var devices = DeviceStore.Load();

        if (devices.Any(d => d.Ip.Equals(ip, StringComparison.OrdinalIgnoreCase)))
        {
            return AddDeviceResult.DuplicateIp;
        }

        if (devices.Any(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            return AddDeviceResult.DuplicateName;
        }

        devices.Add(new Device { Ip = ip, Name = name });
        DeviceStore.Save(devices);

        return AddDeviceResult.Success;
    }


    public RemoveDeviceResult RemoveByIp(string ip)
    {
        var devices = DeviceStore.Load();
        var target = devices.FirstOrDefault(d => d.Ip.Equals(ip, StringComparison.OrdinalIgnoreCase));

        if (target is null)
        {
            return RemoveDeviceResult.NotFound;
        }

        devices.Remove(target);
        DeviceStore.Save(devices);

        return RemoveDeviceResult.Success;
    }


    public List<Device> GetAllDevices()
    {
        return [.. DeviceStore.Load().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)];
    }
}
