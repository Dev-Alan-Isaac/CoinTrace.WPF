using Microsoft.Win32;
using System.Windows;
using Application = System.Windows.Application;

namespace Smart_Tools.WPF.Controllers
{
    /// <summary>
    /// Applies the "Light" / "Dark" / "Match System" resource dictionary
    /// (Themes/Light.xaml or Themes/Dark.xaml) to the running application.
    /// Every view that wants to be themeable must use DynamicResource
    /// (not StaticResource, not hardcoded hex) for the brush keys defined
    /// in those two files -- e.g. Background="{DynamicResource ContentBg}".
    /// DynamicResource is required because it re-resolves when the merged
    /// dictionary is swapped; StaticResource only resolves once at load time.
    /// </summary>
    public static class ThemeManager
    {
        // Tag key used to find and remove the previously-applied theme
        // dictionary without disturbing any other merged dictionaries
        // (e.g. control styles) that live in App.xaml.
        private const string ThemeDictionaryTagKey = "__ActiveThemeDictionary";

        /// <summary>
        /// Applies the given theme ("Light", "Dark", or "Match System").
        /// Safe to call repeatedly (e.g. every time Settings is saved).
        /// </summary>
        public static void Apply(string theme)
        {
            string resolved = theme == "Match System" ? DetectSystemTheme() : theme;
            string path = resolved == "Dark" ? "Theme/Dark.xaml" : "Theme/Light.xaml";

            var newDictionary = new ResourceDictionary { Source = new Uri(path, UriKind.Relative) };
            newDictionary[ThemeDictionaryTagKey] = true;

            var merged = Application.Current.Resources.MergedDictionaries;

            var previous = merged.FirstOrDefault(d => d.Contains(ThemeDictionaryTagKey));
            if (previous != null)
                merged.Remove(previous);

            merged.Add(newDictionary);
        }

        /// <summary>
        /// Reads the Windows "app mode" setting (Settings → Personalization →
        /// Colors → "Choose your mode"). Falls back to Light if it can't be
        /// read (older Windows builds, or the key is missing).
        /// </summary>
        private static string DetectSystemTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

                if (key?.GetValue("AppsUseLightTheme") is int useLightTheme)
                    return useLightTheme == 0 ? "Dark" : "Light";
            }
            catch
            {
                // Registry read can fail in locked-down environments -- just fall back below.
            }

            return "Light";
        }
    }
}