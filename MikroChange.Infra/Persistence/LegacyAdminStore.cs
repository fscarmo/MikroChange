namespace MikroChange.Infra.Persistence;


/// <summary>
/// Versões anteriores do programa salvavam a credencial admin em disco (admin.dat, protegido via DPAPI).
/// Hoje ela é solicitada a cada operação e mantida apenas em memória; esta classe existe somente
/// para remover o arquivo remanescente dessas versões.
/// </summary>
public static class LegacyAdminStore
{
    /// <summary>
    /// Apaga o admin.dat antigo, se existir. Retorna true se um arquivo foi removido.
    /// </summary>
    public static bool DeleteIfExists()
    {
        var path = Paths.LegacyAdminCredentialFile;

        if (!File.Exists(path))
        {
            return false;
        }

        File.Delete(path);
        return true;
    }
}
