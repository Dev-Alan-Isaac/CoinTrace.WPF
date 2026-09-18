using CoinTrace.WPF.Controllers;
using CoinTrace.WPF.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            BorrowersGrid.ItemsSource = _repository.Borrowers;
            _repository.Borrowers.CollectionChanged += (_, _) => { UpdateEmptyState(); UpdateSummary(); };
            _repository.Saved += (_, _) => Dispatcher.Invoke(() =>
            {
                FlashSavedIndicator();
                UpdateSummary();
            });

            UpdateEmptyState();
            UpdateSummary();
        }

        // ---------------- Add borrower (modal) ----------------

        private void BtnAddBorrower_Click(object sender, RoutedEventArgs e)
        {
            TxtModalFullName.Text = string.Empty;
            TxtModalPhone.Text = string.Empty;
            TxtModalEmail.Text = string.Empty;
            TxtModalAddress.Text = string.Empty;
            TxtModalNotes.Text = string.Empty;
            CmbModalStatus.SelectedItem = BorrowerStatus.Active;
            BtnConfirmAddBorrower.IsEnabled = false;

            AddBorrowerScrim.Visibility = Visibility.Visible;
            AddBorrowerCard.Visibility = Visibility.Visible;
            TxtModalFullName.Focus();
        }

        private void AddBorrowerScrim_MouseDown(object sender, MouseButtonEventArgs e) => CloseAddBorrowerOverlay();

        private void BtnCloseAddBorrower_Click(object sender, RoutedEventArgs e) => CloseAddBorrowerOverlay();

        private void CloseAddBorrowerOverlay()
        {
            AddBorrowerScrim.Visibility = Visibility.Collapsed;
            AddBorrowerCard.Visibility = Visibility.Collapsed;
        }

        private void TxtModalFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (BtnConfirmAddBorrower == null)
                return; // fires once during InitializeComponent before the button exists

            BtnConfirmAddBorrower.IsEnabled = !string.IsNullOrWhiteSpace(TxtModalFullName.Text);
        }

        /// <summary>
        /// Creates the borrower only now - nothing was added to the
        /// repository just by opening the modal. Reuses
        /// BorrowerRepository.AddNew() (which already wires the new
        /// Borrower's PropertyChanged into the debounced auto-save) and
        /// then fills in whatever the modal collected, so no repository
        /// changes were needed for this to work.
        /// </summary>
        private void BtnConfirmAddBorrower_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtModalFullName.Text))
                return;

            var borrower = _repository.AddNew();
            borrower.FullName = TxtModalFullName.Text.Trim();
            borrower.Status = CmbModalStatus.SelectedItem is BorrowerStatus status ? status : BorrowerStatus.Active;
            borrower.PhoneNumber = TxtModalPhone.Text.Trim();
            borrower.Email = TxtModalEmail.Text.Trim();
            borrower.Address = TxtModalAddress.Text.Trim();
            borrower.Notes = TxtModalNotes.Text.Trim();

            CloseAddBorrowerOverlay();

            BorrowersGrid.SelectedItem = borrower;
            BorrowersGrid.ScrollIntoView(borrower);
        }

        // ---------------- Grid row actions ----------------

        /// <summary>Delete button embedded in a grid row - the row's Borrower is that button's DataContext.</summary>
        private void DeleteRowButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not Borrower borrower)
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

        // ---------------- Search ----------------

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.Trim();

            BorrowersGrid.ItemsSource = string.IsNullOrEmpty(query)
                ? _repository.Borrowers
                : _repository.Borrowers.Where(b =>
                    b.FullName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.PhoneNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Email.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    b.Address.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

            UpdateEmptyState();
        }

        // ---------------- Empty state / summary ----------------

        private void UpdateEmptyState()
        {
            // Only the "nothing at all" state hides the grid - an empty
            // search result still shows the (now row-less) grid rather
            // than this panel, so the search box stays reachable.
            EmptyStatePanel.Visibility = _repository.Borrowers.Count == 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void UpdateSummary()
        {
            int total = _repository.Borrowers.Count;

            if (total == 0)
            {
                SummaryPanel.Visibility = Visibility.Collapsed;
                return;
            }

            int active = _repository.Borrowers.Count(b => b.Status == BorrowerStatus.Active);
            int needsAttention = _repository.Borrowers.Count(b =>
                b.Status == BorrowerStatus.PastDue || b.Status == BorrowerStatus.Defaulted);

            TxtSummaryTotal.Text = $"{total} total";
            TxtSummaryActive.Text = $"{active} active";
            TxtSummaryAttention.Text = $"{needsAttention} need attention";
            SummaryPanel.Visibility = Visibility.Visible;
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
