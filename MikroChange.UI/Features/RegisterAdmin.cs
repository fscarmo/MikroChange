using MikroChange.Core.Services;
using MikroChange.UI.Helpers;


namespace MikroChange.UI.Features;


public class RegisterAdmin(IAuthService authService)
{
    private readonly IAuthService _authService = authService;


    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("=== Cadastrar/Alterar usuário admin (usado para acessar os MikroTiks) ===\n");

        if (_authService.HasAdmin())
        {
            var currentUsername = ConsoleHelper.ReadNonEmpty("Usuário admin atual: ");
            var currentPassword = ConsoleHelper.ReadPassword("Senha admin atual: ");
            var stored = _authService.GetAdmin();

            var isCurrentValid = (
                stored is not null
                && stored.Username == currentUsername
                && stored.Password == currentPassword
            );

            if (!isCurrentValid)
            {
                Console.WriteLine("Usuário ou senha admin atuais incorretos. Operação cancelada.");
                return;
            }
        }
        else
        {
            Console.WriteLine("Nenhum usuário admin cadastrado ainda. Vamos cadastrar o primeiro.\n");
        }

        var newUsername = ConsoleHelper.ReadNonEmpty("Novo usuário admin: ");
        var newPassword = ConsoleHelper.ReadPassword("Nova senha admin: ");

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            Console.WriteLine("A senha não pode ser vazia. Operação cancelada.");
            return;
        }

        var confirmation = ConsoleHelper.ReadPassword("Confirme a nova senha admin: ");

        if (newPassword != confirmation)
        {
            Console.WriteLine("As senhas não coincidem. Operação cancelada.");
            return;
        }

        _authService.SaveAdmin(newUsername, newPassword);
        Console.WriteLine("Usuário admin cadastrado com sucesso!");
    }
}
