using MikroChange.Core.Services;
using MikroChange.UI.Features;
using MikroChange.UI.Helpers;


namespace MikroChange.UI;


public class AppMikroChange(
    IAuthService authService,
    IDeviceService deviceService,
    IPasswordChangeService passwordChangeService,
    IUserStatusService userStatusService)
{
    private readonly IAuthService _authService = authService;
    private readonly IDeviceService _deviceService = deviceService;
    private readonly IPasswordChangeService _passwordChangeService = passwordChangeService;
    private readonly IUserStatusService _userStatusService = userStatusService;


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

        RemoveStoredAdminCredential();
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


    /// <summary>
    /// Versões anteriores salvavam a credencial admin em disco (admin.dat). Como agora ela é solicitada
    /// a cada operação e mantida apenas em memória, remove o arquivo antigo caso exista.
    /// </summary>
    private void RemoveStoredAdminCredential()
    {
        try
        {
            if (!_authService.RemoveStoredAdminCredential())
            {
                return;
            }

            ConsoleHelper.PrintWarning(
                "A credencial admin salva por versões anteriores (admin.dat) foi removida.\n" +
                "Agora usuário e senha admin são solicitados a cada operação e ficam apenas em memória.");
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            ConsoleHelper.PrintWarning(
                $"Não foi possível remover o admin.dat de versões anteriores: {e.Message}\n" +
                "Apague manualmente o arquivo %LocalAppData%\\MikroChange\\admin.dat.");
        }

        ConsoleHelper.Pause();
    }


    private void MainMenuLoop()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== MikroTik Password Changer ===\n");
            Console.WriteLine("1 - Alterar senha master do MikroChange");
            Console.WriteLine("2 - Cadastrar novo dispositivo MikroTik");
            Console.WriteLine("3 - Remover dispositivo MikroTik");
            Console.WriteLine("4 - Listar dispositivos MikroTik");
            Console.WriteLine("5 - Executar alteração de senha em massa");
            Console.WriteLine("6 - Habilitar/Desabilitar usuário em massa");
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
                    new RegisterDevice(_deviceService).Execute();
                    break;
                case "3":
                    new RemoveDevice(_deviceService).Execute();
                    break;
                case "4":
                    new ListDevices(_deviceService).Execute();
                    break;
                case "5":
                    new ChangeDevicesPassword(_deviceService, _passwordChangeService).Execute();
                    break;
                case "6":
                    new ChangeDevicesUserStatus(_deviceService, _userStatusService).Execute();
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
