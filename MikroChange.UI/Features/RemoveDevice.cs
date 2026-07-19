using MikroChange.Core.Enums;
using MikroChange.Core.Services;
using MikroChange.UI.Helpers;


namespace MikroChange.UI.Features;


public class RemoveDevice(IDeviceService deviceService)
{
    private readonly IDeviceService _deviceService = deviceService;


    public void Execute()
    {
        Console.WriteLine("=== Remover dispositivo ===");
        var ip = ConsoleHelper.ReadNonEmpty("IP do dispositivo a remover: ");

        var result = _deviceService.RemoveByIp(ip);

        Console.WriteLine(result == RemoveDeviceResult.Success
            ? "Dispositivo removido com sucesso!"
            : "Nenhum dispositivo encontrado com esse IP.");
    }
}
