namespace MikroChange.Core.Models;


/// <summary>
/// Representa um roteador MikroTik cadastrado.
/// </summary>
public class Device
{
    public string Ip { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
