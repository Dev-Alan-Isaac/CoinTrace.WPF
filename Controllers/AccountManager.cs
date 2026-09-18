using System;
using Smart_Tools.WPF.Properties;
using Smart_Tools.WPF.Security;

namespace Smart_Tools.WPF.Controllers
{
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        LockedOut
    }

    public enum SecretAnswerResult
    {
        Correct,
        Incorrect,
        LockedOut
    }

    /// <summary>
    /// Owns the single local account this app protects itself with: create
    /// it once on first run, verify it on every subsequent launch, and
    /// track the failed-attempt lockout.
    ///
    /// Storage:
    /// - Username: DPAPI-encrypted (reversible - CredentialProtector), since
    ///   it's shown back to the user (e.g. pre-filled on the login screen).
    /// - Password and secret-question answer: PBKDF2 salted hashes only
    ///   (PasswordHasher) - never stored or held in memory in reversible
    ///   form, because nothing legitimate ever needs to read them back.
    ///
    /// Lockout state (failed-attempt count, lockout-expiry timestamp) is
    /// persisted to the same per-user settings, so restarting the app
    /// during a lockout does not reset it.
    /// </summary>
    public class AccountManager
    {
        private const int MaxFailedAttempts = 3;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(5);

        public bool HasAccount => Settings.Default.AccountExists;

        public string SecretQuestion => Settings.Default.SecretQuestion;

        public string GetUsername() => CredentialProtector.Unprotect(Settings.Default.EncryptedUsername);

        /// <summary>
        /// Creates the one local account this app uses. Call only when
        /// HasAccount is false (the first-run "create your account" flow).
        /// </summary>
        public void CreateAccount(string username, string password, string secretQuestion, string secretAnswer)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Password is required.", nameof(password));
            if (string.IsNullOrWhiteSpace(secretQuestion)) throw new ArgumentException("Secret question is required.", nameof(secretQuestion));
            if (string.IsNullOrWhiteSpace(secretAnswer)) throw new ArgumentException("Secret answer is required.", nameof(secretAnswer));

            PasswordHasher.Hash(password, out string pwSalt, out string pwHash);
            PasswordHasher.Hash(NormalizeAnswer(secretAnswer), out string answerSalt, out string answerHash);

            Settings.Default.EncryptedUsername = CredentialProtector.Protect(username.Trim());
            Settings.Default.PasswordSalt = pwSalt;
            Settings.Default.PasswordHash = pwHash;
            Settings.Default.SecretQuestion = secretQuestion.Trim();
            Settings.Default.SecretAnswerSalt = answerSalt;
            Settings.Default.SecretAnswerHash = answerHash;
            Settings.Default.AccountExists = true;

            ClearLockout();
            Settings.Default.Save();
        }

        /// <summary>
        /// Attempts a login. Handles the 3-attempt / 5-minute lockout
        /// itself: a wrong username or password counts as one failed
        /// attempt regardless of which field was wrong (so a bad actor
        /// can't tell username from password by which error comes back).
        /// </summary>
        public LoginResult TryLogin(string username, string password, out TimeSpan remainingLockout)
        {
            if (IsLockedOut(out remainingLockout))
                return LoginResult.LockedOut;

            bool usernameMatches = string.Equals(
                username?.Trim(), GetUsername(), StringComparison.OrdinalIgnoreCase);
            bool passwordMatches = PasswordHasher.Verify(
                password ?? string.Empty, Settings.Default.PasswordSalt, Settings.Default.PasswordHash);

            if (usernameMatches && passwordMatches)
            {
                ClearLockout();
                Settings.Default.Save();
                return LoginResult.Success;
            }

            RegisterFailedAttempt();
            return IsLockedOut(out remainingLockout) ? LoginResult.LockedOut : LoginResult.InvalidCredentials;
        }

        /// <summary>
        /// Checks the secret-question answer as part of the forgot-password
        /// flow. Shares the same failed-attempt counter and lockout as
        /// TryLogin, so guessing the secret answer is subject to the same
        /// 3-attempt / 5-minute limit as guessing the password.
        /// </summary>
        public SecretAnswerResult VerifySecretAnswer(string answer, out TimeSpan remainingLockout)
        {
            if (IsLockedOut(out remainingLockout))
                return SecretAnswerResult.LockedOut;

            bool matches = PasswordHasher.Verify(
                NormalizeAnswer(answer ?? string.Empty),
                Settings.Default.SecretAnswerSalt,
                Settings.Default.SecretAnswerHash);

            if (matches)
            {
                ClearLockout();
                Settings.Default.Save();
                return SecretAnswerResult.Correct;
            }

            RegisterFailedAttempt();
            return IsLockedOut(out remainingLockout) ? SecretAnswerResult.LockedOut : SecretAnswerResult.Incorrect;
        }

        /// <summary>
        /// Sets a new password after a successful secret-answer check.
        /// Clears any active lockout so the user can log straight in.
        /// </summary>
        public void ResetPassword(string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Password is required.", nameof(newPassword));

            PasswordHasher.Hash(newPassword, out string salt, out string hash);
            Settings.Default.PasswordSalt = salt;
            Settings.Default.PasswordHash = hash;

            ClearLockout();
            Settings.Default.Save();
        }

        public bool IsLockedOut(out TimeSpan remaining)
        {
            remaining = TimeSpan.Zero;

            if (Settings.Default.FailedAttempts < MaxFailedAttempts)
                return false;

            DateTime lockoutUntilUtc = Settings.Default.LockoutUntilUtc;
            if (lockoutUntilUtc == default)
                return false;

            TimeSpan timeLeft = lockoutUntilUtc - DateTime.UtcNow;
            if (timeLeft <= TimeSpan.Zero)
            {
                // Lockout window has passed - reset so the next attempt
                // gets a clean slate instead of an instant re-lock.
                ClearLockout();
                Settings.Default.Save();
                return false;
            }

            remaining = timeLeft;
            return true;
        }

        private void RegisterFailedAttempt()
        {
            Settings.Default.FailedAttempts++;

            if (Settings.Default.FailedAttempts >= MaxFailedAttempts)
                Settings.Default.LockoutUntilUtc = DateTime.UtcNow.Add(LockoutDuration);

            Settings.Default.Save();
        }

        private void ClearLockout()
        {
            Settings.Default.FailedAttempts = 0;
            Settings.Default.LockoutUntilUtc = default;
        }

        // Case/whitespace-insensitive so "Blue" and "blue " hash the same -
        // secret answers shouldn't fail on formatting a user won't remember.
        private static string NormalizeAnswer(string answer) => answer.Trim().ToLowerInvariant();
    }
}
