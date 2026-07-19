namespace MikroChange.Infra.Persistence;


/// <summary>
/// Centraliza os caminhos de armazenamento local usados pelo programa.
/// Os dados ficam em %LocalAppData%\MikroChange, atrelados ao usuário do Windows.
/// </summary>
public static class Paths
{
    public static string RootFolder
    {
        get
        {
            var baseFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var path = Path.Combine(baseFolder, "MikroChange");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static string MasterPasswordFile => Path.Combine(RootFolder, "master.dat");
    
    public static string AdminCredentialFile => Path.Combine(RootFolder, "admin.dat");
    
    public static string DevicesFile => Path.Combine(RootFolder, "devices.dat");
}
