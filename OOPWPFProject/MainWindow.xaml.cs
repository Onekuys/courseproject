using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OOPWPFProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EntityManager<Reservation> bookings = new EntityManager<Reservation>();
        public MainWindow()
        {
            InitializeComponent();
            bookings = new EntityManager<Reservation>();
            BookingsDataGrid.ItemsSource = bookings.Items;
        }

        private void OpenReservationWindow_Click(object sender, RoutedEventArgs e)
        {
            AddReservationWindow addWindow = new AddReservationWindow(bookings);
            addWindow.ShowDialog();
        }

        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            var selected = BookingsDataGrid.SelectedItem as Reservation;
            if (selected != null)
            {
                MessageBoxResult result = MessageBox.Show("Видалити ці записи?", "Видалення", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK) bookings.Remove(selected);
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть запис для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        private void CancelRecord_Click(Object sender, RoutedEventArgs e)
        {
            var selected = BookingsDataGrid.SelectedItem as Reservation;
            if (selected != null)
            {

                if (selected.IsCanceled)
                {
                    MessageBox.Show("Це бронювання вже скасовано!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                MessageBoxResult result = MessageBox.Show("Скасувати ці бронювання?", "Скасування", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK)
                {
                    selected.Cancel();
                    BookingsDataGrid.Items.Refresh();
                    MessageBox.Show("Бронювання успішно скасовано!", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть запис для скасування.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Метод для обробки кліку на кнопку "Сортувати"
        private void SortRecords_Click(object sender, RoutedEventArgs e)
        {
            if (bookings.Items.Count == 0)
            {
                MessageBox.Show("Список записів порожній.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string? sortOption = (SortComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            IEnumerable<Reservation> sortedRecords = null;

            switch (sortOption)
            {
                case "Назва фільму":
                    sortedRecords = bookings.Items.OrderBy(b => b.MovieTitle).ToList();
                    break;

                case "Час сеансу":
                    sortedRecords = bookings.Items.OrderBy(b => b.ShowTime).ToList();
                    break;
                case "Номер місця":
                    sortedRecords = bookings.Items.OrderBy(b => b.SeatNumber).ToList();
                    break;
            }

            if (sortedRecords != null)
            {
                bookings.Clear();
                foreach (var r in sortedRecords)
                {
                    bookings.Add(r);
                }
            }
        }
        // Метод для обробки кліку на кнопку "Пошук"
        private void SearchName_Click(object sender, RoutedEventArgs e)
        {
            string nameInput = SearchNameInput.Text;
            if (string.IsNullOrEmpty(nameInput)) { MessageBox.Show("Будь ласка, введіть назву для пошуку.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            EntityManager<Reservation>? foundBooking = new EntityManager<Reservation>();
            int foundCount = 0;

            foreach (var record in bookings.Items)
            {
                if (record.MovieTitle.ToLower() == nameInput.ToLower())
                {
                    foundBooking.Add(record);
                    foundCount++;
                }
                
            }
            if (foundCount > 0)
            {
                MessageBox.Show($"Знайдено записів: {foundCount}.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
                foundBooking.DisplayAll();
            }
            else
            {
                MessageBox.Show("Записів не знайдено.", "Пошук", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}