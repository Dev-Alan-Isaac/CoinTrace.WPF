using System;
using System.Windows;
using System.Windows.Threading;
using Smart_Tools.WPF.Controllers;

namespace Smart_Tools.WPF.Views
{
    public partial class LoginView : Window
    {
        private readonly AccountManager _accountManager = new();
        private readonly DispatcherTimer _lockoutTimer;

        /// <summary>The username that was signed in with, set once DialogResult is true.</summary>
        public string Username { get; private set; } = string.Empty;

        public LoginView()
        {
            InitializeComponent();

            _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _lockoutTimer.Tick += LockoutTimer_Tick;

            Loaded += LoginView_Loaded;
            Closed += (_, _) => _lockoutTimer.Stop();
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            UsernameBox.Text = _accountManager.GetUsername();
            UsernameBox.Focus();

            RefreshLockoutState();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ShowError("Enter a username and password.");
                return;
            }

            var result = _accountManager.TryLogin(username, password, out TimeSpan remaining);

            switch (result)
            {
                case LoginResult.Success:
                    Username = username;
                    DialogResult = true;
                    Close();
                    break;

                case LoginResult.LockedOut:
                    ApplyLockoutUi(remaining);
                    break;

                case LoginResult.InvalidCredentials:
                default:
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                    ShowError("Incorrect username or password.");
                    break;
            }
        }

        private void ForgotPasswordLink_Click(object sender, RoutedEventArgs e)
        {
            var forgotPasswordView = new ForgotPasswordView { Owner = this };
            forgotPasswordView.ShowDialog();

            // Re-check in case a reset succeeded (clears any lockout) or a
            // wrong-answer streak just tripped one.
            RefreshLockoutState();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // ---- Lockout handling ----

        private void RefreshLockoutState()
        {
            if (_accountManager.IsLockedOut(out TimeSpan remaining))
                ApplyLockoutUi(remaining);
            else
                ClearLockoutUi();
        }

        private void ApplyLockoutUi(TimeSpan remaining)
        {
            LoginButton.IsEnabled = false;
            UsernameBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;

            ShowError(LockoutMessage(remaining));

            if (!_lockoutTimer.IsEnabled)
                _lockoutTimer.Start();
        }

        private void ClearLockoutUi()
        {
            _lockoutTimer.Stop();

            LoginButton.IsEnabled = true;
            UsernameBox.IsEnabled = true;
            PasswordBox.IsEnabled = true;

            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void LockoutTimer_Tick(object? sender, EventArgs e)
        {
            if (_accountManager.IsLockedOut(out TimeSpan remaining))
                ShowError(LockoutMessage(remaining));
            else
                ClearLockoutUi();
        }

        private static string LockoutMessage(TimeSpan remaining)
        {
            var clamped = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
            return $"Too many failed attempts. Try again in {clamped:mm\\:ss}.";
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
