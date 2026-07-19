using MikroChange.Core.Services;


namespace MikroChange.UI.Features;


public class ListDevices(IDeviceService deviceService)
{
    private readonly IDeviceService _deviceService = deviceService;


    public void Execute()
    {
        Console.WriteLine("=== Dispositivos cadastrados ===");
        var devices = _deviceService.GetAllDevices();

        if (devices.Count == 0)
        {
            Console.WriteLine("Nenhum dispositivo cadastrado.");
            return;
        }

        var ipWidth = Math.Max(15, devices.Max(d => d.Ip.Length) + 2);

        Console.WriteLine($"{"IP".PadRight(ipWidth)}Nome");
        Console.WriteLine(new string('-', ipWidth + 20));

        foreach (var device in devices)
        {
            Console.WriteLine($"{device.Ip.PadRight(ipWidth)}{device.Name}");
        }
    }
}
