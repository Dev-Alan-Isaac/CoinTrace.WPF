using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using UserControl = System.Windows.Controls.UserControl;

namespace Smart_Tools.WPF.Views
{
    /// <summary>
    /// Interaction logic for MemorialView.xaml
    ///
    /// PLACEHOLDER CONTENT: TxtMemorialName / TxtMemorialDates /
    /// TxtMemorialMessage in MemorialView.xaml all show placeholder text -
    /// replace with the actual name, dates, and dedication message.
    ///
    /// PhotoPath / BackgroundImagePath below are both empty by default, so
    /// the view falls back to the plain flower icon and solid background
    /// it had before - set either one to a real file path (relative to the
    /// app's folder, or a full absolute path - both work) to show that
    /// image instead. Neither one crashes the view if the path is empty or
    /// the file can't be found; it just quietly falls back.
    /// </summary>
    public partial class MemorialView : UserControl
    {
        private const string PhotoPath = @"Images\Memento.jpeg";
        private const string BackgroundImagePath = @"Images\Memento-bg.png";

        private static readonly TimeSpan FadeInDuration = TimeSpan.FromMilliseconds(450);

        /// <summary>
        /// Raised when the back button is clicked. The parent window owns
        /// navigation history, so this view just asks to go back rather
        /// than deciding where "back" actually leads.
        /// </summary>
        public event EventHandler? BackRequested;

        public MemorialView()
        {
            InitializeComponent();

            bool hasPhoto = TryLoadImage(PhotoPath, bitmap =>
                EllipsePhoto.Fill = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill });
            EllipsePhoto.Visibility = hasPhoto ? Visibility.Visible : Visibility.Collapsed;
            IconFallback.Visibility = hasPhoto ? Visibility.Collapsed : Visibility.Visible;

            bool hasBackground = TryLoadImage(BackgroundImagePath, bitmap => ImgBackground.Source = bitmap);
            ImgBackground.Visibility = hasBackground ? Visibility.Visible : Visibility.Collapsed;
            BackgroundOverlay.Visibility = hasBackground ? Visibility.Visible : Visibility.Collapsed;

            // Loaded fires every time this view re-enters the visual tree
            // (i.e. every time someone navigates back to it), not just the
            // first time, so the fade-in replays on each visit.
            Loaded += (_, _) =>
            {
                var fadeIn = new DoubleAnimation(0, 1, FadeInDuration)
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                RootGrid.BeginAnimation(OpacityProperty, fadeIn);
            };
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>Loads an image from a relative or absolute path and hands
        /// it to <paramref name="apply"/> if it exists and decodes
        /// successfully; returns false (and applies nothing) otherwise -
        /// covers an empty path, a typo'd path, a deleted file, or a
        /// corrupt/unsupported image, none of which should ever crash this
        /// view.</summary>
        private static bool TryLoadImage(string path, Action<BitmapImage> apply)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            try
            {
                // Path.Combine returns the second argument as-is when it's
                // already rooted (e.g. "C:\Users\Me\photo.jpg"), so this
                // handles a full absolute path and a path relative to the
                // app's own folder (e.g. "Assets\photo.jpg") the same way.
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                if (!File.Exists(fullPath))
                    return false;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();

                apply(bitmap);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
