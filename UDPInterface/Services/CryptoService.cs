using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace UDPInterface;

internal class CryptoService
{
    static byte[] _key = Encoding.Default.GetBytes("LYLIGRANDTURISMO");
    static byte[] _iv = Encoding.Default.GetBytes("LYLIGRANDTURISMO");
    public static void Encrypt(string file, string data)
    {
        using (StreamWriter _streamWriter = new StreamWriter(file, true))
        {
            using (ICryptoTransform _iCrypto = new TripleDESCryptoServiceProvider().CreateEncryptor(_key, _iv))
            {
                var _byteData = Encoding.Default.GetBytes(data);
                var _encryptedData = _iCrypto.TransformFinalBlock(_byteData, 0, _byteData.Length);
                _streamWriter.Write(Convert.ToBase64String(_encryptedData, 0, _encryptedData.Length));
            }
        }
    }
    public static string Decrypt(string file)
    {
        using (StreamReader _streamReader = new StreamReader(file))
        {
            using (ICryptoTransform _iCrypto = new TripleDESCryptoServiceProvider().CreateDecryptor(_key, _iv))
            {
                var _byteData = Convert.FromBase64String(_streamReader.ReadToEnd());
                var _decryptedData = _iCrypto.TransformFinalBlock(_byteData, 0, _byteData.Length);
                return Encoding.Default.GetString(_decryptedData);
            }
        }
    }
}
