using System.Runtime.InteropServices;
using System.Text;


namespace MikroChange.Infra.Security;


/// <summary>
/// Encapsula a Windows Data Protection API (DPAPI) via P/Invoke ao crypt32.dll.
/// Os dados protegidos ficam atrelados à conta do usuário do Windows que executa o programa
/// (DataProtectionScope equivalente a "CurrentUser"), sem a necessidade de gerenciar chaves.
/// Não depende de nenhum pacote NuGet externo.
/// </summary>
public static class DpapiProtector
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DataBlob
    {
        public int CbData;
        public IntPtr PbData;
    }


    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptProtectData(
        ref DataBlob pDataIn,
        string? szDataDescr,
        ref DataBlob pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        out DataBlob pDataOut);


    [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CryptUnprotectData(
        ref DataBlob pDataIn,
        IntPtr ppszDataDescr,
        ref DataBlob pOptionalEntropy,
        IntPtr pvReserved,
        IntPtr pPromptStruct,
        int dwFlags,
        out DataBlob pDataOut);


    [DllImport("kernel32.dll")]
    private static extern IntPtr LocalFree(IntPtr hMem);


    // Impede que o Windows tente exibir qualquer UI.
    private const int CryptProtectUiForbidden = 0x1;


    // Entropia adicional: dificulta que o blob criptografado seja aberto por outro processo
    // que não conheça esse valor, mesmo rodando com o mesmo usuário do Windows.
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("MikroTikPasswordChanger.v1");

    
    public static byte[] Protect(byte[] data) => RunCrypt(data, protect: true);

    
    public static byte[] Unprotect(byte[] protectedData) => RunCrypt(protectedData, protect: false);


    private static byte[] RunCrypt(byte[] input, bool protect)
    {
        var inputHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
        var entropyHandle = GCHandle.Alloc(Entropy, GCHandleType.Pinned);

        try
        {
            var dataIn = new DataBlob
            {
                CbData = input.Length,
                PbData = inputHandle.AddrOfPinnedObject()
            };

            var entropyBlob = new DataBlob
            {
                CbData = Entropy.Length,
                PbData = entropyHandle.AddrOfPinnedObject()
            };
            
            bool success = protect
                
                ? CryptProtectData(
                    ref dataIn,
                    "MikroChange",
                    ref entropyBlob,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CryptProtectUiForbidden,
                    out DataBlob dataOut)

                : CryptUnprotectData(
                    ref dataIn,
                    IntPtr.Zero,
                    ref entropyBlob,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    CryptProtectUiForbidden,
                    out dataOut);

            if (!success)
            {
                var errorCode = Marshal.GetLastWin32Error();
                var operation = protect ? "proteger" : "desproteger";
                throw new InvalidOperationException(
                    $"Falha ao {operation} os dados via DPAPI (código de erro do Windows: {errorCode}).");
            }

            try
            {
                var result = new byte[dataOut.CbData];
                if (dataOut.CbData > 0)
                {
                    Marshal.Copy(dataOut.PbData, result, 0, dataOut.CbData);
                }
                return result;
            }
            finally
            {
                LocalFree(dataOut.PbData);
            }
        }
        finally
        {
            inputHandle.Free();
            entropyHandle.Free();
        }
    }
}
