using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public class FileServices
    {
        public static string GetHashSHA256(string fullPathFile) {
            using (var sha256 = SHA256.Create()) {

                using (var stream = File.OpenRead(fullPathFile))
                {
                    byte[] hash = sha256.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
