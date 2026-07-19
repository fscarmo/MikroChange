using MikroChange.Core.Models;


namespace MikroChange.Core.Services;


public interface IPasswordChangeService
{
    public IEnumerable<Change> ExecuteChange(
        IEnumerable<Device> devices,
        Admin deviceAdminAccess,
        string targetUsername,
        string newPassword);
}
