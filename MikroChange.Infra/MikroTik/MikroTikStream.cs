using MikroChange.Core.Exceptions;
using System.Net.Sockets;
using System.Text;


namespace MikroChange.Infra.MikroTik;


/// <summary>
/// Camada de protocolo binário "puro" da API do RouterOS: sabe apenas como codificar e
/// decodificar palavras (words) e sentenças (sentences) sobre um NetworkStream, conforme a
/// especificação oficial.
///
/// Referência do protocolo: https://help.mikrotik.com/docs/display/ROS/API
/// </summary>
public sealed class MikroTikStream(NetworkStream stream)
{
    private readonly NetworkStream _stream = stream;


    /// <summary>
    /// Escreve uma sentença (lista de palavras) e a encerra com uma palavra de tamanho zero.
    /// </summary>
    public void WriteSentence(IReadOnlyList<string> words)
    {
        foreach (var word in words)
        {
            WriteWord(word);
        }

        WriteLength(0);
        _stream.Flush();
    }


    /// <summary>
    /// Lê uma sentença completa (até encontrar uma palavra de tamanho zero).
    /// </summary>
    public List<string> ReadSentence()
    {
        var words = new List<string>();

        while (true)
        {
            var length = ReadLength();
            if (length == 0)
            {
                break;
            }

            var buffer = new byte[length];
            ReadExact(buffer, length);
            words.Add(Encoding.UTF8.GetString(buffer));
        }

        return words;
    }


    private void WriteWord(string word)
    {
        var bytes = Encoding.UTF8.GetBytes(word);
        WriteLength(bytes.Length);
        _stream.Write(bytes, 0, bytes.Length);
    }


    private void WriteLength(int length)
    {
        if (length < 0x80)
        {
            _stream.WriteByte((byte)length);
        }
        else if (length < 0x4000)
        {
            var encoded = length | 0x8000;
            _stream.WriteByte((byte)(encoded >> 8));
            _stream.WriteByte((byte)encoded);
        }
        else if (length < 0x200000)
        {
            var encoded = length | 0xC00000;
            _stream.WriteByte((byte)(encoded >> 16));
            _stream.WriteByte((byte)(encoded >> 8));
            _stream.WriteByte((byte)encoded);
        }
        else if (length < 0x10000000)
        {
            var encoded = unchecked((uint)(length | unchecked((int)0xE0000000)));
            _stream.WriteByte((byte)(encoded >> 24));
            _stream.WriteByte((byte)(encoded >> 16));
            _stream.WriteByte((byte)(encoded >> 8));
            _stream.WriteByte((byte)encoded);
        }
        else
        {
            var encoded = unchecked((uint)length);
            _stream.WriteByte(0xF0);
            _stream.WriteByte((byte)(encoded >> 24));
            _stream.WriteByte((byte)(encoded >> 16));
            _stream.WriteByte((byte)(encoded >> 8));
            _stream.WriteByte((byte)encoded);
        }
    }


    private int ReadLength()
    {
        var c = ReadByteChecked();

        if ((c & 0x80) == 0x00)
        {
            return c;
        }

        if ((c & 0xC0) == 0x80)
        {
            var b2 = ReadByteChecked();
            return ((c & 0x3F) << 8) | b2;
        }

        if ((c & 0xE0) == 0xC0)
        {
            var b2 = ReadByteChecked();
            var b3 = ReadByteChecked();
            return ((c & 0x1F) << 16) | (b2 << 8) | b3;
        }

        if ((c & 0xF0) == 0xE0)
        {
            var b2 = ReadByteChecked();
            var b3 = ReadByteChecked();
            var b4 = ReadByteChecked();
            return ((c & 0x0F) << 24) | (b2 << 16) | (b3 << 8) | b4;
        }

        // 5 bytes (primeiro byte 0xF0): tamanho vem inteiro nos 4 bytes seguintes.
        var c1 = ReadByteChecked();
        var c2 = ReadByteChecked();
        var c3 = ReadByteChecked();
        var c4 = ReadByteChecked();
        return (c1 << 24) | (c2 << 16) | (c3 << 8) | c4;
    }


    private int ReadByteChecked()
    {
        var value = _stream.ReadByte();
        if (value == -1)
        {
            throw new MikroTikException("Conexão encerrada inesperadamente pelo dispositivo.");
        }

        return value;
    }


    private void ReadExact(byte[] buffer, int length)
    {
        var offset = 0;
        while (offset < length)
        {
            var read = _stream.Read(buffer, offset, length - offset);
            if (read == 0)
            {
                throw new MikroTikException("Conexão encerrada inesperadamente pelo dispositivo.");
            }

            offset += read;
        }
    }
}
