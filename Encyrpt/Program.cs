using System;

namespace Encyrpt
{
    class Program
    {
        static void Main(string[] args)
        {
           

            var data = AESEncryption.EncryptDefault("Hello World", "passphrase");
            Console.WriteLine(data);
            Console.WriteLine(AESEncryption.DecryptDefault(data, "passphrase"));
        }
    }
}
