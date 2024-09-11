using HostingLib.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HostingLib.Helpers
{
    public class ServerEncryptionHelper
    {
        private static readonly RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048);

        public static string GetPublicKey()
        {
            LoggingController.LogDebug($"ServerEncryptionHelper.GetPublicKey - returned the public key");
            return rsa.ToXmlString(false); // false to get the public key only
        }

        public static byte[] Decrypt(byte[] data)
        {
            return rsa.Decrypt(data, false);
        }
    }
}
