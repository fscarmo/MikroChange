namespace MikroChange.Core.Services;


public interface IAuthService
{
    public bool HasMasterPassword();

    public void SetMasterPassword(string password);

    public bool VerifyMasterPassword(string password);

    /// <summary>
    /// Remove o arquivo de credencial admin deixado por versões anteriores (o admin agora só vive em memória).
    /// Retorna true se havia um arquivo e ele foi removido.
    /// </summary>
    public bool RemoveStoredAdminCredential();
}
