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

        public EncryptionController()
        {
            var (loadedKey, loadedIv) = LoadKeyAndIv("key_iv.dat");
            if(loadedKey == null & loadedIv == null)
            {
                var (generatedKey, generatedIv) = GenerateKeyAndIv();
                key = generatedKey;
                iv = generatedIv;
                SaveKeyAndIv("key_iv.dat", generatedKey, generatedIv);
            }
            else
            {
                key = loadedKey;
                iv = loadedIv;
            }
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

        public string EncryptPassword(string password)
        {
            byte[] encrypted_data = Encrypt(password);
            return Convert.ToBase64String(encrypted_data);
        }

        public string DecryptPassword(string encrypted_password)
        {
            byte[] encrypted_data = Convert.FromBase64String(encrypted_password);
            return Decrypt(encrypted_data);
        }

        public static (byte[] key, byte[] iv) GenerateKeyAndIv(int keySize = 16)
        {
            if (keySize != 16 && keySize != 24 && keySize != 32)
            {
                throw new ArgumentException("Key size must be 16, 24, or 32 bytes.");
            }

            byte[] keyBytes = RandomNumberGenerator.GetBytes(keySize);
            byte[] ivBytes = RandomNumberGenerator.GetBytes(16);

            return (keyBytes, ivBytes);
        }

        public static void SaveKeyAndIv(string filePath, byte[] key, byte[] iv)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            using (var binaryWriter = new BinaryWriter(fileStream))
            {
                binaryWriter.Write(key.Length);
                binaryWriter.Write(key);
                binaryWriter.Write(iv.Length);
                binaryWriter.Write(iv);
            }
        }

        public static (byte[] key, byte[] iv) LoadKeyAndIv(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var binaryReader = new BinaryReader(fileStream))
                {
                    int keyLength = binaryReader.ReadInt32();
                    byte[] key = binaryReader.ReadBytes(keyLength);
                    int ivLength = binaryReader.ReadInt32();
                    byte[] iv = binaryReader.ReadBytes(ivLength);
                    return (key, iv);
                }
            }
            catch(FileNotFoundException ex)
            {
                return (null, null);
            }
        }
    }
}
