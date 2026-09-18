using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows.Threading;
using CoinTrace.WPF.Models;

namespace CoinTrace.WPF.Controllers
{
    /// <summary>
    /// Loads/saves the borrower list to a JSON file under the user's
    /// AppData folder, so it survives app restarts without needing a
    /// database. Saves are debounced: an edit restarts a short timer, and
    /// the actual disk write happens once typing pauses - not on every
    /// keystroke.
    /// </summary>
    public class BorrowerRepository
    {
        private static readonly string StoreDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CoinTrace");

        private static readonly string StorePath = Path.Combine(StoreDirectory, "borrowers.json");

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true
        };

        private readonly DispatcherTimer _saveTimer;

        public ObservableCollection<Borrower> Borrowers { get; }

        /// <summary>Fired after a successful write to disk, so the view can flash a "Saved" indicator.</summary>
        public event EventHandler? Saved;

        public BorrowerRepository()
        {
            Borrowers = new ObservableCollection<Borrower>(Load());

            _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
            _saveTimer.Tick += (_, _) =>
            {
                _saveTimer.Stop();
                SaveNow();
            };

            foreach (var borrower in Borrowers)
                borrower.PropertyChanged += (_, _) => ScheduleSave();
        }

        /// <summary>Adds a new borrower and wires it into the auto-save pipeline.</summary>
        public Borrower AddNew()
        {
            var borrower = new Borrower { FullName = "New borrower" };
            borrower.PropertyChanged += (_, _) => ScheduleSave();

            Borrowers.Add(borrower);
            ScheduleSave();

            return borrower;
        }

        public void Remove(Borrower borrower)
        {
            Borrowers.Remove(borrower);
            ScheduleSave();
        }

        /// <summary>
        /// Restarts the debounce window. Called on every field edit and
        /// list change - the actual write happens once edits stop for the
        /// interval, not on each keystroke.
        /// </summary>
        private void ScheduleSave()
        {
            _saveTimer.Stop();
            _saveTimer.Start();
        }

        public void SaveNow()
        {
            try
            {
                Directory.CreateDirectory(StoreDirectory);
                string json = JsonSerializer.Serialize(Borrowers, SerializerOptions);
                File.WriteAllText(StorePath, json);
                Saved?.Invoke(this, EventArgs.Empty);
            }
            catch (IOException)
            {
                // Disk write failed (locked file, out of space, etc). The
                // in-memory list is still intact and the next edit retries
                // the save - there's nothing actionable to surface to the
                // person mid-keystroke.
            }
        }

        private static List<Borrower> Load()
        {
            try
            {
                if (!File.Exists(StorePath))
                    return new List<Borrower>();

                string json = File.ReadAllText(StorePath);
                return JsonSerializer.Deserialize<List<Borrower>>(json, SerializerOptions) ?? new List<Borrower>();
            }
            catch (Exception ex) when (ex is IOException or JsonException)
            {
                // Corrupt or unreadable file - start fresh rather than
                // crashing the app on launch.
                return new List<Borrower>();
            }
        }
    }
}
