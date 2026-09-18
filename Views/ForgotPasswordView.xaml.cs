using Smart_Tools.WPF.Controllers;
using System.Windows;
using System.Windows.Controls;

namespace CoinTrace.WPF.Views
{
    public partial class ForgotPasswordView : UserControl
    {
        private readonly AccountManager _accountManager = new();

        /// <summary>Raised after a successful password reset. MainWindow swaps back to LoginView.</summary>
        public event EventHandler? PasswordResetCompleted;

        /// <summary>Raised when the person backs out without resetting anything.</summary>
        public event EventHandler? BackToLoginRequested;

        public ForgotPasswordView()
        {
            InitializeComponent();
            Loaded += ForgotPasswordView_Loaded;
        }

        private void ForgotPasswordView_Loaded(object sender, RoutedEventArgs e)
        {
            // Reset to step 1 each time this control is shown, in case it's
            // being reused after a previous cancelled attempt.
            QuestionStep.Visibility = Visibility.Visible;
            NewPasswordStep.Visibility = Visibility.Collapsed;
            AnswerBox.Clear();
            QuestionErrorText.Visibility = Visibility.Collapsed;
            NewPasswordBox.Clear();
            ConfirmNewPasswordBox.Clear();
            PasswordErrorText.Visibility = Visibility.Collapsed;

            SecretQuestionText.Text = _accountManager.SecretQuestion;

            // Defensive: shouldn't be reachable if there's no account yet,
            // but avoids showing a blank question if it somehow is.
            if (string.IsNullOrWhiteSpace(_accountManager.SecretQuestion))
            {
                MessageBox.Show(Window.GetWindow(this), "No account is set up yet.", "Reset password",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                BackToLoginRequested?.Invoke(this, EventArgs.Empty);
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

            MessageBox.Show(Window.GetWindow(this), "Your password has been reset. You can now sign in.", "Reset password",
                MessageBoxButton.OK, MessageBoxImage.Information);

            PasswordResetCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void BackToLoginLink_Click(object sender, RoutedEventArgs e)
        {
            BackToLoginRequested?.Invoke(this, EventArgs.Empty);
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
