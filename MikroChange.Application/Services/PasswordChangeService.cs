using MikroChange.Core.Enums;
using MikroChange.Core.Exceptions;
using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.Infra.MikroTik.Clients;


namespace MikroChange.Application.Services;


/// <summary>
/// Orquestra a alteração de senha de um usuário em vários MikroTiks, um de cada vez.
/// Traduz falhas de conexão / autenticação / comando em um resultado por dispositivo,
/// para que a camada de UI possa reportar sucesso/erro individualmente sem interromper o lote.
/// </summary>
public class PasswordChangeService : IPasswordChangeService
{
    public IEnumerable<Change> ExecuteChange(
        IEnumerable<Device> devices,
        Admin deviceAdminAccess,
        string targetUsername,
        string newPassword)
    {
        if (deviceAdminAccess.Username == targetUsername)
        {
            yield return new Change(
                ChangeStatus.ProtectedUser,
                null,
                $"Não é permitido alterar a senha do usuário \"{deviceAdminAccess.Username}\".");
            yield break;
        }

        foreach (var device in devices)
        {
            yield return ChangeOnDevice(device, deviceAdminAccess, targetUsername, newPassword);
        }
    }


    private static Change ChangeOnDevice(
        Device targetDeviceToChange,
        Admin deviceAdminAccess,
        string targetUsername,
        string newPassword)
    {
        using var client = new MikroTikApiClient();
        try
        {
            client.Connect(targetDeviceToChange.Ip);
        }
        catch (MikroTikException e)
        {
            return new Change(ChangeStatus.ConnectionFailed, targetDeviceToChange, e.Message);
        }

        try
        {
            client.ExecuteLogin(deviceAdminAccess.Username, deviceAdminAccess.Password);
        }
        catch (MikroTikException e)
        {
            return new Change(ChangeStatus.AuthFailed, targetDeviceToChange, e.Message);
        }

        try
        {
            client.SetUserPassword(targetUsername, newPassword);
        }
        catch (MikroTikException e)
        {
            return new Change(ChangeStatus.CommandFailed, targetDeviceToChange, e.Message);
        }

        return new Change(ChangeStatus.Success, targetDeviceToChange, null);
    }
}
