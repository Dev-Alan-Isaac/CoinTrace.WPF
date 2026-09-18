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

            // Apply whatever theme is currently in AppSettingsStore before any
            // window shows, so the app never flashes light-then-dark on launch.
            ThemeManager.Apply(AppSettingsStore.Theme);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Catch-all save on close -- covers any in-place edits (e.g.
            // renaming a category or pattern) that don't go through one of
            // the explicit add/remove/save actions that already call
            // AppSettingsStore.Save() themselves.
            AppSettingsStore.Save();
            base.OnExit(e);
        }
    }
}
