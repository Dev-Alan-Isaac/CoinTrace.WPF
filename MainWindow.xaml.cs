using CoinTrace.WPF.Controllers;
using CoinTrace.WPF.Views;
using MaterialDesignThemes.Wpf;
using Smart_Tools.WPF.Controllers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using ContactDeveloperView = CoinTrace.WPF.Views.ContactDeveloperView;
using MemorialView = CoinTrace.WPF.Views.MemorialView;

namespace CoinTrace.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Width thresholds that decide the sidebar layout.
        // Below CompactThreshold  -> icon-only sidebar (labels hidden).
        // Below HiddenThreshold   -> sidebar collapses almost fully (very narrow windows).
        private const double CompactThreshold = 700;
        private const double SidebarExpandedWidth = 240;
        private const double SidebarCompactWidth = 64;

        private bool _isCompact;

        // ---------------- Memorial easter egg ----------------
        // Not linked from any visible button - clicking the app title
        // this many times in a row, each click within the reset window
        // of the last, quietly opens the memorial page. Any pause longer
        // than the window, or any other navigation click in between,
        // resets the count back to zero.
        private const int MemorialEasterEggClickCount = 7;
        private static readonly TimeSpan MemorialEasterEggResetWindow = TimeSpan.FromSeconds(3);
        private int _titleClickCount;
        private DateTime _lastTitleClickTime;

        // Remembers whatever was on screen right before Memorial was opened,
        // so its back button can return there instead of always landing on
        // a fixed default page.
        private UIElement? _previousView;
        private string? _previousLabel;

        private readonly MemorialView _memorialView = new MemorialView();
        private readonly ContactDeveloperView _contactDeveloperView = new ContactDeveloperView();
        private readonly SettingsView _settingsView = new SettingsView();


        public MainWindow()
        {
            InitializeComponent();

            UpdateThemeToggleIcon();

            _memorialView.BackRequested += MemorialView_BackRequested;
        }


        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            bool shouldBeCompact = ActualWidth < CompactThreshold;

            if (shouldBeCompact == _isCompact)
                return;

            _isCompact = shouldBeCompact;
            ApplySidebarLayout(_isCompact);
        }

        private void BtnThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            // A quick toggle only makes sense as a binary switch - if the
            // stored preference is "Match System" (or anything else),
            // treat that the same as "currently light" so the button
            // always has an unambiguous next state to flip to. The full
            // Light/Dark/Match System choice still lives in Settings for
            // anyone who wants that third option back.
            string newTheme = AppSettingsStore.Theme == "Dark" ? "Light" : "Dark";

            AppSettingsStore.Theme = newTheme;
            AppSettingsStore.Save();
            ThemeManager.Apply(newTheme);

            UpdateThemeToggleIcon();
        }

        private void UpdateThemeToggleIcon()
        {
            IconThemeToggle.Kind = AppSettingsStore.Theme == "Dark"
                ? PackIconKind.WeatherSunny
                : PackIconKind.WeatherNight;
        }

        /// <summary>
        /// Swaps the active view into MainContentArea and updates the
        /// breadcrumb label to match, so the person always has a clear
        /// signal for which tool they're currently in.
        /// </summary>
        private void NavigateTo(UIElement view, string label)
        {
            // Capture whatever's currently showing before we replace it, so
            // Memorial's back button has somewhere real to return to.
            if (MainContentArea.Content is UIElement current && !ReferenceEquals(current, view))
            {
                _previousView = current;
                _previousLabel = TxtBreadcrumbActive.Text;
            }

            MainContentArea.Content = view;
            TxtBreadcrumbActive.Text = label;
        }

        // Sends the person back to whatever they were looking at before the
        // memorial easter egg was triggered, falling back to the default
        // page if for some reason nothing was captured.
        private void MemorialView_BackRequested(object? sender, EventArgs e)
        {

        }

        /// <summary>
        /// Toggles the sidebar between the full (icon + label) layout and the
        /// compact (icon-only) layout used on narrow windows.
        /// </summary>
        private void ApplySidebarLayout(bool compact)
        {
            SidebarColumn.Width = new GridLength(compact ? SidebarCompactWidth : SidebarExpandedWidth);

            var visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            var alignment = compact ? System.Windows.HorizontalAlignment.Center : System.Windows.HorizontalAlignment.Left;

            // App title block
            TxtAppSubtitle.Visibility = visibility;
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show(
                this,
                "Are you sure you want to close Smart Tools?",
                "Confirm Exit",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No); // default focus on "No" so an accidental Enter doesn't close it

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
                return;
            }
        }


        // ---------------- About ----------------
        private void BtnContactDeveloper_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_contactDeveloperView, "Contact Developer");
        }

        // Hidden trigger for the memorial page - deliberately not wired to
        // any visible button. Clicking the app title MemorialEasterEggClickCount
        // times in a row (each click no more than MemorialEasterEggResetWindow
        // after the last) opens the memorial view; nothing on screen hints
        // that this text is clickable at all.
        private void TxtAppTitle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DateTime now = DateTime.UtcNow;

            if (now - _lastTitleClickTime > MemorialEasterEggResetWindow)
                _titleClickCount = 0;

            _titleClickCount++;
            _lastTitleClickTime = now;

            if (_titleClickCount >= MemorialEasterEggClickCount)
            {
                _titleClickCount = 0;
                NavigateTo(_memorialView, "Memorial");
            }
        }

        // ---------------- Options ----------------
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateTo(_settingsView, "Settings");
        }

    }
}