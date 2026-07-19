using MikroChange.Core.Services;
using MikroChange.UI.Features;
using MikroChange.UI.Helpers;


namespace MikroChange.UI;


public class AppMikroChange(
    IAuthService authService,
    IDeviceService deviceService,
    IPasswordChangeService passwordChangeService)
{
    private readonly IAuthService _authService = authService;
    private readonly IDeviceService _deviceService = deviceService;
    private readonly IPasswordChangeService _passwordChangeService = passwordChangeService;


    public void Run()
    {
        Console.Title = "MikroTik Password Changer";

        if (!_authService.HasMasterPassword())
        {
            FirstRunSetup();
        }
        else if (!LoginWithMasterPassword())
        {
            Console.WriteLine("Senha incorreta. Encerrando o programa.");
            return;
        }

        MainMenuLoop();
    }


    private void FirstRunSetup()
    {
        Console.WriteLine("=== Primeira execução ===");
        Console.WriteLine("Nenhuma senha master cadastrada. Vamos cadastrar uma agora.\n");

        while (true)
        {
            var password = ConsoleHelper.ReadPassword("Nova senha master: ");

            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("A senha não pode ser vazia.\n");
                continue;
            }

            var confirmation = ConsoleHelper.ReadPassword("Confirme a senha master: ");

            if (password != confirmation)
            {
                Console.WriteLine("As senhas não coincidem. Tente novamente.\n");
                continue;
            }

            _authService.SetMasterPassword(password);
            Console.WriteLine("\nSenha master cadastrada com sucesso!\n");
            break;
        }
    }


    private bool LoginWithMasterPassword()
    {
        var password = ConsoleHelper.ReadPassword("Senha master: ");
        return _authService.VerifyMasterPassword(password);
    }


    private void MainMenuLoop()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== MikroTik Password Changer ===\n");
            Console.WriteLine("1 - Alterar senha master do MikroChange");
            Console.WriteLine("2 - Cadastrar/Alterar usuário admin");
            Console.WriteLine("3 - Cadastrar novo dispositivo");
            Console.WriteLine("4 - Remover dispositivo");
            Console.WriteLine("5 - Listar dispositivos");
            Console.WriteLine("6 - Executar alteração");
            Console.WriteLine("0 - Sair");
            Console.Write("\nEscolha uma opção: ");

            var option = Console.ReadLine()?.Trim();
            Console.WriteLine();

            switch (option)
            {
                case "1":
                    new ChangeMasterPassword(_authService).Execute();
                    break;
                case "2":
                    new RegisterAdmin(_authService).Execute();
                    break;
                case "3":
                    new RegisterDevice(_deviceService).Execute();
                    break;
                case "4":
                    new RemoveDevice(_deviceService).Execute();
                    break;
                case "5":
                    new ListDevices(_deviceService).Execute();
                    break;
                case "6":
                    new ChangeDevicesPassword(_authService, _deviceService, _passwordChangeService).Execute();
                    break;
                case "0":
                    return;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }

            ConsoleHelper.Pause();
        }
    }
}
