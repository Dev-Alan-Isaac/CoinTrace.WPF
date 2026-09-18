using System;
using System.Security.Cryptography;
using System.Text;

namespace Smart_Tools.WPF.Security
{
    /// <summary>
    /// Encrypts/decrypts short strings (usernames, passwords) using the
    /// Windows Data Protection API (DPAPI), scoped to the current Windows
    /// user. The encrypted blob can only be decrypted on the same machine,
    /// by the same Windows account that created it - there's no key for
    /// this app to generate, store, or leak.
    /// </summary>
    internal static class CredentialProtector
    {
        // Extra entropy ties the blob to this specific app, on top of the
        // Windows-user scoping. It is not itself a secret - DPAPI's real
        // protection comes from the user's Windows login.
        private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("SmartTools.WPF.Login.v1");

        public static string Protect(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = ProtectedData.Protect(plainBytes, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cipherBytes);
        }

        public static string Unprotect(string cipherTextBase64)
        {
            if (string.IsNullOrEmpty(cipherTextBase64))
                return string.Empty;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherTextBase64);
                byte[] plainBytes = ProtectedData.Unprotect(cipherBytes, Entropy, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plainBytes);
            }
            catch (Exception)
            {
                // Corrupt blob, or settings copied to a different machine /
                // Windows account than the one that encrypted them.
                return string.Empty;
            }
        }
    }
}
