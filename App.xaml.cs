using CoinTrace.WPF.Controllers;
using Smart_Tools.WPF.Controllers;
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

            // Apply the saved theme before MainWindow is created, so its
            // DynamicResource brushes (and the pre-login screens it hosts
            // in MainContentArea) render themed from the first frame.
            ThemeManager.Apply(AppSettingsStore.Theme);

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();

            // Account creation / login / forgot-password are now gated
            // inside MainWindow itself (see ShowAuthScreen in
            // MainWindow.xaml.cs), swapped into MainContentArea before the
            // sidebar and breadcrumb are revealed - nothing more to do here.
        }
    }
}
