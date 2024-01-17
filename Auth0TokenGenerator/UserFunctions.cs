using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Auth0TokenGenerator
{
    internal static class UserFunctions
    {
        private static byte[] _salt =
        {
            0x13, 0x10, 0x16, 0x09, 0x12, 0x15, 0x11, 0x14,
            0x04, 0x02, 0x07, 0x05, 0x01, 0x08, 0x06, 0x03
        };

        private static byte[] _key =
        {
            0x01, 0x05, 0x09, 0x13, 0x17, 0x21, 0x25, 0x29,
            0x02, 0x06, 0x10, 0x14, 0x18, 0x22, 0x26, 0x30,
            0x03, 0x07, 0x11, 0x15, 0x19, 0x23, 0x27, 0x31,
            0x04, 0x08, 0x12, 0x16, 0x20, 0x24, 0x28, 0x32
        };

        public static string EncodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            string encryptedString;
            byte[] encrypted;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _salt;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(text);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }
            encryptedString = Convert.ToBase64String(encrypted);
            //encryptedString = Encoding.Default.GetString(encrypted);
            return encryptedString;
        }

        public static string DecodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException("text");

            string plaintext = null;
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = _key;
                aesAlg.IV = _salt;
                byte[] cipher = Convert.FromBase64String(text);
                //byte[] cipher = Encoding.Default.GetBytes(text);
                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipher))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt, Encoding.Default))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            try
                            {
                                plaintext = srDecrypt.ReadToEnd();
                            }
                            catch (Exception ex)
                            {
                                // cant messagebox in a web app so we will just return an empty string and let the app fail
                                plaintext = string.Empty;
                            }
                        }
                    }
                }
            }
            return plaintext;
        }
    }
}
