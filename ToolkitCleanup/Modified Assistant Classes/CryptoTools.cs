using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ToolkitCleanup
{
    public static class Crypto
    {
        // Salt can be set to anything.
        // Example string entered below.
        private static readonly byte[] _Salt = Encoding.ASCII.GetBytes("p6806642kbM7c5");

        /// <summary>
        /// Decrypt the given string.  Assumes the string was encrypted using
        /// EncryptStringAES(), using an identical sharedSecret.
        /// </summary>
        /// <param name="cipherText">The text to decrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for decryption.</param>
        public static string DecryptStringAES(string cipherText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                return null;
            }

            string plaintext = null;

            try
            {
                byte[] bytes = Convert.FromBase64String(cipherText);
                using Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _Salt);//!!!! need to implement: , 1001, HashAlgorithmName.SHA256);
                using MemoryStream msDecrypt = new MemoryStream(bytes);
                using RijndaelManaged aesAlg = new RijndaelManaged() { KeySize = 256, BlockSize = 128 }; // Important
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                aesAlg.IV = ReadByteArray(msDecrypt);
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using StreamReader srDecrypt = new StreamReader(csDecrypt);
                plaintext = srDecrypt.ReadToEnd();
                aesAlg?.Clear();
            }
            catch
            {
                plaintext = string.Empty;
            }

            return plaintext = new string(plaintext.Where(c => !char.IsControl(c)).ToArray());
        }

        /// <summary>
        /// Encrypt the given string using AES.  The string can be decrypted using
        /// DecryptStringAES().  The sharedSecret parameters must match.
        /// </summary>
        /// <param name="plainText">The text to encrypt.</param>
        /// <param name="sharedSecret">A password used to generate a key for encryption.</param>
        public static string EncryptStringAES(string plainText, string sharedSecret)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return null;
            }

            if (string.IsNullOrEmpty(sharedSecret))
            {
                return null;
            }

            string outStr = null;

            try
            {
                using Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(sharedSecret, _Salt);//!!!! need to implement: , 1001, HashAlgorithmName.SHA256);
                using RijndaelManaged aesAlg = new RijndaelManaged();
                aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                using MemoryStream msEncrypt = new MemoryStream();
                msEncrypt.Write(BitConverter.GetBytes(aesAlg.IV.Length), 0, sizeof(int));
                msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);

                // Simplifying the CryptoStream using statement breaks the saved string
                // Must include calls to Flush the StreamWriter and CryptoStream to fix
                using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(plainText);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();
                outStr = Convert.ToBase64String(msEncrypt.ToArray());
            }
            catch
            {
            }

            return outStr;
        }

        private static byte[] ReadByteArray(Stream s)
        {
            byte[] rawLength = new byte[sizeof(int)];
            if (s.Read(rawLength, 0, rawLength.Length) != rawLength.Length)
            {
                throw new SystemException("Byte array not properly formatted.");
            }

            byte[] buffer = new byte[BitConverter.ToInt32(rawLength, 0)];
            if (s.Read(buffer, 0, buffer.Length) != buffer.Length)
            {
                throw new SystemException("Unable to read byte array.");
            }

            return buffer;
        }
    }
}