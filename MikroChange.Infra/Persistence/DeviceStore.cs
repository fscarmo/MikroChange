using MikroChange.Infra.Security;
using MikroChange.Core.Models;
using System.Text;
using System.Text.Json;


namespace MikroChange.Infra.Persistence;


/// <summary>
/// Persiste a lista de dispositivos (IP + nome), protegida via DPAPI.
/// </summary>
public static class DeviceStore
{
    public static List<Device> Load()
    {
        if (!File.Exists(Paths.DevicesFile))
        {
            return [];
        }

        var protectedBytes = File.ReadAllBytes(Paths.DevicesFile);
        if (protectedBytes.Length == 0)
        {
            return [];
        }

        var plainBytes = DpapiProtector.Unprotect(protectedBytes);
        var json = Encoding.UTF8.GetString(plainBytes);

        return JsonSerializer.Deserialize<List<Device>>(json) ?? [];
    }


    public static void Save(List<Device> devices)
    {
        var json = JsonSerializer.Serialize(devices);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var protectedBytes = DpapiProtector.Protect(plainBytes);

        File.WriteAllBytes(Paths.DevicesFile, protectedBytes);
    }
}
