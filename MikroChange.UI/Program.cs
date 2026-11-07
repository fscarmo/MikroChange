using MikroChange.Application.Services;
using MikroChange.Core.Services;


namespace MikroChange.UI;


public class Program
{
    private static readonly IAuthService _authService = new AuthService();
    private static readonly IDeviceService _deviceService = new DeviceService();
    private static readonly IPasswordChangeService passwordChangeService = new PasswordChangeService();
    private static readonly IUserStatusService _userStatusService = new UserStatusService();



    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            Console.WriteLine($"Nenhum argumento esperado, {args.Length} argumento(s) obtido(s).");
        }

        var app = new AppMikroChange(_authService, _deviceService, passwordChangeService, _userStatusService);
        app.Run();
    }
}
