using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace CoinTrace.WPF.Models
{
    public enum BorrowerStatus
    {
        Active,
        PastDue,
        PaidOff,
        Defaulted
    }

    /// <summary>
    /// A person the app is tracking a loan/expense relationship with.
    /// Implements INotifyPropertyChanged so the registration view can
    /// auto-save on every edit instead of needing an explicit Save button -
    /// BorrowerRepository subscribes to this and debounces the disk write.
    /// </summary>
    public class Borrower : INotifyPropertyChanged
    {
        private string _fullName = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _email = string.Empty;
        private string _address = string.Empty;
        private BorrowerStatus _status = BorrowerStatus.Active;
        private string _notes = string.Empty;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string FullName
        {
            get => _fullName;
            set => SetField(ref _fullName, value);
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetField(ref _phoneNumber, value);
        }

        public string Email
        {
            get => _email;
            set => SetField(ref _email, value);
        }

        public string Address
        {
            get => _address;
            set => SetField(ref _address, value);
        }

        public BorrowerStatus Status
        {
            get => _status;
            set => SetField(ref _status, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetField(ref _notes, value);
        }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

        // Derived, not persisted - drive which contact-method badges show
        // in the list and detail views.
        [JsonIgnore]
        public bool HasPhone => !string.IsNullOrWhiteSpace(PhoneNumber);

        [JsonIgnore]
        public bool HasEmail => !string.IsNullOrWhiteSpace(Email);

        [JsonIgnore]
        public bool HasAddress => !string.IsNullOrWhiteSpace(Address);

        public event PropertyChangedEventHandler? PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value))
                return;

            field = value;
            UpdatedAtUtc = DateTime.UtcNow;
            OnPropertyChanged(propertyName);

            // Contact badges depend on these fields but aren't themselves
            // bindable unless their own change is announced too.
            switch (propertyName)
            {
                case nameof(PhoneNumber):
                    OnPropertyChanged(nameof(HasPhone));
                    break;
                case nameof(Email):
                    OnPropertyChanged(nameof(HasEmail));
                    break;
                case nameof(Address):
                    OnPropertyChanged(nameof(HasAddress));
                    break;
            }
        }

        private void OnPropertyChanged(string? propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
