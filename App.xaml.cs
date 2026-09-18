using CoinTrace.WPF.Controllers;
using Smart_Tools.WPF.Controllers;
using Smart_Tools.WPF.Views;
using System.Windows;

namespace CoinTrace.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Apply the saved theme FIRST, before any window is created.
            // CreateAccountView/LoginView use the same DynamicResource
            // brushes (CardBg, AccentGradient, TextPrimary, ...) as
            // MainWindow, so they need the theme's resource dictionary
            // already swapped in or they render with WPF's unstyled
            // defaults instead of matching the rest of the app.
            ThemeManager.Apply(AppSettingsStore.Theme);

            var accountManager = new AccountManager();

            // First run ever: no local account exists yet, so gate on
            // creating one before anything else happens.
            if (!accountManager.HasAccount)
            {
                var createAccountView = new CreateAccountView();
                if (createAccountView.ShowDialog() != true)
                {
                    // User closed/cancelled account creation - nothing to
                    // log into yet, so there's nothing useful the app can
                    // show. Exit cleanly instead of opening MainWindow.
                    Shutdown();
                    return;
                }
                // Account just created - fall through to the login screen
                // below so the credential check always runs the same way,
                // rather than silently auto-logging the new account in.
            }

            var loginView = new LoginView();
            if (loginView.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            // Only now does the real app open.
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}