using System.IO;
using System.Text.Json;

namespace CoinTrace.WPF.Controllers
{
    /// <summary>
    /// App-wide settings shared across views. Persisted as JSON to
    /// %AppData%/Smart Tools/settings.json -- loaded on first access (see the
    /// static constructor below) and saved after explicit add/remove/save
    /// actions plus once more on app shutdown as a catch-all.
    /// </summary>

    public static class AppSettingsStore
    {
        // ---------------- General settings ----------------
        public static string Theme { get; set; } = "Light";
        public static string Language { get; set; } = "en";
        public static bool ShowDesktopNotifications { get; set; } = true;


        // ---------------- Persistence ----------------
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coin Trace");

        private static readonly string SettingsFilePath = Path.Combine(SettingsDirectory, "settings.json");

        static AppSettingsStore()
        {
            Load();
        }

        private static bool Load()
        {
            if (!File.Exists(SettingsFilePath))
                return false;

            string json = File.ReadAllText(SettingsFilePath);
            var dto = JsonSerializer.Deserialize<PersistedSettings>(json);
            if (dto == null)
                return false;

            Theme = dto.Theme ?? "Light";
            Language = dto.Language ?? "en";
            ShowDesktopNotifications = dto.ShowDesktopNotifications;
            return true;
        }

        /// <summary>
        /// Writes the current in-memory settings to
        /// %AppData%/Smart Tools/settings.json. Safe to call often --
        /// wired up after every explicit add/remove/save action, and once
        /// more on app shutdown (see App.xaml.cs) as a catch-all for any
        /// in-place edits (e.g. renaming a category) that don't have their
        /// own dedicated save button.
        /// </summary>
        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);

                var dto = new PersistedSettings
                {
                    Theme = Theme,
                    Language = Language,
                    ShowDesktopNotifications = ShowDesktopNotifications,
                };

                string json = JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SettingsFilePath, json);
            }
            catch
            {
                // Best-effort -- a failed save (e.g. locked-down AppData, disk
                // full) shouldn't crash the app; the in-memory settings for
                // this session are unaffected either way.
            }
        }


        private class PersistedSettings
        {
            public string? Theme { get; set; }
            public string? Language { get; set; }
            public bool ShowDesktopNotifications { get; set; }
        }
    }
}


