using MikroChange.Core.Models;


namespace MikroChange.Core.Services;


public interface IAuthService
{
    public bool HasAdmin();
    
    public Admin? GetAdmin();

    public void SaveAdmin(string username, string password);

    public bool HasMasterPassword();

    public void SetMasterPassword(string password);

    public bool VerifyMasterPassword(string password);

}
