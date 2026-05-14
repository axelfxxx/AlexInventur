using System.Security.Cryptography;
using System.Text;

namespace InventurApp.Security
{
    public static class PasswordHasher
    {
        private const int Iterations = 120_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const string Prefix = "PBKDF2";

        public static string Hash(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            if (storedHash.StartsWith($"{Prefix}$", StringComparison.OrdinalIgnoreCase))
                return VerifyPbkdf2(password, storedHash);

            // Kompatibilität zu älteren Versionen: SHA256-Hex-Hash.
            var legacy = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(legacy),
                Encoding.UTF8.GetBytes(storedHash));
        }

        public static bool NeedsRehash(string storedHash) =>
            !storedHash.StartsWith($"{Prefix}$", StringComparison.OrdinalIgnoreCase);

        private static bool VerifyPbkdf2(string password, string storedHash)
        {
            var parts = storedHash.Split('$');
            if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);
                var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                    Encoding.UTF8.GetBytes(password),
                    salt,
                    iterations,
                    HashAlgorithmName.SHA256,
                    expectedHash.Length);

                return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
