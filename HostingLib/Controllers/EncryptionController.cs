using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Controllers
{
    public class EncryptionController
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        public byte[] Key => key;
        public byte[] Iv => iv;

        public EncryptionController(byte[] _key, byte[] _iv)
        {
            key = _key;
            iv = _iv;
            ValidateKeyAndIv(key, iv);
        }

        private static void ValidateKeyAndIv(byte[] key, byte[] iv)
        {
            if (key.Length is not (16 or 24 or 32))
                throw new ArgumentException("Key size must be 16, 24, or 32 bytes.");

            if (iv.Length != 16)
                throw new ArgumentException("IV size must be 16 bytes.");
        }

        private Aes CreateAes()
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }

        public byte[] Encrypt(string plain_text)
        {
            try
            {
                using var aes = CreateAes();
                var encryptor = aes.CreateEncryptor();
                using var ms_encrypt = new MemoryStream();
                using (var cs_encrypt = new CryptoStream(ms_encrypt, encryptor, CryptoStreamMode.Write))
                using (var sw_encrypt = new StreamWriter(cs_encrypt, Encoding.UTF8))
                {
                    sw_encrypt.Write(plain_text);
                }
                return ms_encrypt.ToArray();
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"EncyptionController.Encrypt - Threw exception {ex}");
                throw;
            }
        }

        public string Decrypt(byte[] encrypted_text)
        {
            try
            {
                using var aes = CreateAes();
                var decryptor = aes.CreateDecryptor();
                using var ms_decrypt = new MemoryStream(encrypted_text);
                using var cs_decrypt = new CryptoStream(ms_decrypt, decryptor, CryptoStreamMode.Read);
                using var sr_decrypt = new StreamReader(cs_decrypt, Encoding.UTF8);
                return sr_decrypt.ReadToEnd();
            }
            catch (Exception ex)
            {
                LoggingController.LogError($"EncyptionController.Decrypt - Threw exception {ex}");
                throw;
            }
        }

        public async Task EncryptStreamAsync(Stream input, Stream output)
        {
            using var aes = CreateAes();
            using var crypto_stream = new CryptoStream(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await input.CopyToAsync(crypto_stream);
        }

        public async Task DecryptStreamAsync(Stream input, Stream output)
        {
            using var aes = CreateAes();
            using var crypto_stream = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            await crypto_stream.CopyToAsync(output);
        }

        public string EncryptData(string data)
        {
            var encrypted_bytes = Encrypt(data);
            return Convert.ToBase64String(encrypted_bytes);
        }

        public string DecryptData(string encryptedData)
        {
            var encrypted_bytes = Convert.FromBase64String(encryptedData);
            return Decrypt(encrypted_bytes);
        }

        public static (byte[] key, byte[] iv) GenerateKeyAndIv(int key_size = 16)
        {
            if (key_size is not (16 or 24 or 32))
                throw new ArgumentException("Key size must be 16, 24, or 32 bytes.");


            var key_bytes = RandomNumberGenerator.GetBytes(key_size);
            var iv_bytes = RandomNumberGenerator.GetBytes(16);

            return (key_bytes, iv_bytes);
        }
    }
}