using Smart_Tools.WPF.Controllers;
using System.Windows;

namespace Smart_Tools.WPF.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginManager _loginManager = new();

        /// <summary>The username that was signed in with, set once DialogResult is true.</summary>
        public string Username { get; private set; } = string.Empty;

        public LoginView()
        {
            InitializeComponent();
            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loginManager.TryLoadSavedCredentials(out string savedUsername, out string savedPassword))
            {
                UsernameBox.Text = savedUsername;
                PasswordBox.Password = savedPassword;
                RememberMeCheckBox.IsChecked = true;
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (!_loginManager.ValidateCredentials(username, password))
            {
                ShowError("Enter a username and password.");
                return;
            }

            // TODO: replace ValidateCredentials()'s placeholder logic with
            // your real authentication check (API call, local user store,
            // etc.) before trusting this login.

            _loginManager.SaveCredentials(username, password, RememberMeCheckBox.IsChecked == true);

            Username = username;
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
