using Smart_Tools.WPF.Properties;
using Smart_Tools.WPF.Security;

namespace Smart_Tools.WPF.Controllers
{
    /// <summary>
    /// Reads and writes the "remember me" login credentials to the app's
    /// per-user settings, encrypting both the username and password with
    /// DPAPI before they ever touch disk.
    /// </summary>
    public class LoginManager
    {
        public bool TryLoadSavedCredentials(out string username, out string password)
        {
            username = string.Empty;
            password = string.Empty;

            if (!Settings.Default.RememberMe)
                return false;

            username = CredentialProtector.Unprotect(Settings.Default.EncryptedUsername);
            password = CredentialProtector.Unprotect(Settings.Default.EncryptedPassword);

            return !string.IsNullOrEmpty(username);
        }

        public void SaveCredentials(string username, string password, bool rememberMe)
        {
            if (rememberMe)
            {
                Settings.Default.EncryptedUsername = CredentialProtector.Protect(username);
                Settings.Default.EncryptedPassword = CredentialProtector.Protect(password);
                Settings.Default.RememberMe = true;
                Settings.Default.Save();
            }
            else
            {
                ClearSavedCredentials();
            }
        }

        public void ClearSavedCredentials()
        {
            Settings.Default.EncryptedUsername = string.Empty;
            Settings.Default.EncryptedPassword = string.Empty;
            Settings.Default.RememberMe = false;
            Settings.Default.Save();
        }

        /// <summary>
        /// Placeholder check (both fields non-empty). Replace the body with
        /// your real authentication call - an API request, a local user
        /// database lookup, etc.
        /// </summary>
        public bool ValidateCredentials(string username, string password)
        {
            return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password);
        }
    }
}
