using MikroChange.Core.Enums;
using MikroChange.Core.Exceptions;
using MikroChange.Core.Models;
using MikroChange.Core.Services;
using MikroChange.Infra.MikroTik.Clients;


namespace MikroChange.Application.Services;


/// <summary>
/// Orquestra a habilitação/desabilitação de um usuário em vários MikroTiks, um de cada vez.
/// Segue o mesmo modelo de <see cref="PasswordChangeService"/>: falhas de conexão / autenticação /
/// comando são traduzidas em um resultado por dispositivo, sem interromper o lote.
/// </summary>
public class UserStatusService : IUserStatusService
{
    public IEnumerable<Change> ExecuteChange(
        IEnumerable<Device> devices,
        Admin deviceAdminAccess,
        string targetUsername,
        bool disable)
    {
        // Proteção contra lockout: o usuário admin (usado para autenticar nos MikroTiks) nunca é alvo.
        if (string.Equals(deviceAdminAccess.Username, targetUsername, StringComparison.OrdinalIgnoreCase))
        {
            var verb = disable ? "desabilitar" : "habilitar";

            yield return new Change(
                ChangeStatus.ProtectedUser,
                null,
                $"Não é permitido {verb} o usuário \"{deviceAdminAccess.Username}\".");
            yield break;
        }

        foreach (var device in devices)
        {
            yield return ChangeOnDevice(device, deviceAdminAccess, targetUsername, disable);
        }
    }


    private static Change ChangeOnDevice(
        Device targetDeviceToChange,
        Admin deviceAdminAccess,
        string targetUsername,
        bool disable)
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
            client.SetUserDisabled(targetUsername, disable);
        }
        catch (MikroTikException e)
        {
            return new Change(ChangeStatus.CommandFailed, targetDeviceToChange, e.Message);
        }

        return new Change(ChangeStatus.Success, targetDeviceToChange, null);
    }
}
