using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            // Створення директорії Data
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("Data");
            }

            bookings = new EntityManager<Reservation>();
            BookingsDataGrid.ItemsSource = bookings.Items;

            // Завантаження даних з JSON файлу
            string jsonDataPath = System.IO.Path.Combine("Data", "Reservations.json");

            // Перевірка наявності файлу та завантаження даних
            if (File.Exists(jsonDataPath))
            {
                try
                {
                    string fileContent = File.ReadAllText(jsonDataPath);

                    var jsonRead = JsonSerializer.Deserialize<ObservableCollection<Reservation>>(fileContent);

                    if (jsonRead != null)
                    {
                        foreach (var b in jsonRead) { bookings.Add(b); }
                    }
                    Logger.Log("Збережено", "Дані успішно завантажені при запуску програми.");
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("Немає прав на читання з файлу.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}");
                }
            }
        }

        // Метод для обробки кліку на кнопку "Додати бронювання"
        private void OpenReservationWindow_Click(object sender, RoutedEventArgs e)
        {
            AddReservationWindow addWindow = new AddReservationWindow(bookings);
            addWindow.ShowDialog();
        }
        // Метод для обробки кліку на кнопку "Видалити"
        private void DeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            var selected = BookingsDataGrid.SelectedItem as Reservation;
            if (selected != null)
            {
                MessageBoxResult result = MessageBox.Show("Видалити ці записи?", "Видалення", MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.OK) bookings.Remove(selected);
                Logger.Log("Видалено", $"Видалено запис: {selected.MovieTitle}, {selected.ShowTime}, {string.Join(", ", selected.SeatNumbers)}, {selected.Format}");
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть запис для видалення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        // Метод для обробки кліку на кнопку "Скасувати бронювання"
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
                    Logger.Log("Змінено", $"Скасовано запис: {selected.MovieTitle}, {selected.ShowTime}, {string.Join(", ", selected.SeatNumbers)}, {selected.Format}");
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
                    sortedRecords = bookings.Items.OrderBy(b => b.SeatNumbers).ToList();
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
        // Метод для обробки групування бронювань
        private void GroupRecords_Click(object sender, RoutedEventArgs e)
        {

            if (BookingsDataGrid.SelectedItems.Count < 2)
            {
                MessageBox.Show("Будь ласка, оберіть щонайменше 2 записи для об'єднання (Затисність Ctrl).", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                var selectedRecords = BookingsDataGrid.SelectedItems.Cast<Reservation>().ToList();
                Reservation combinedRecords = selectedRecords[0];

                for (int i = 1; i < selectedRecords.Count; i++)
                {
                    combinedRecords += selectedRecords[i];
                }

                foreach (var record in selectedRecords)
                {
                    bookings.Remove(record);
                }

                bookings.Add(combinedRecords);
                BookingsDataGrid.Items.Refresh();

                MessageBox.Show("Бронювання успішно об'єднано!.", "Об'єднання", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message, "Помилка об'єднання", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Метод для порівняння бронювань на час та місце
        private void CompareEquality_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItems.Count != 2)
            {
                MessageBox.Show("Будь ласка, оберіть рівно 2 записи для порівняння.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var selectedRecords = BookingsDataGrid.SelectedItems.Cast<Reservation>().ToList();
            if (selectedRecords[0] == selectedRecords[1])
            {
                MessageBox.Show("Ці записи мають однаковий час та місце.", "Порівняння", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ці записи не мають однаковий час та місце.", "Порівняння", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        // Метод для порівняння бронювань на час
        private void CompareTime_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsDataGrid.SelectedItems.Count != 2)
            {
                MessageBox.Show("Будь ласка, оберіть рівно 2 записи для порівняння.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = BookingsDataGrid.SelectedItems.Cast<Reservation>().ToList();

            if (selected[0] < selected[1])
            {
                MessageBox.Show($"Сеанс '{selected[0].MovieTitle}' відбувається раніше, ніж '{selected[1].MovieTitle}'.", "Порівняння часу (<)", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (selected[0] > selected[1])
            {
                MessageBox.Show($"Сеанс '{selected[1].MovieTitle}' відбувається раніше, ніж '{selected[0].MovieTitle}'.", "Порівняння часу (<)", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ці сеанси відбуваються одночасно.", "Порівняння часу", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Метод для збереження даних у JSON файл при закритті вікна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                string jsonData = JsonSerializer.Serialize(bookings.Items);
                string jsonPath = System.IO.Path.Combine("Data", "Reservations.json");
                File.WriteAllText(jsonPath, jsonData);
                Logger.Log("Збережено", "Збережено останні дані після закриття програми.");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Виникла помилка при доступі до файлу: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Немає прав на запис у файл.");
            }
        }

        // Метод для експорту групових бронювань у окремий JSON файл
        private void ExportGrouped_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var groupedReservations = new List<Reservation>();

                // Знаходження і збереження групових бронювань (з кількістю місць більше 1)
                foreach (var item in bookings.Items)
                {
                    if (item.SeatNumbers.Count > 1)
                    {
                        groupedReservations.Add(item);
                    }
                }
                // Експорт групових бронювань у окремий JSON файл
                if (groupedReservations.Count > 0)
                {
                    string jsonData = JsonSerializer.Serialize(groupedReservations);
                    string jsonPath = System.IO.Path.Combine("Data", "GroupedReservations.json");
                    File.WriteAllText(jsonPath, jsonData);

                    MessageBox.Show($"Успішно експортовано групових бронювань: {groupedReservations.Count}", "Експорт", MessageBoxButton.OK, MessageBoxImage.Information);
                    Logger.Log("Збережено", "Збережено інформацію про усі групові бронювання.");
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Виникла помилка при доступі до файлу: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("Немає прав на запис у файл.");
            }
        }
    }

}