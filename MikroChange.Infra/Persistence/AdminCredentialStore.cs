using MikroChange.Core.Models;
using MikroChange.Infra.Security;
using System.Text;
using System.Text.Json;


namespace MikroChange.Infra.Persistence;


/// <summary>
/// Persiste a credencial de admin usada para acessar os MikroTiks, protegida via DPAPI.
/// </summary>
public static class AdminCredentialStore
{
    public static bool Exists() => File.Exists(Paths.AdminCredentialFile);


    public static void Save(AdminCredential credential)
    {
        var json = JsonSerializer.Serialize(credential);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        var protectedBytes = DpapiProtector.Protect(plainBytes);

        File.WriteAllBytes(Paths.AdminCredentialFile, protectedBytes);
    }


    public static AdminCredential? Load()
    {
        if (!Exists())
        {
            return null;
        }

        var protectedBytes = File.ReadAllBytes(Paths.AdminCredentialFile);
        var plainBytes = DpapiProtector.Unprotect(protectedBytes);
        var json = Encoding.UTF8.GetString(plainBytes);

        return JsonSerializer.Deserialize<AdminCredential>(json);
    }
}
