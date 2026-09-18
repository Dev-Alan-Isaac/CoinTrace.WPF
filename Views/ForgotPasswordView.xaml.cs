using System;
using System.Windows;
using Smart_Tools.WPF.Controllers;

namespace Smart_Tools.WPF.Views
{
    public partial class ForgotPasswordView : Window
    {
        private readonly AccountManager _accountManager = new();

        public ForgotPasswordView()
        {
            InitializeComponent();
            Loaded += ForgotPasswordView_Loaded;
        }

        private void ForgotPasswordView_Loaded(object sender, RoutedEventArgs e)
        {
            SecretQuestionText.Text = _accountManager.SecretQuestion;

            // Defensive: shouldn't be reachable if there's no account yet,
            // but avoids showing a blank question if it somehow is.
            if (string.IsNullOrWhiteSpace(_accountManager.SecretQuestion))
            {
                MessageBox.Show(this, "No account is set up yet.", "Reset password",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = false;
                Close();
            }
        }

        private void VerifyAnswerButton_Click(object sender, RoutedEventArgs e)
        {
            var result = _accountManager.VerifySecretAnswer(AnswerBox.Text, out TimeSpan remaining);

            switch (result)
            {
                case SecretAnswerResult.Correct:
                    QuestionStep.Visibility = Visibility.Collapsed;
                    NewPasswordStep.Visibility = Visibility.Visible;
                    break;

                case SecretAnswerResult.LockedOut:
                    ShowQuestionError(LockoutMessage(remaining));
                    break;

                case SecretAnswerResult.Incorrect:
                default:
                    ShowQuestionError("That answer doesn't match.");
                    break;
            }
        }

        private void SavePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string newPassword = NewPasswordBox.Password;
            string confirmPassword = ConfirmNewPasswordBox.Password;

            if (newPassword.Length < 8)
            {
                ShowPasswordError("Password must be at least 8 characters.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowPasswordError("Passwords don't match.");
                return;
            }

            _accountManager.ResetPassword(newPassword);

            MessageBox.Show(this, "Your password has been reset. You can now sign in.", "Reset password",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }

        private static string LockoutMessage(TimeSpan remaining)
        {
            int minutes = (int)Math.Ceiling(remaining.TotalMinutes);
            return $"Too many attempts. Try again in about {minutes} minute{(minutes == 1 ? "" : "s")}.";
        }

        private void ShowQuestionError(string message)
        {
            QuestionErrorText.Text = message;
            QuestionErrorText.Visibility = Visibility.Visible;
        }

        private void ShowPasswordError(string message)
        {
            PasswordErrorText.Text = message;
            PasswordErrorText.Visibility = Visibility.Visible;
        }
    }
}
