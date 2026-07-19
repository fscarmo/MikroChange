using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.Infra.Persistence;
using MikroChange.Infra.Security;


namespace MikroChange.Application.Services;


/// <summary>
/// Implementação das regras de negócio relacionadas à senha master do programa e à credencial
/// admin dos MikroTiks.
/// </summary>
public class AuthService : IAuthService
{
    public bool HasAdmin()
    {
        return AdminStore.Exists();
    }


    public Admin? GetAdmin()
    {
        return AdminStore.Load();
    }


    public void SaveAdmin(string username, string password)
    {
        var admin = new Admin
        {
            Username = username,
            Password = password,
        };
        AdminStore.Save(admin);
    }


    public bool HasMasterPassword()
    {
        return MasterPasswordStore.Exists();
    }


    public void SetMasterPassword(string password)
    {
        var result = PasswordHasher.Hash(password);
        MasterPasswordStore.Save(result);
    }


    public bool VerifyMasterPassword(string password)
    {
        var (salt, hash, iterations) = MasterPasswordStore.Load();
        return PasswordHasher.Verify(password, salt, hash, iterations);
    }
}
