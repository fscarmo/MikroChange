using MikroChange.Core.Services;
using MikroChange.Infra.Persistence;
using MikroChange.Infra.Security;


namespace MikroChange.Application.Services;


/// <summary>
/// Implementação das regras de negócio relacionadas à senha master do programa.
/// A credencial admin dos MikroTiks não é persistida: ela é solicitada a cada operação.
/// </summary>
public class AuthService : IAuthService
{
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


    public bool RemoveStoredAdminCredential()
    {
        return LegacyAdminStore.DeleteIfExists();
    }
}
