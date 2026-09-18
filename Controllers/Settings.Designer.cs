using System;
using System.Configuration;

namespace Smart_Tools.WPF.Properties
{
    // Hand-written equivalent of the file Visual Studio normally generates
    // from Properties/Settings.settings. Every credential-shaped value here
    // is either a DPAPI blob (EncryptedUsername) or a salted PBKDF2 hash
    // (PasswordHash/Salt, SecretAnswerHash/Salt) - never plaintext. See
    // Security/CredentialProtector.cs, Security/PasswordHasher.cs and
    // Controllers/AccountManager.cs.
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static readonly Settings defaultInstance =
            ((Settings)(ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default => defaultInstance;

        [UserScopedSetting]
        [DefaultSettingValue("False")]
        public bool AccountExists
        {
            get => (bool)this[nameof(AccountExists)];
            set => this[nameof(AccountExists)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string EncryptedUsername
        {
            get => (string)this[nameof(EncryptedUsername)];
            set => this[nameof(EncryptedUsername)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string PasswordHash
        {
            get => (string)this[nameof(PasswordHash)];
            set => this[nameof(PasswordHash)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string PasswordSalt
        {
            get => (string)this[nameof(PasswordSalt)];
            set => this[nameof(PasswordSalt)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string SecretQuestion
        {
            get => (string)this[nameof(SecretQuestion)];
            set => this[nameof(SecretQuestion)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string SecretAnswerHash
        {
            get => (string)this[nameof(SecretAnswerHash)];
            set => this[nameof(SecretAnswerHash)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string SecretAnswerSalt
        {
            get => (string)this[nameof(SecretAnswerSalt)];
            set => this[nameof(SecretAnswerSalt)] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int FailedAttempts
        {
            get => (int)this[nameof(FailedAttempts)];
            set => this[nameof(FailedAttempts)] = value;
        }

        // Stored as UTC. DateTime.MinValue means "no active lockout".
        [UserScopedSetting]
        [DefaultSettingValue("0001-01-01T00:00:00")]
        public DateTime LockoutUntilUtc
        {
            get => (DateTime)this[nameof(LockoutUntilUtc)];
            set => this[nameof(LockoutUntilUtc)] = value;
        }
    }
}
