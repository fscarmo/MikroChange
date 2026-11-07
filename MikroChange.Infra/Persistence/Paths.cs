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
    
    /// <summary>
    /// Arquivo usado por versões anteriores para guardar a credencial admin. Não é mais gravado.
    /// </summary>
    public static string LegacyAdminCredentialFile => Path.Combine(RootFolder, "admin.dat");
    
    public static string DevicesFile => Path.Combine(RootFolder, "devices.dat");
}
