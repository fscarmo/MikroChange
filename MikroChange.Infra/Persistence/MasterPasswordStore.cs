using MikroChange.Infra.Security;
using System.Text.Json;


namespace MikroChange.Infra.Persistence;


internal class MasterPasswordRecord
{
    public string Salt { get; set; } = string.Empty;
    public string Hash { get; set; } = string.Empty;
    public int Iterations { get; set; }
}


/// <summary>
/// Persiste o hash da senha master. Não guarda nem manipula a senha em texto puro -
/// apenas grava/lê o resultado já calculado pela camada de Security.
/// </summary>
public static class MasterPasswordStore
{
    public static bool Exists() => File.Exists(Paths.MasterPasswordFile);

    
    public static void Save(PasswordHasher.HashResult result)
    {
        var record = new MasterPasswordRecord
        {
            Salt = result.SaltBase64,
            Hash = result.HashBase64,
            Iterations = result.Iterations
        };

        var json = JsonSerializer.Serialize(record);
        File.WriteAllText(Paths.MasterPasswordFile, json);
    }


    public static (string Salt, string Hash, int Iterations) Load()
    {
        var json = File.ReadAllText(Paths.MasterPasswordFile);
        var record = JsonSerializer.Deserialize<MasterPasswordRecord>(json)
                     ?? throw new InvalidOperationException("Arquivo de senha master corrompido.");

        return (record.Salt, record.Hash, record.Iterations);
    }
}
