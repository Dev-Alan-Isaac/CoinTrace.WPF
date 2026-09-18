using Microsoft.Toolkit.Uwp.Notifications;
using System.IO;

namespace Smart_Tools.WPF.Controllers
{
    public class ToastNotificationManager
    {
        // Ships alongside the app (Images\app-logo.png, Copy to Output Directory)
        // - same base-directory-relative convention this project already uses
        // for ffmpeg.exe/ffprobe.exe under Engines\FFMPEG.
        private static readonly string DefaultLogoPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "app-logo.png");

        /// <summary>
        /// Displays a Windows 11 native toast notification.
        /// </summary>
        /// <param name="title">The bold headline of the notification.</param>
        /// <param name="message">The main message body.</param>
        /// <param name="imageUri">Optional absolute path or pack URI for a thumbnail image.
        /// Falls back to the app's own logo (Images\app-logo.png) when not given.</param>
        public static void ShowToast(string title, string message, string imageUri = null)
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            string logoPath = !string.IsNullOrWhiteSpace(imageUri) ? imageUri : DefaultLogoPath;

            // A toast is often fired from a background/completion callback, not
            // a place with a good way to surface "the logo file went missing" -
            // so just skip the logo rather than letting a bad path throw and
            // swallow the whole notification.
            if (File.Exists(logoPath))
            {
                builder.AddAppLogoOverride(new Uri(logoPath), ToastGenericAppLogoCrop.Circle);
            }

            builder.Show();
        }
    }
}