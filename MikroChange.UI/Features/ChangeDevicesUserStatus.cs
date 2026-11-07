using MikroChange.Core.Enums;
using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.UI.Helpers;


namespace MikroChange.UI.Features;


public class ChangeDevicesUserStatus(
    IDeviceService deviceService,
    IUserStatusService userStatusService)
{
    private readonly IDeviceService _deviceService = deviceService;
    private readonly IUserStatusService _userStatusService = userStatusService;


    public void Execute()
    {
        Console.Clear();
        Console.WriteLine("=== Habilitar/Desabilitar usuário em massa ===\n");

        var targetDevices = _deviceService.GetAllDevices();

        if (targetDevices.Count == 0)
        {
            Console.WriteLine("Nenhum dispositivo cadastrado. Cadastre ao menos um dispositivo antes de continuar (opção 2).");
            return;
        }

        var disableChoice = ReadAction();

        if (disableChoice is null)
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        var disable = disableChoice.Value;
        var actionInfinitive = disable ? "desabilitar" : "habilitar";
        var actionParticiple = disable ? "desabilitado" : "habilitado";

        var targetUsername = ConsoleHelper.ReadNonEmpty($"\nUsuário (dentro do MikroTik) que será {actionParticiple}: ");

        Console.WriteLine($"\nIsso irá {actionInfinitive} o usuário '{targetUsername}' em {targetDevices.Count} dispositivo(s) cadastrado(s).");

        if (!ConsoleHelper.Confirm("Tem certeza que deseja continuar?"))
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        var adminDeviceAccess = AdminPrompt.Read();

        if (adminDeviceAccess is null)
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        Console.WriteLine();

        try
        {
            ExecuteChangeLoop(targetDevices, adminDeviceAccess, targetUsername, disable);
        }
        finally
        {
            AdminPrompt.Clear(adminDeviceAccess);
        }
    }


    /// <summary>
    /// Pergunta qual ação executar. Retorna true para desabilitar, false para habilitar
    /// e null se a operação for cancelada.
    /// </summary>
    private static bool? ReadAction()
    {
        Console.WriteLine("1 - Desabilitar usuário");
        Console.WriteLine("2 - Habilitar usuário");
        Console.WriteLine("0 - Cancelar");

        while (true)
        {
            Console.Write("\nEscolha a ação: ");
            var input = Console.ReadLine();

            if (input is null)
            {
                return null;
            }

            switch (input.Trim())
            {
                case "1":
                    return true;
                case "2":
                    return false;
                case "0":
                    return null;
                default:
                    ConsoleHelper.PrintError("Opção inválida.");
                    break;
            }
        }
    }


    private void ExecuteChangeLoop(
        List<Device> targetDevices,
        Admin adminDeviceAccess,
        string targetUsername,
        bool disable)
    {
        var actionInfinitive = disable ? "desabilitar" : "habilitar";
        var actionParticiple = disable ? "desabilitado" : "habilitado";

        var successCount = 0;
        var failureCount = 0;

        foreach (var outcome in _userStatusService.ExecuteChange(targetDevices, adminDeviceAccess, targetUsername, disable))
        {
            var deviceInfo = outcome.Device is null ? "" : $" {outcome.Device.Name} ({outcome.Device.Ip}) -";

            switch (outcome.Status)
            {
                case ChangeStatus.Success:
                    successCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Green,
                        $"[OK]{deviceInfo} usuário '{targetUsername}' {actionParticiple} com sucesso.");
                    break;

                case ChangeStatus.ConnectionFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO]{deviceInfo} falha de conexão: {outcome.ErrorMessage}");
                    break;

                case ChangeStatus.AuthFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO]{deviceInfo} falha de autenticação: {outcome.ErrorMessage}");
                    break;

                case ChangeStatus.CommandFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO]{deviceInfo} falha ao {actionInfinitive} usuário: {outcome.ErrorMessage}");
                    break;

                case ChangeStatus.ProtectedUser:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO]{deviceInfo} falha na operação: {outcome.ErrorMessage}");
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Concluído: {successCount} sucesso(s), {failureCount} falha(s).");
    }
}
