using Smart_Tools.WPF.Controllers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace CoinTrace.WPF.Views
{
    public partial class LoginView : UserControl
    {
        private readonly AccountManager _accountManager = new();
        private readonly DispatcherTimer _lockoutTimer;

        /// <summary>Raised once a login succeeds. MainWindow reveals the real app in response.</summary>
        public event EventHandler? LoginSucceeded;

        /// <summary>Raised when the person clicks "Forgot password?". MainWindow swaps in ForgotPasswordView.</summary>
        public event EventHandler? ForgotPasswordRequested;

        /// <summary>The username that was signed in with, set once LoginSucceeded fires.</summary>
        public string Username { get; private set; } = string.Empty;

        public LoginView()
        {
            InitializeComponent();

            _lockoutTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _lockoutTimer.Tick += LockoutTimer_Tick;

            Loaded += LoginView_Loaded;
            Unloaded += (_, _) => _lockoutTimer.Stop();
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
                    LoginSucceeded?.Invoke(this, EventArgs.Empty);
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
            ForgotPasswordRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // ---- Lockout handling ----

        /// <summary>
        /// Re-checks lockout state. Call this whenever this control becomes
        /// visible again (e.g. returning from the forgot-password flow),
        /// since a reset clears the lockout and a wrong-answer streak there
        /// can trip a new one.
        /// </summary>
        public void RefreshLockoutState()
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
