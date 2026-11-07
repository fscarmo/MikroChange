using MikroChange.Core.Models;


namespace MikroChange.Core.Services;


public interface IUserStatusService
{
    public IEnumerable<Change> ExecuteChange(
        IEnumerable<Device> devices,
        Admin deviceAdminAccess,
        string targetUsername,
        bool disable);
}
