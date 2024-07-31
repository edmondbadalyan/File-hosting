using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class EncryptionController
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        public byte[] GetKey => key;
        public byte[] GetIv => iv;

        public EncryptionController(byte[] _key, byte[] _iv)
        {
            if (_key.Length != 16 && _key.Length != 24 && _key.Length != 32)
            {
                throw new ArgumentException("Key size must be 16, 24, or 32 bytes.");
            }

            if (_iv.Length != 16)
            {
                throw new ArgumentException("IV size must be 16 bytes.");
            }

            key = _key;
            iv = _iv;
        }

        public byte[] Encrypt(string plainText)
        {
            using Aes aes_alg = Aes.Create();
            aes_alg.Key = key;
            aes_alg.IV = iv;
            aes_alg.Padding = PaddingMode.PKCS7;

            ICryptoTransform encryptor = aes_alg.CreateEncryptor(aes_alg.Key, aes_alg.IV);

            using (MemoryStream ms_encrypt = new MemoryStream())
            {
                using (CryptoStream cs_encrypt = new CryptoStream(ms_encrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter sw_encrypt = new StreamWriter(cs_encrypt, Encoding.UTF8))
                {
                    sw_encrypt.Write(plainText);
                    sw_encrypt.Flush();
                }
                return ms_encrypt.ToArray();
            }
        }

        public string Decrypt(byte[] encryptedText)
        {
            try
            {
                using Aes aes_alg = Aes.Create();
                aes_alg.Key = key;
                aes_alg.IV = iv;
                aes_alg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes_alg.CreateDecryptor(aes_alg.Key, aes_alg.IV);

                using MemoryStream ms_decrypt = new MemoryStream(encryptedText);
                {
                    using CryptoStream cs_decrypt = new CryptoStream(ms_decrypt, decryptor, CryptoStreamMode.Read);
                    using StreamReader sr_decrypt = new StreamReader(cs_decrypt, Encoding.UTF8);
                    {
                        return sr_decrypt.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new InvalidOperationException(ex.ToString());
            }
        }

        public void EncryptFile(string input_file, string output_file)
        {
            byte[] file_contents = File.ReadAllBytes(input_file);
            byte[] encrypted_contents = Encrypt(Convert.ToBase64String(file_contents));
            File.WriteAllBytes(output_file, encrypted_contents);
        }

        public void DecryptFile(string input_file, string output_file)
        {
            byte[] encrypted_contents = File.ReadAllBytes(input_file);
            string decrypted_string = Decrypt(encrypted_contents);
            byte[] decrypted_contents = Convert.FromBase64String(decrypted_string);
            File.WriteAllBytes(output_file, decrypted_contents);
        }

        public string EncryptData(string data)
        {
            byte[] encrypted_bytes = Encrypt(data);
            return Convert.ToBase64String(encrypted_bytes);
        }

        public string DecryptData(string encrypted_data)
        {
            byte[] encrypted_bytes = Convert.FromBase64String(encrypted_data);
            return Decrypt(encrypted_bytes);
        }

        public static (byte[] key, byte[] iv) GenerateKeyAndIv(int key_size = 16)
        {
            if (key_size != 16 && key_size != 24 && key_size != 32)
            {
                throw new ArgumentException("Key size must be 16, 24, or 32 bytes.");
            }

            byte[] key_bytes = RandomNumberGenerator.GetBytes(key_size);
            byte[] iv_bytes = RandomNumberGenerator.GetBytes(16);

            return (key_bytes, iv_bytes);
        }
    }
}
