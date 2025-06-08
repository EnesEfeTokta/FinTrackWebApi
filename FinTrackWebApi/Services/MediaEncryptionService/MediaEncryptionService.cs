using System.Security.Cryptography;
using System.Text;

namespace FinTrackWebApi.Services.MediaEncryptionService
{
    public class MediaEncryptionService : IMediaEncryptionService
    {
        private const int KeySize = 256; // AES-256
        private const int Iterations = 10000; // PBKDF2 için iterasyon sayısı

        public string GenerateRandomKey(int length = 20)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+";
            StringBuilder res = new StringBuilder();
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return res.ToString();
        }

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16]; // 128-bit salt
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string GenerateIV()
        {
            byte[] ivBytes = new byte[16]; // AES için 128-bit IV
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(ivBytes);
            }
            return Convert.ToBase64String(ivBytes);
        }

        public string HashKey(string key)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private byte[] DeriveKey(string password, byte[] salt)
        {
            using (var rfc2898 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return rfc2898.GetBytes(KeySize / 8); // 256 bit / 8 = 32 bytes
            }
        }

        public async Task EncryptFileAsync(string inputFile, string outputFile, string password, string saltString, string ivString)
        {
            byte[] salt = Convert.FromBase64String(saltString);
            byte[] iv = Convert.FromBase64String(ivString);
            byte[] key = DeriveKey(password, salt);

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = 128; // AES için blok boyutu her zaman 128 bit
                aes.Mode = CipherMode.CBC; // CBC veya GCM (GCM daha modern ve güvenli)
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = key;
                aes.IV = iv;

                using (var fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var fsOutput = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var cryptoStream = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    // IV'yi dosyanın başına yazmak yerine DB'de sakladık, bu da bir yöntem.
                    // Alternatif olarak IV'yi şifreli dosyanın başına yazabilirsiniz.
                    // fsOutput.Write(iv, 0, iv.Length); // Eğer IV'yi dosyaya yazacaksanız
                    await fsInput.CopyToAsync(cryptoStream);
                }
            }
        }

        public Stream GetDecryptedVideoStream(string inputFile, string password, string saltString, string ivString)
        {
            byte[] salt = Convert.FromBase64String(saltString);
            byte[] iv = Convert.FromBase64String(ivString);
            byte[] key = DeriveKey(password, salt);

            var fsInput = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Eğer IV'yi dosyanın başından okuyacaksanız:
            // byte[] ivFromFile = new byte[16];
            // fsInput.Read(ivFromFile, 0, ivFromFile.Length);

            Aes aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = 128;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = key;
            aes.IV = iv; // ivFromFile eğer dosyadan okuduysanız

            // CryptoStream, fsInput stream'ini de kapatacağı için
            // FileStreamResult ile dönerken dikkatli olmak lazım.
            // Stream'i olduğu gibi dönmek yerine, response'a yazarken anlık decrypt etmek daha iyi olabilir.
            // Ya da CryptoStream kapatıldığında alttaki stream'i kapatmaması için bir wrapper yazılabilir.
            // Şimdilik FileStreamResult'ın bunu yönettiğini varsayalım.
            var cryptoStream = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read);
            return cryptoStream;
        }
    }
}
