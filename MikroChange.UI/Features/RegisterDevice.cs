using MikroChange.Core.Enums;
using MikroChange.Core.Services;
using MikroChange.UI.Helpers;
using System.Net;


namespace MikroChange.UI.Features;


public class RegisterDevice(IDeviceService deviceService)
{
    private readonly IDeviceService _deviceService = deviceService;
    

    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("=== Cadastrar novo dispositivo ===\n");

        var ip = ConsoleHelper.ReadNonEmpty("IP do dispositivo: ");
        if (!IPAddress.TryParse(ip, out _))
        {
            Console.WriteLine("IP inválido. Operação cancelada.");
            return;
        }

        var name = ConsoleHelper.ReadNonEmpty("Nome do dispositivo: ");

        var result = _deviceService.Add(ip, name);
        switch (result)
        {
            case AddDeviceResult.Success:
                Console.WriteLine("Dispositivo cadastrado com sucesso!");
                break;
            case AddDeviceResult.DuplicateIp:
                Console.WriteLine("Já existe um dispositivo cadastrado com esse IP. Nada foi alterado.");
                break;
            case AddDeviceResult.DuplicateName:
                Console.WriteLine("Já existe um dispositivo cadastrado com esse nome. Nada foi alterado.");
                break;
        }
    }
}
