using MikroChange.Core.Contracts;
using MikroChange.Core.Exceptions;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;


namespace MikroChange.Infra.MikroTik.Clients;


/// <summary>
/// Cliente da API do RouterOS (porta padrão 8728). Cuida apenas da semântica do MikroTik
/// (conectar, autenticar, executar comandos) - a leitura/escrita crua de bytes no
/// protocolo fica a cargo de <see cref="MikroChange.Infra.MikroTik.MikroTikStream"/>.
///
/// Suporta os dois esquemas de login existentes no RouterOS:
///  - Moderno (RouterOS >= 6.43): usuário e senha enviados diretamente em uma única sentença.
///  - Legado (RouterOS < 6.43): desafio MD5 em duas etapas.
/// </summary>
public sealed class MikroTikApiClient(int timeout = 8000) : IMikrotikClient
{
    private readonly int _timeout = timeout;
    private readonly TcpClient _client = new();


    private MikroTikStream? _stream;


    public void Connect(string host, int port = 8728)
    {
        try
        {
            var connectTask = _client.ConnectAsync(host, port);
            if (!connectTask.Wait(_timeout))
            {
                throw new MikroTikException($"Tempo esgotado ao conectar em {host}:{port}.");
            }

            var stream = _client.GetStream();
            stream.ReadTimeout = _timeout;
            stream.WriteTimeout = _timeout;

            _stream = new MikroTikStream(stream);
        }
        catch (Exception e) when (e is not MikroTikException)
        {
            throw new MikroTikException($"Falha ao conectar em {host}:{port}: {e.Message}", e);
        }
    }


    public void ExecuteLogin(string username, string password)
    {
        EnsureConnected();

        // Tentativa 1: esquema moderno (RouterOS >= 6.43) - usuário e senha em uma única sentença.
        var reply = Talk("/login", $"=name={username}", $"=password={password}");
        var last = reply[^1];

        if (last[0] == "!done")
        {
            return;
        }

        if (last[0] != "!trap")
        {
            throw new MikroTikException("Resposta inesperada do dispositivo durante o login.");
        }

        // Tentativa 2: esquema legado (RouterOS < 6.43) - desafio MD5 em duas etapas.
        var challengeReply = Talk("/login");
        var challengeSentence = challengeReply[^1];
        var challengeHex = ExtractWordValue(challengeSentence, "ret")
            ?? throw new MikroTikException("Usuário ou senha de administrador inválidos.");
        
        var responseHash = ComputeLegacyChallengeResponse(password, challengeHex);
        var finalReply = Talk("/login", $"=name={username}", $"=response=00{responseHash}");
        var finalSentence = finalReply[^1];

        if (finalSentence[0] != "!done")
        {
            throw new MikroTikException("Usuário ou senha de administrador inválidos.");
        }
    }


    /// <summary>
    /// Localiza o usuário pelo nome e altera sua senha usando /user/set.
    /// </summary>
    public void SetUserPassword(string targetUsername, string newPassword)
    {
        EnsureConnected();

        var findReply = Talk("/user/print", $"?name={targetUsername}");
        var userSentence = findReply.FirstOrDefault(s => s.Count > 0 && s[0] == "!re")
            ?? throw new MikroTikException($"Usuário '{targetUsername}' não encontrado neste MikroTik.");

        var userId = ExtractWordValue(userSentence, ".id")
            ?? throw new MikroTikException("Não foi possível identificar o usuário neste MikroTik.");

        var setReply = Talk("/user/set", $"=numbers={userId}", $"=password={newPassword}");
        var trapSentence = setReply.FirstOrDefault(s => s.Count > 0 && s[0] == "!trap");

        if (trapSentence is not null)
        {
            var message = ExtractWordValue(trapSentence, "message") ?? "Erro desconhecido retornado pelo MikroTik.";
            throw new MikroTikException(message);
        }
    }


    private void EnsureConnected()
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("Conecte-se ao dispositivo antes de executar comandos.");
        }
    }


    /// <summary>
    /// Envia uma sentença e lê sentenças de resposta até (e incluindo) a que começa com "!done".
    /// </summary>
    private List<List<string>> Talk(params string[] words)
    {
        _stream!.WriteSentence(words);

        var sentences = new List<List<string>>();
        while (true)
        {
            var sentence = _stream.ReadSentence();

            if (sentence.Count == 0)
            {
                // Sentença vazia (separador). Ignora e continua lendo.
                continue;
            }

            sentences.Add(sentence);

            if (sentence[0] == "!done")
            {
                break;
            }
        }

        return sentences;
    }


    private static string? ExtractWordValue(IReadOnlyList<string> sentence, string key)
    {
        var prefix = $"={key}=";
        foreach (var word in sentence)
        {
            if (word.StartsWith(prefix, StringComparison.Ordinal))
            {
                return word[prefix.Length..];
            }
        }

        return null;
    }


    private static string ComputeLegacyChallengeResponse(string password, string challengeHex)
    {
        var challengeBytes = Convert.FromHexString(challengeHex);
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        var buffer = new byte[1 + passwordBytes.Length + challengeBytes.Length];
        buffer[0] = 0x00;
        Buffer.BlockCopy(passwordBytes, 0, buffer, 1, passwordBytes.Length);
        Buffer.BlockCopy(challengeBytes, 0, buffer, 1 + passwordBytes.Length, challengeBytes.Length);

        var hash = MD5.HashData(buffer);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }


    public void Dispose()
    {
        _client.Dispose();
    }
}
