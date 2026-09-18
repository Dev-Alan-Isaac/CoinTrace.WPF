using System.Diagnostics;
using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace CoinTrace.WPF.Views
{
    /// <summary>
    /// Interaction logic for ContactDeveloperView.xaml
    ///
    /// PLACEHOLDER CONTENT: EmailAddress and GitHubUrl below (and the
    /// matching text in ContactDeveloperView.xaml) need to be replaced
    /// with the real address and profile/repo link.
    /// </summary>
    public partial class ContactDeveloperView : UserControl
    {
        private const string EmailAddress = "contactalanisaac@gmail.com";
        private const string GitHubUrl = "https://github.com/Dev-Alan-Isaac";

        public ContactDeveloperView()
        {
            InitializeComponent();
        }

        private void BtnContactEmail_Click(object sender, RoutedEventArgs e) =>
            SetContactStatus(OpenUrl($"mailto:{EmailAddress}") ? null : "Couldn't open your email app - no default mail client is configured.");

        private void BtnContactGitHub_Click(object sender, RoutedEventArgs e) =>
            SetContactStatus(OpenUrl(GitHubUrl) ? null : "Couldn't open your browser to show the GitHub link.");

        private void SetContactStatus(string? message)
        {
            TxtContactStatus.Text = message ?? "";
            TxtContactStatus.Visibility = string.IsNullOrEmpty(message) ? Visibility.Collapsed : Visibility.Visible;
        }

        private static bool OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return true;
            }
            catch
            {
                // Best effort - no default mail client/browser configured
                // on this machine shouldn't crash the app over a contact link.
                return false;
            }
        }
    }
}
