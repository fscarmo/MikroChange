using MikroChange.Core.Services;
using MikroChange.UI.Helpers;


namespace MikroChange.UI.Features;


public class ChangeMasterPassword(IAuthService authService)
{
    private readonly IAuthService _authService = authService;


    public void Execute()
    {
        Console.WriteLine("=== Mudar senha master ===");
        var current = ConsoleHelper.ReadPassword("Senha master atual: ");

        if (!_authService.VerifyMasterPassword(current))
        {
            Console.WriteLine("Senha atual incorreta. Operação cancelada.");
            return;
        }

        var newPassword = ConsoleHelper.ReadPassword("Nova senha master: ");

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            Console.WriteLine("A senha não pode ser vazia. Operação cancelada.");
            return;
        }

        var confirmation = ConsoleHelper.ReadPassword("Confirme a nova senha master: ");

        if (newPassword != confirmation)
        {
            Console.WriteLine("As senhas não coincidem. Operação cancelada.");
            return;
        }

        _authService.SetMasterPassword(newPassword);
        Console.WriteLine("Senha master alterada com sucesso!");
    }
}
