using System.Windows;
using System.Windows.Controls;
using Smart_Tools.WPF.Controllers;

namespace Smart_Tools.WPF.Views
{
    public partial class CreateAccountView : Window
    {
        private readonly AccountManager _accountManager = new();

        public string CreatedUsername { get; private set; } = string.Empty;

        public CreateAccountView()
        {
            InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;
            string secretQuestion = GetSecretQuestionText();
            string secretAnswer = SecretAnswerBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Enter a username.");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Password must be at least 8 characters.");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Passwords don't match.");
                return;
            }

            if (string.IsNullOrWhiteSpace(secretQuestion))
            {
                ShowError("Choose or type a secret question.");
                return;
            }

            if (string.IsNullOrWhiteSpace(secretAnswer))
            {
                ShowError("Enter an answer to your secret question.");
                return;
            }

            _accountManager.CreateAccount(username, password, secretQuestion, secretAnswer);

            CreatedUsername = username;
            DialogResult = true;
            Close();
        }

        private string GetSecretQuestionText()
        {
            if (SecretQuestionCombo.SelectedItem is ComboBoxItem item)
                return item.Content?.ToString()?.Trim() ?? string.Empty;

            return SecretQuestionCombo.Text?.Trim() ?? string.Empty;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
