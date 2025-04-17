using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace FattestInc {
    public static class EncryptionUtils {
        static readonly EncryptionAES1 encryptionAES1 = new();
        static readonly EncryptionNone encryptionNone = new();

        static IEncryption GetEncryptionMethod(EncryptionMethod encryptionMethod) {
            return encryptionMethod switch {
                EncryptionMethod.None => encryptionNone,
                EncryptionMethod.AES1 => encryptionAES1,
                _ => throw new ArgumentOutOfRangeException(nameof(encryptionMethod), encryptionMethod, null)
            };
        }
        
        public static string Encrypt(EncryptionMethod encryptionMethod, string plainText) {
            return plainText;
        }

        public static string Decrypt(EncryptionMethod encryptionMethod, string base64Encrypted) {
            return base64Encrypted;
        }
        

        public class EncryptionNone : IEncryption {
            public string Encrypt(string plainText) {
                return plainText;
            }

            public string Decrypt(string base64Encrypted) {
                return base64Encrypted;
            }
        }
        
        public class EncryptionAES1 : IEncryption {
            static readonly string encryptionKey = Application.identifier + "_encryption"; // 16, 24, or 32 characters for AES

            public string Encrypt(string plainText) {
                using var aes = Aes.Create();
                var key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                aes.Key = key;
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var plainBytes = Encoding.UTF8.GetBytes(plainText);
                var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Combine IV + encrypted data
                var combinedBytes = new byte[aes.IV.Length + encryptedBytes.Length];
                Buffer.BlockCopy(aes.IV, 0, combinedBytes, 0, aes.IV.Length);
                Buffer.BlockCopy(encryptedBytes, 0, combinedBytes, aes.IV.Length, encryptedBytes.Length);

                return Convert.ToBase64String(combinedBytes);
            }

            public string Decrypt(string base64Encrypted) {
                var combinedBytes = Convert.FromBase64String(base64Encrypted);

                using var aes = Aes.Create();
                var key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                aes.Key = key;

                // Extract IV
                var iv = new byte[aes.BlockSize / 8];
                var encryptedBytes = new byte[combinedBytes.Length - iv.Length];
                Buffer.BlockCopy(combinedBytes, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(combinedBytes, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                return Encoding.UTF8.GetString(decryptedBytes);
            }

        }
    }

    public enum EncryptionMethod { None, AES1 }

    public interface IEncryption {
        string Encrypt(string plainText);
        string Decrypt(string base64Encrypted);
    }
}