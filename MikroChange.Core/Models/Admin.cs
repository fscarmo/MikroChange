namespace MikroChange.Core.Models;


/// <summary>
/// Credencial do usuário admin usada para autenticar nos MikroTiks. Nunca é gravada em disco:
/// é solicitada a cada operação que altera dispositivos e descartada logo após a execução.
/// </summary>
public class Admin
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
