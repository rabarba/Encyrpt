﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encyrpt
{
    public class AESEncryption
    {

        const string Key = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG"; // must be 32 character
        const string IV = "ABCDEFGHIJKLMNOP"; // must be 16 character

        public static string EncryptDefault(string input, string passphrase = null)
        {
            byte[] key, iv;
            PassphraseToDefaultKeyAndIV(RawBytesFromString(passphrase), null, 1, out key, out iv);

            return Convert.ToBase64String(EncryptBytes(Encoding.UTF8.GetBytes(input), key, iv));
        }

        public static string DecryptDefault(string inputBase64, string passphrase = null)
        {
            byte[] key, iv;
            PassphraseToDefaultKeyAndIV(RawBytesFromString(passphrase), null, 1, out key, out iv);

            return Encoding.UTF8.GetString(DecryptBytes(Convert.FromBase64String(inputBase64), key, iv));
        }

        public static byte[] DecryptBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (var cipher = new RijndaelManaged())
            {
                cipher.Key = Key;
                cipher.IV = IV;
                cipher.Mode = CipherMode.CBC;
                cipher.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = cipher.CreateDecryptor(cipher.Key, cipher.IV);

                // Create the streams used for decryption. 
                using (var inputStream = new MemoryStream(cipherText))
                {
                    using (var outputStream = new MemoryStream())
                    {
                        using (CryptoStream decryptedStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                        {
                            var buffer = new byte[1024];
                            decryptedStream.Flush();
                            var read = decryptedStream.Read(buffer, 0, buffer.Length);
                            while (read > 0)
                            {
                                outputStream.Write(buffer, 0, read);
                                decryptedStream.Flush();
                                read = decryptedStream.Read(buffer, 0, buffer.Length);
                            }
                            outputStream.Seek(0, 0);
                            return outputStream.ToArray();
                        }
                    }
                }
            }
        }

        public static byte[] EncryptBytes(byte[] input, byte[] Key, byte[] IV)
        {
            // Check arguments. 
            if (input == null || input.Length <= 0)
                return new byte[0]; //nothing to encode
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Create an RijndaelManaged object 
            // with the specified key and IV. 
            using (RijndaelManaged cipher = new RijndaelManaged())
            {
                cipher.Key = Key;
                cipher.IV = IV;
                cipher.Mode = CipherMode.CBC;
                cipher.Padding = PaddingMode.PKCS7;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = cipher.CreateEncryptor(cipher.Key, cipher.IV);

                // Create the streams used for encryption. 
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (CryptoStream encryptStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                    {
                        encryptStream.Write(input, 0, input.Length);
                        encryptStream.FlushFinalBlock();
                        outputStream.Seek(0, 0);
                        return outputStream.ToArray();
                    }
                }
            }

        }

        static byte[] RawBytesFromString(string input)
        {
            var ret = new List<Byte>();

            foreach (char x in input)
            {
                var c = (byte)((ulong)x & 0xFF);
                ret.Add(c);
            }

            return ret.ToArray();
        }

        public static void PassphraseToDefaultKeyAndIV(byte[] data, byte[] salt, int count, out byte[] key, out byte[] iv)
        {
            List<byte> hashList = new List<byte>();
            byte[] currentHash = new byte[0];

            int preHashLength = data.Length + ((salt != null) ? salt.Length : 0);
            byte[] preHash = new byte[preHashLength];

            System.Buffer.BlockCopy(data, 0, preHash, 0, data.Length);
            if (salt != null)
                System.Buffer.BlockCopy(salt, 0, preHash, data.Length, salt.Length);

            MD5 hash = MD5.Create();
            currentHash = hash.ComputeHash(preHash);

            for (int i = 1; i < count; i++)
            {
                currentHash = hash.ComputeHash(currentHash);
            }

            hashList.AddRange(currentHash);

            while (hashList.Count < 48) // for 32-byte key and 16-byte iv
            {
                preHashLength = currentHash.Length + data.Length + ((salt != null) ? salt.Length : 0);
                preHash = new byte[preHashLength];

                System.Buffer.BlockCopy(currentHash, 0, preHash, 0, currentHash.Length);
                System.Buffer.BlockCopy(data, 0, preHash, currentHash.Length, data.Length);
                if (salt != null)
                    System.Buffer.BlockCopy(salt, 0, preHash, currentHash.Length + data.Length, salt.Length);

                currentHash = hash.ComputeHash(preHash);

                for (int i = 1; i < count; i++)
                {
                    currentHash = hash.ComputeHash(currentHash);
                }

                hashList.AddRange(currentHash);
            }
            hash.Clear();
            key = new byte[32];
            iv = new byte[16];
            hashList.CopyTo(0, key, 0, 32);
            hashList.CopyTo(32, iv, 0, 16);
        }

    }
}
