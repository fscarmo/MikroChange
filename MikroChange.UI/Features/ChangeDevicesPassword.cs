using MikroChange.Core.Enums;
using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.UI.Helpers;


namespace MikroChange.UI.Features;


public class ChangeDevicesPassword(
    IAuthService authService,
    IDeviceService deviceService,
    IPasswordChangeService passwordChangeService)
{
    private readonly IAuthService _authService = authService;
    private readonly IDeviceService _deviceService = deviceService;
    private readonly IPasswordChangeService _passwordChangeService = passwordChangeService;


    public void Execute()
    {
        if (!_authService.HasAdmin())
        {
            Console.WriteLine("Nenhum usuário admin cadastrado. Cadastre um usuário admin antes de continuar (opção 2).");
            return;
        }

        Console.Clear();
        Console.WriteLine("=== Executar alteração em massa ===\n");

        var targetDevices = _deviceService.GetAllDevices();

        if (targetDevices.Count == 0)
        {
            Console.WriteLine("Nenhum dispositivo cadastrado. Cadastre ao menos um dispositivo antes de continuar (opção 3).");
            return;
        }

        var targetUsername = ConsoleHelper.ReadNonEmpty("Usuário (dentro do MikroTik) que terá a senha alterada: ");
        var newPassword = ConsoleHelper.ReadPassword("Nova senha: ");

        if (string.IsNullOrWhiteSpace(newPassword))
        {
            Console.WriteLine("A senha não pode ser vazia. Operação cancelada.");
            return;
        }

        var confirmation = ConsoleHelper.ReadPassword("Confirme a nova senha: ");

        if (newPassword != confirmation)
        {
            Console.WriteLine("As senhas não coincidem. Operação cancelada.");
            return;
        }

        Console.WriteLine($"\nIsso irá alterar a senha do usuário '{targetUsername}' em {targetDevices.Count} dispositivo(s) cadastrado(s).");

        if (!ConsoleHelper.Confirm("Tem certeza que deseja continuar?"))
        {
            Console.WriteLine("Operação cancelada.");
            return;
        }

        var adminDeviceAccess = _authService.GetAdmin()
            ?? throw new InvalidOperationException("Credencial admin não encontrada.");

        Console.WriteLine();

        ExecuteChangeLoop(targetDevices, adminDeviceAccess, targetUsername, newPassword);
    }


    private void ExecuteChangeLoop(
        List<Device> targetDevices,
        Admin adminDeviceAccess,
        string targetUsername,
        string newPassword)
    {
        var successCount = 0;
        var failureCount = 0;

        foreach (var outcome in _passwordChangeService.ExecuteChange(targetDevices, adminDeviceAccess, targetUsername, newPassword))
        {
            var deviceName = outcome.Device?.Name ?? "Dispositivo desconhecido";
            var deviceAddr = outcome.Device?.Ip ?? "0.0.0.0";

            switch (outcome.Status)
            {
                case ChangeStatus.Success:
                    successCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Green,
                        $"[OK] {deviceName} ({deviceAddr}) - senha alterada com sucesso.");
                    break;

                case ChangeStatus.ConnectionFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO] {deviceName} ({deviceAddr}) - falha de conexão: {outcome.ErrorMessage}");
                    break;

                case ChangeStatus.AuthFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO] {deviceName} ({deviceAddr}) - falha de autenticação: {outcome.ErrorMessage}");
                    break;

                case ChangeStatus.CommandFailed:
                    failureCount++;
                    ConsoleHelper.PrintLine(
                        ConsoleColor.Red,
                        $"[ERRO] {deviceName} ({deviceAddr}) - falha ao alterar senha: {outcome.ErrorMessage}");
                    break;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Concluído: {successCount} sucesso(s), {failureCount} falha(s).");
    }
}
