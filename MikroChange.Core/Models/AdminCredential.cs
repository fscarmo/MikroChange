namespace MikroChange.Core.Models;


/// <summary>
/// Credencial do usuário admin usada para autenticar em todos os MikroTiks cadastrados.
/// </summary>
public class AdminCredential
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
