using MikroChange.Core.Models;


namespace MikroChange.UI.Helpers;


/// <summary>
/// Solicita a credencial admin dos MikroTiks no momento de executar uma ação que altera dispositivos.
/// A credencial vive apenas em memória, durante a operação: nada é gravado em disco e quem chama
/// deve descartá-la com <see cref="Clear"/> assim que a ação terminar.
/// </summary>
public static class AdminPrompt
{
    /// <summary>
    /// Pede usuário e senha admin. Retorna null (já avisando o usuário) se a senha for vazia.
    /// </summary>
    public static Admin? Read()
    {
        Console.WriteLine("Informe as credenciais de administrador para acessar os MikroTiks.");
        Console.WriteLine("Elas ficam apenas em memória e são descartadas ao final desta operação.\n");

        var username = ConsoleHelper.ReadNonEmpty("Usuário admin: ");
        var password = ConsoleHelper.ReadPassword("Senha admin: ");

        if (string.IsNullOrWhiteSpace(password))
        {
            ConsoleHelper.PrintError("A senha não pode ser vazia.");
            return null;
        }

        return new Admin
        {
            Username = username,
            Password = password,
        };
    }


    /// <summary>
    /// Descarta a credencial: remove as referências guardadas no objeto para que as strings
    /// deixem de ser alcançáveis e possam ser coletadas pelo GC.
    /// </summary>
    public static void Clear(Admin admin)
    {
        admin.Username = string.Empty;
        admin.Password = string.Empty;
    }
}
