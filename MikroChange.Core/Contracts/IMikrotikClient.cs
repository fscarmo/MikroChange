namespace MikroChange.Core.Contracts;


public interface IMikrotikClient : IDisposable
{
    public void Connect(string host, int port);


    public void ExecuteLogin(string username, string password);


    public void SetUserPassword(string targetUsername, string newPassword);
}
