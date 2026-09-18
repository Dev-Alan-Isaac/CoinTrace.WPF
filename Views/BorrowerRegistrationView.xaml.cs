using CoinTrace.WPF.Controllers;
using CoinTrace.WPF.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CoinTrace.WPF.Views
{
    /// <summary>
    /// Interaction logic for BorrowerRegistrationView.xaml
    /// </summary>
    public partial class BorrowerRegistrationView : UserControl
    {
        private readonly BorrowerRepository _repository = new();

        public BorrowerRegistrationView()
        {
            InitializeComponent();

            BorrowerListBox.ItemsSource = _repository.Borrowers;
            _repository.Saved += (_, _) => Dispatcher.Invoke(FlashSavedIndicator);

            UpdateEmptyState();
        }

        private void AddBorrowerButton_Click(object sender, RoutedEventArgs e)
        {
            var borrower = _repository.AddNew();
            BorrowerListBox.SelectedItem = borrower;
        }

        private void DeleteBorrowerButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailPanel.DataContext is not Borrower borrower)
                return;

            var result = MessageBox.Show(
                Window.GetWindow(this),
                $"Delete {borrower.FullName}? This can't be undone.",
                "Delete borrower",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            _repository.Remove(borrower);
        }

        private void BorrowerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DetailPanel.DataContext = BorrowerListBox.SelectedItem;
            UpdateEmptyState();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.Trim();

            BorrowerListBox.ItemsSource = string.IsNullOrEmpty(query)
                ? _repository.Borrowers
                : _repository.Borrowers.Where(b =>
                    b.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Email.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private void UpdateEmptyState()
        {
            bool hasSelection = BorrowerListBox.SelectedItem is Borrower;

            EmptyStatePanel.Visibility = hasSelection ? Visibility.Collapsed : Visibility.Visible;
            DetailScrollViewer.Visibility = hasSelection ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Briefly shows the "Saved" indicator, then fades it back out.</summary>
        private void FlashSavedIndicator()
        {
            AnimateFlash(SavedIcon);
            AnimateFlash(SavedText);
        }

        private static void AnimateFlash(UIElement element)
        {
            element.Opacity = 1;

            var animation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600),
                BeginTime = TimeSpan.FromMilliseconds(700)
            };

            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}
